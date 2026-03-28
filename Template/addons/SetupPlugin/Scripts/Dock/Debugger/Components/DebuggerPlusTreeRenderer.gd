@tool
extends RefCounted

const _DETAIL_ROW_INDENT: String = "    "

var _tree: Tree
var _entry_text
var _source_navigator
var _settings
var _expanded_entries: Dictionary = {}

func _init(tree: Tree, entry_text, source_navigator, settings) -> void:
	_tree = tree
	_entry_text = entry_text
	_source_navigator = source_navigator
	_settings = settings

func rebuild(entries: PackedStringArray, show_timestamps: bool, warning_prefix_case: int, error_prefix_case: int) -> void:
	_snapshot_expanded_state()
	_tree.clear()
	var root: TreeItem = _tree.create_item()
	var message_column: int = 1 if show_timestamps else 0
	var detail_indent: String = _DETAIL_ROW_INDENT + _DETAIL_ROW_INDENT if show_timestamps else _DETAIL_ROW_INDENT

	for entry in entries:
		var entry_item: TreeItem = _tree.create_item(root)
		var summary: String = _entry_text.single_line_summary(entry)
		var parts: Dictionary = _entry_text.split_timestamp_prefix(summary)
		var timestamp: String = str(parts.get("timestamp", ""))
		var message: String = str(parts.get("message", summary))
		message = _entry_text.apply_prefix_to_message(message, entry, warning_prefix_case, error_prefix_case)

		if show_timestamps and not timestamp.is_empty():
			entry_item.set_text(0, timestamp)
			var timestamp_color: Color = _settings.color_timestamp if _settings.colors_enabled else _settings.DEFAULT_COLOR_TIMESTAMP_TEXT
			entry_item.set_custom_color(0, timestamp_color)
		elif show_timestamps:
			entry_item.set_text(0, "")

		entry_item.set_text(message_column, message)
		entry_item.set_custom_color(message_column, _entry_text.entry_color_for_message(message, entry, _settings))
		entry_item.set_tooltip_text(0, "")
		if show_timestamps:
			entry_item.set_tooltip_text(1, "")
		entry_item.collapsed = not bool(_expanded_entries.get(entry, false))
		entry_item.set_metadata(0, {"kind": "entry", "entry": entry})

		for detail_line in _entry_text.extract_detail_lines(entry):
			_build_detail_item(entry_item, detail_line, entry, message_column, detail_indent, show_timestamps)

func set_all_entries_collapsed(collapsed: bool) -> void:
	var root: TreeItem = _tree.get_root()
	if root == null:
		return
	var item: TreeItem = root.get_first_child()
	while item != null:
		var entry: String = entry_from_tree_item(item)
		if not entry.is_empty():
			item.collapsed = collapsed
			_expanded_entries[entry] = not collapsed
		item = item.get_next()

func remember_entry_state(item: TreeItem) -> void:
	if item == null:
		return
	var entry: String = entry_from_tree_item(item)
	if not entry.is_empty():
		_expanded_entries[entry] = not item.collapsed

func entry_from_tree_item(item: TreeItem) -> String:
	if item == null:
		return ""
	var meta: Variant = item.get_metadata(0)
	if meta is Dictionary:
		return str((meta as Dictionary).get("entry", ""))
	return ""

func source_target_from_tree_item(item: TreeItem) -> Dictionary:
	if item == null:
		return {}
	var meta: Variant = item.get_metadata(0)
	if not (meta is Dictionary):
		return {}
	var data: Dictionary = meta as Dictionary
	if str(data.get("kind", "")) != "source":
		return {}
	return {"path": str(data.get("path", "")), "line": int(data.get("line", 1))}

func _build_detail_item(entry_item: TreeItem, detail_line: String, entry: String, message_column: int, detail_indent: String, show_timestamps: bool) -> void:
	var detail_item: TreeItem = _tree.create_item(entry_item)
	detail_item.set_tooltip_text(0, "")
	if show_timestamps:
		detail_item.set_tooltip_text(1, "")

	var source_data: Dictionary = _source_navigator.parse_source_line(detail_line)
	if source_data.is_empty():
		source_data = _source_navigator.parse_stack_trace_line(detail_line)

	if source_data.is_empty():
		detail_item.set_text(message_column, "%s%s" % [detail_indent, detail_line])
		detail_item.set_custom_color(message_column, _entry_text.detail_color_for_line(detail_line, _settings))
		detail_item.set_metadata(0, {"kind": "detail", "entry": entry})
		return

	var path: String = str(source_data.get("path", ""))
	var line_number: int = int(source_data.get("line", 1))
	var is_source_line: bool = detail_line.begins_with("Source:")
	var label: String = "%sSource: %s:%d" % [detail_indent, path, line_number] if is_source_line else "%s%s" % [detail_indent, detail_line]
	detail_item.set_text(message_column, label)
	var source_color: Color = _settings.color_source if _settings.colors_enabled else _settings.DEFAULT_COLOR_SOURCE_TEXT
	var stack_color: Color = _settings.color_stack_frame if _settings.colors_enabled else _settings.DEFAULT_COLOR_DETAIL_STACK_FRAME
	detail_item.set_custom_color(message_column, source_color if is_source_line else stack_color)
	detail_item.set_metadata(0, {"kind": "source", "entry": entry, "path": path, "line": line_number})

func _snapshot_expanded_state() -> void:
	var root: TreeItem = _tree.get_root()
	if root == null:
		return
	var item: TreeItem = root.get_first_child()
	while item != null:
		var entry: String = entry_from_tree_item(item)
		if not entry.is_empty():
			_expanded_entries[entry] = not item.collapsed
		item = item.get_next()
