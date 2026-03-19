@tool
# Lightweight dock that mirrors Godot debugger errors in a searchable list.
extends VBoxContainer

const REFRESH_INTERVAL_SECONDS: float = 0.75
const DebuggerErrorScannerScript = preload("../DebuggerErrorScanner.gd")
const DebuggerErrorFormatterScript = preload("../DebuggerErrorFormatter.gd")

var _scanner: DebuggerErrorScanner
var _formatter: DebuggerErrorFormatter
var _refresh_button: Button
var _copy_all_button: Button
var _expand_all_button: Button
var _collapse_all_button: Button
var _filter_edit: LineEdit
var _include_stack_trace_checkbox: CheckButton
var _use_short_type_names_checkbox: CheckButton
var _error_tree: Tree
var _tree_scroll_container: ScrollContainer
var _entry_context_menu: PopupMenu
var _all_entries: PackedStringArray = []
var _visible_entries: PackedStringArray = []
var _expanded_entries: Dictionary = {}
var _context_entry_to_copy: String = ""
var _source_path_cache: Dictionary = {}
var _editor_root: Control
var _debugger_root: Control
var _watched_debugger_trees: Array[Tree] = []
var _debugger_event_bridge

func _ready() -> void:
	_scanner = DebuggerErrorScannerScript.new()
	_formatter = DebuggerErrorFormatterScript.new()
	_create_controls()
	_build_layout()
	_register_events()
	_bind_debugger_event_hooks()
	_update_dock_title()
	_refresh_errors()

func prepare_for_disable() -> void:
	_unregister_events()
	_attach_bridge_signal(false)

func _create_controls() -> void:
	_refresh_button = Button.new()
	_refresh_button.text = "Refresh"
	_refresh_button.custom_minimum_size = Vector2(110, 0)

	_copy_all_button = Button.new()
	_copy_all_button.text = "Copy All"
	_copy_all_button.custom_minimum_size = Vector2(120, 0)

	_expand_all_button = Button.new()
	_expand_all_button.text = "Expand All"
	_expand_all_button.custom_minimum_size = Vector2(130, 0)

	_collapse_all_button = Button.new()
	_collapse_all_button.text = "Collapse All"
	_collapse_all_button.custom_minimum_size = Vector2(130, 0)

	_filter_edit = LineEdit.new()
	_filter_edit.placeholder_text = "Filter errors (message, source, stack)"
	_filter_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	_include_stack_trace_checkbox = CheckButton.new()
	_include_stack_trace_checkbox.text = "Include Stack Trace"
	_include_stack_trace_checkbox.button_pressed = true

	_use_short_type_names_checkbox = CheckButton.new()
	_use_short_type_names_checkbox.text = "Use Short Type Names"
	_use_short_type_names_checkbox.button_pressed = true

	_error_tree = Tree.new()
	_error_tree.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_error_tree.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_error_tree.custom_minimum_size = Vector2(0, 200)
	_error_tree.columns = 1
	_error_tree.hide_root = true
	_error_tree.auto_tooltip = false
	_error_tree.add_theme_font_size_override("font_size", 15)
	_error_tree.add_theme_color_override("font_color", Color(0.9, 0.9, 0.9))

	_tree_scroll_container = ScrollContainer.new()
	_tree_scroll_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_tree_scroll_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_tree_scroll_container.custom_minimum_size = Vector2(0, 200)
	var panel_style: StyleBoxFlat = StyleBoxFlat.new()
	panel_style.bg_color = Color(0.05, 0.05, 0.05)
	panel_style.border_width_left = 0
	panel_style.border_width_top = 0
	panel_style.border_width_right = 0
	panel_style.border_width_bottom = 0
	_tree_scroll_container.add_theme_stylebox_override("panel", panel_style)
	_tree_scroll_container.add_child(_error_tree)

	_entry_context_menu = PopupMenu.new()
	_entry_context_menu.add_item("Copy Error", 0)

func _build_layout() -> void:
	add_theme_constant_override("separation", 8)

	var toolbar: HBoxContainer = HBoxContainer.new()
	toolbar.add_theme_constant_override("separation", 8)
	toolbar.add_child(_refresh_button)
	toolbar.add_child(_copy_all_button)
	toolbar.add_child(_expand_all_button)
	toolbar.add_child(_collapse_all_button)

	var options_row: HBoxContainer = HBoxContainer.new()
	options_row.add_theme_constant_override("separation", 16)
	options_row.add_child(_include_stack_trace_checkbox)
	options_row.add_child(_use_short_type_names_checkbox)

	var sections: VBoxContainer = VBoxContainer.new()
	sections.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	sections.size_flags_vertical = Control.SIZE_EXPAND_FILL
	sections.add_theme_constant_override("separation", 6)

	sections.add_child(_tree_scroll_container)

	add_child(toolbar)
	add_child(_filter_edit)
	add_child(options_row)
	add_child(sections)
	add_child(_entry_context_menu)

func _register_events() -> void:
	_refresh_button.pressed.connect(_on_refresh_pressed)
	_copy_all_button.pressed.connect(_on_copy_all_pressed)
	_expand_all_button.pressed.connect(_on_expand_all_pressed)
	_collapse_all_button.pressed.connect(_on_collapse_all_pressed)
	_filter_edit.text_changed.connect(_on_filter_text_changed)
	_include_stack_trace_checkbox.toggled.connect(_on_options_toggled)
	_use_short_type_names_checkbox.toggled.connect(_on_options_toggled)
	_error_tree.item_selected.connect(_on_tree_item_selected)
	_error_tree.item_activated.connect(_on_tree_item_activated)
	_error_tree.gui_input.connect(_on_tree_gui_input)
	_entry_context_menu.id_pressed.connect(_on_entry_context_menu_id_pressed)

func _unregister_events() -> void:
	_disconnect_signal(_refresh_button, "pressed", "_on_refresh_pressed")
	_disconnect_signal(_copy_all_button, "pressed", "_on_copy_all_pressed")
	_disconnect_signal(_expand_all_button, "pressed", "_on_expand_all_pressed")
	_disconnect_signal(_collapse_all_button, "pressed", "_on_collapse_all_pressed")
	_disconnect_signal(_filter_edit, "text_changed", "_on_filter_text_changed")
	_disconnect_signal(_include_stack_trace_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_use_short_type_names_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_error_tree, "item_selected", "_on_tree_item_selected")
	_disconnect_signal(_error_tree, "item_activated", "_on_tree_item_activated")
	_disconnect_signal(_error_tree, "gui_input", "_on_tree_gui_input")
	_disconnect_signal(_entry_context_menu, "id_pressed", "_on_entry_context_menu_id_pressed")
	_unbind_debugger_event_hooks()

func attach_debugger_event_bridge(bridge: Object) -> void:
	if _debugger_event_bridge == bridge:
		return
	_attach_bridge_signal(false)
	_debugger_event_bridge = bridge
	_attach_bridge_signal(true)

func _attach_bridge_signal(connect_signal: bool) -> void:
	if _debugger_event_bridge == null:
		return
	var callback: Callable = Callable(self, "_on_debugger_bridge_event")
	if connect_signal:
		if _debugger_event_bridge.has_signal("debugger_event") and not _debugger_event_bridge.is_connected("debugger_event", callback):
			_debugger_event_bridge.connect("debugger_event", callback)
	else:
		if _debugger_event_bridge.has_signal("debugger_event") and _debugger_event_bridge.is_connected("debugger_event", callback):
			_debugger_event_bridge.disconnect("debugger_event", callback)

func _on_debugger_bridge_event() -> void:
	_refresh_errors()

func _disconnect_signal(source: Object, signal_name: StringName, method_name: String) -> void:
	if source != null and source.is_connected(signal_name, Callable(self, method_name)):
		source.disconnect(signal_name, Callable(self, method_name))

func _refresh_errors() -> void:
	if _scanner == null or _formatter == null:
		return
	var latest_entries: PackedStringArray = _collect_errors_with_scanner()
	var has_changed: bool = not _packed_string_arrays_equal(_all_entries, latest_entries)
	if has_changed:
		_all_entries = latest_entries
		_apply_filter()
	else:
		return
	if _visible_entries.is_empty():
		_show_feedback("No debugger errors found.", Color(0.9, 0.9, 0.5))
	else:
		_show_feedback("Showing %d of %d errors." % [_visible_entries.size(), _all_entries.size()], Color(0.6, 0.95, 0.6))

func _apply_filter() -> void:
	var needle: String = _filter_edit.text.strip_edges().to_lower()
	_visible_entries = []
	for entry in _all_entries:
		if needle.is_empty() or entry.to_lower().contains(needle):
			_visible_entries.append(entry)
	_rebuild_error_tree()
	_update_dock_title()

func _single_line_summary(entry: String) -> String:
	var lines: PackedStringArray = entry.split("\n", false)
	for line in lines:
		var trimmed: String = line.strip_edges()
		if not trimmed.is_empty():
			return trimmed
	var fallback: String = entry.strip_edges()
	return fallback if not fallback.is_empty() else "(Error entry)"

func _on_refresh_pressed() -> void:
	_refresh_errors()

func _on_filter_text_changed(_text: String) -> void:
	_apply_filter()
	if _visible_entries.is_empty():
		_show_feedback("No errors match filter.", Color(0.9, 0.9, 0.5))
	else:
		_show_feedback("Showing %d of %d errors." % [_visible_entries.size(), _all_entries.size()], Color(0.6, 0.95, 0.6))

func _on_options_toggled(_enabled: bool) -> void:
	_refresh_errors()

func _on_tree_item_selected() -> void:
	# Selection is used for copy/open actions; expand/collapse is handled in gui_input
	# so repeated clicks on the same selected entry can still toggle state.
	return

func _on_tree_gui_input(event: InputEvent) -> void:
	if not (event is InputEventMouseButton):
		return
	var mouse_event: InputEventMouseButton = event as InputEventMouseButton
	if not mouse_event.pressed:
		return

	if mouse_event.button_index == MOUSE_BUTTON_RIGHT:
		_show_entry_context_menu(mouse_event.position)
		return

	if mouse_event.button_index != MOUSE_BUTTON_LEFT:
		return

	var item: TreeItem = _error_tree.get_item_at_position(mouse_event.position)
	if item == null:
		return
	var meta: Variant = item.get_metadata(0)
	if not (meta is Dictionary):
		return
	var data: Dictionary = meta as Dictionary
	if str(data.get("kind", "")) != "entry":
		return

	# Avoid double-toggling when the user clicks the native arrow/indent area.
	if mouse_event.position.x <= 22.0:
		return

	item.collapsed = not item.collapsed
	var entry: String = str(data.get("entry", ""))
	if not entry.is_empty():
		_expanded_entries[entry] = not item.collapsed

func _show_entry_context_menu(position: Vector2) -> void:
	var item: TreeItem = _error_tree.get_item_at_position(position)
	if item == null:
		return
	var entry: String = _entry_from_tree_item(item)
	if entry.is_empty():
		return
	_context_entry_to_copy = entry
	_entry_context_menu.position = DisplayServer.mouse_get_position() + Vector2i(0, 12)
	_entry_context_menu.reset_size()
	_entry_context_menu.popup()

func _on_entry_context_menu_id_pressed(id: int) -> void:
	if id != 0 or _context_entry_to_copy.is_empty():
		return
	DisplayServer.clipboard_set(_context_entry_to_copy)
	_show_feedback("Copied selected error to clipboard.", Color(0.6, 0.95, 0.6))

func _on_tree_item_activated() -> void:
	var item: TreeItem = _error_tree.get_selected()
	if item == null:
		return
	var meta: Variant = item.get_metadata(0)
	if not (meta is Dictionary):
		return
	var data: Dictionary = meta as Dictionary
	if str(data.get("kind", "")) == "source":
		_open_source_in_vscode(str(data.get("path", "")), int(data.get("line", 1)))

func _on_copy_all_pressed() -> void:
	if _visible_entries.is_empty():
		_show_feedback("No errors to copy.", Color(0.9, 0.9, 0.5))
		return
	DisplayServer.clipboard_set("\n\n".join(_visible_entries))
	_show_feedback("Copied %d errors." % _visible_entries.size(), Color(0.6, 0.95, 0.6))

func _on_expand_all_pressed() -> void:
	_set_all_entries_collapsed(false)
	_show_feedback("Expanded all errors.", Color(0.6, 0.95, 0.6))

func _on_collapse_all_pressed() -> void:
	_set_all_entries_collapsed(true)
	_show_feedback("Collapsed all errors.", Color(0.6, 0.95, 0.6))

func _collect_errors_with_scanner() -> PackedStringArray:
	_bind_debugger_event_hooks()
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return []

	var debugger_root: Control = _scanner.find_debugger_tab_control(root)
	if debugger_root == null:
		return []

	var rows: PackedStringArray = []
	var pending: Array[Node] = [debugger_root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is Tree:
			var tree: Tree = node as Tree
			rows.append_array(_scanner.collect_tree_error_rows(tree, _formatter, _include_stack_trace_checkbox.button_pressed, _use_short_type_names_checkbox.button_pressed))

	return _scanner.dedupe_non_empty(rows)

func _rebuild_error_tree() -> void:
	_snapshot_expanded_state()
	_error_tree.clear()
	var root: TreeItem = _error_tree.create_item()
	for entry in _visible_entries:
		var entry_item: TreeItem = _error_tree.create_item(root)
		entry_item.set_text(0, _single_line_summary(entry))
		entry_item.set_tooltip_text(0, "")
		entry_item.collapsed = not bool(_expanded_entries.get(entry, false))
		entry_item.set_metadata(0, {"kind": "entry", "entry": entry})

		for detail_line in _extract_detail_lines(entry):
			var detail_item: TreeItem = _error_tree.create_item(entry_item)
			detail_item.set_tooltip_text(0, "")
			var source_data: Dictionary = _parse_source_line(detail_line)
			if source_data.is_empty():
				var stack_source: Dictionary = _parse_stack_trace_line(detail_line)
				if stack_source.is_empty():
					detail_item.set_text(0, detail_line)
					detail_item.set_metadata(0, {"kind": "detail", "entry": entry})
					continue
				detail_item.set_text(0, detail_line)
				detail_item.set_metadata(0, {
					"kind": "source",
					"entry": entry,
					"path": str(stack_source.get("path", "")),
					"line": int(stack_source.get("line", 1))
				})
				continue

			var path: String = str(source_data.get("path", ""))
			var line_number: int = int(source_data.get("line", 1))
			detail_item.set_text(0, "Source: %s:%d" % [path, line_number])
			detail_item.set_custom_color(0, Color(0.45, 0.75, 1.0))
			detail_item.set_metadata(0, {
				"kind": "source",
				"entry": entry,
				"path": path,
				"line": line_number
			})

func _snapshot_expanded_state() -> void:
	var root: TreeItem = _error_tree.get_root()
	if root == null:
		return

	var item: TreeItem = root.get_first_child()
	while item != null:
		var meta: Variant = item.get_metadata(0)
		if meta is Dictionary:
			var data: Dictionary = meta as Dictionary
			if str(data.get("kind", "")) == "entry":
				var entry: String = str(data.get("entry", ""))
				if not entry.is_empty():
					_expanded_entries[entry] = not item.collapsed
		item = item.get_next()

func _extract_detail_lines(entry: String) -> PackedStringArray:
	var summary: String = _single_line_summary(entry)
	var details: PackedStringArray = []
	for line in entry.split("\n", false):
		var trimmed: String = line.strip_edges()
		if trimmed.is_empty():
			continue
		if trimmed == summary and details.is_empty():
			continue
		details.append(trimmed)
	return details

func _parse_source_line(line: String) -> Dictionary:
	if not line.begins_with("Source:"):
		return {}

	var payload: String = line.trim_prefix("Source:").strip_edges()
	var separator: int = payload.rfind(":")
	if separator <= 0:
		return {}

	var path: String = payload.substr(0, separator).strip_edges()
	var line_text: String = payload.substr(separator + 1).strip_edges()
	if path.is_empty() or not line_text.is_valid_int():
		return {}

	return {"path": _resolve_source_path(path), "line": int(line_text)}

func _parse_stack_trace_line(line: String) -> Dictionary:
	var trimmed: String = line.strip_edges()
	if not trimmed.contains(":"):
		return {}

	var regex: RegEx = RegEx.new()
	if regex.compile("(?:^|\\s)([^\\s:]+\\.(?:cs|gd)):(\\d+)") != OK:
		return {}
	var match: RegExMatch = regex.search(trimmed)
	if match == null:
		return {}

	var raw_path: String = match.get_string(1).strip_edges()
	var line_text: String = match.get_string(2).strip_edges()
	if raw_path.is_empty() or not line_text.is_valid_int():
		return {}

	return {"path": _resolve_source_path(raw_path), "line": int(line_text)}

func _entry_from_tree_item(item: TreeItem) -> String:
	var meta: Variant = item.get_metadata(0)
	if meta is Dictionary:
		return str((meta as Dictionary).get("entry", ""))
	return ""

func _open_source_in_vscode(path: String, line: int) -> void:
	if path.is_empty():
		return

	var absolute_path: String = ProjectSettings.globalize_path(path)
	var target: String = "%s:%d" % [absolute_path, maxi(1, line)]
	var pid: int = OS.create_process("code", ["-g", target], false)
	if pid == -1:
		var uri_path: String = absolute_path.replace(" ", "%20")
		OS.shell_open("vscode://file/%s:%d" % [uri_path, maxi(1, line)])

func _resolve_source_path(path: String) -> String:
	if path.is_empty():
		return path
	if path.begins_with("res://"):
		return path

	if _source_path_cache.has(path):
		return str(_source_path_cache[path])

	if path.begins_with("/") and FileAccess.file_exists(path):
		var localized: String = ProjectSettings.localize_path(path)
		_source_path_cache[path] = localized
		return localized

	var file_name: String = path.get_file()
	var matches: PackedStringArray = []
	_collect_matching_files("res://", file_name, matches)
	if matches.is_empty():
		_source_path_cache[path] = path
		return path

	if matches.size() == 1:
		_source_path_cache[path] = matches[0]
		return matches[0]

	for candidate in matches:
		if candidate.ends_with(path):
			_source_path_cache[path] = candidate
			return candidate

	_source_path_cache[path] = matches[0]
	return matches[0]

func _collect_matching_files(directory_path: String, file_name: String, matches: PackedStringArray) -> void:
	var dir: DirAccess = DirAccess.open(directory_path)
	if dir == null:
		return

	dir.list_dir_begin()
	var entry_name: String = dir.get_next()
	while entry_name != "":
		if entry_name == "." or entry_name == "..":
			entry_name = dir.get_next()
			continue
		var full_path: String = directory_path.path_join(entry_name)
		if dir.current_is_dir():
			_collect_matching_files(full_path, file_name, matches)
		elif entry_name == file_name:
			matches.append(full_path)
		entry_name = dir.get_next()
	dir.list_dir_end()

func _show_feedback(text: String, color: Color) -> void:
	# Bottom feedback UI intentionally removed to maximize vertical space.
	pass

func _set_all_entries_collapsed(collapsed: bool) -> void:
	var root: TreeItem = _error_tree.get_root()
	if root == null:
		return
	var item: TreeItem = root.get_first_child()
	while item != null:
		var meta: Variant = item.get_metadata(0)
		if meta is Dictionary:
			var data: Dictionary = meta as Dictionary
			if str(data.get("kind", "")) == "entry":
				item.collapsed = collapsed
				var entry: String = str(data.get("entry", ""))
				if not entry.is_empty():
					_expanded_entries[entry] = not collapsed
		item = item.get_next()

func _update_dock_title() -> void:
	var parent_node: Node = get_parent()
	if parent_node == null:
		return
	var title_text: String = "Debugger+"
	if _visible_entries.size() > 0:
		title_text = "Debugger+ (%d)" % _visible_entries.size()
	parent_node.set("title", title_text)

func _bind_debugger_event_hooks() -> void:
	if _scanner == null:
		return

	if _editor_root == null:
		_editor_root = EditorInterface.get_base_control()
	if _editor_root != null and not _editor_root.is_connected("child_entered_tree", Callable(self, "_on_editor_tree_structure_changed")):
		_editor_root.child_entered_tree.connect(_on_editor_tree_structure_changed)

	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return
	var debugger_root_candidate: Control = _scanner.find_debugger_tab_control(root)
	if debugger_root_candidate == null:
		return

	if _debugger_root != debugger_root_candidate:
		_unwatch_debugger_trees()
		_debugger_root = debugger_root_candidate

	var pending: Array[Node] = [_debugger_root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is Tree:
			_watch_debugger_tree(node as Tree)

func _watch_debugger_tree(tree: Tree) -> void:
	for existing in _watched_debugger_trees:
		if existing == tree:
			return
	_watched_debugger_trees.append(tree)
	if not tree.is_connected("item_selected", Callable(self, "_on_debugger_tree_changed")):
		tree.item_selected.connect(_on_debugger_tree_changed)
	if not tree.is_connected("item_activated", Callable(self, "_on_debugger_tree_changed")):
		tree.item_activated.connect(_on_debugger_tree_changed)
	if not tree.is_connected("nothing_selected", Callable(self, "_on_debugger_tree_changed")):
		tree.nothing_selected.connect(_on_debugger_tree_changed)
	if not tree.is_connected("minimum_size_changed", Callable(self, "_on_debugger_tree_changed")):
		tree.minimum_size_changed.connect(_on_debugger_tree_changed)

func _unbind_debugger_event_hooks() -> void:
	if _editor_root != null:
		_disconnect_signal(_editor_root, "child_entered_tree", "_on_editor_tree_structure_changed")
	_editor_root = null
	_unwatch_debugger_trees()
	_debugger_root = null

func _unwatch_debugger_trees() -> void:
	for tree in _watched_debugger_trees:
		if tree == null or not is_instance_valid(tree):
			continue
		_disconnect_signal(tree, "item_selected", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "item_activated", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "nothing_selected", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "minimum_size_changed", "_on_debugger_tree_changed")
	_watched_debugger_trees.clear()

func _on_editor_tree_structure_changed(_node: Node = null) -> void:
	_bind_debugger_event_hooks()
	_refresh_errors()

func _on_debugger_tree_changed() -> void:
	_refresh_errors()

func _packed_string_arrays_equal(left: PackedStringArray, right: PackedStringArray) -> bool:
	if left.size() != right.size():
		return false
	for index in range(left.size()):
		if left[index] != right[index]:
			return false
	return true
