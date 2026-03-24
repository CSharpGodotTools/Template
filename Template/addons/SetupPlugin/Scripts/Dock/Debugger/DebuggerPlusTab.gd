@tool
# Lightweight dock that mirrors Godot debugger errors in a searchable list.
extends VBoxContainer

const DebuggerErrorScannerScript = preload("../DebuggerErrorScanner.gd")
const DebuggerErrorFormatterScript = preload("../DebuggerErrorFormatter.gd")
const DebuggerSettingsPopupScript = preload("DebuggerSettingsPopup.gd")
const DebuggerPlusSettingsScript = preload("DebuggerPlusSettings.gd")
const DETAIL_ROW_INDENT := "    "
const REFRESH_BUTTON_WIDTH := 110
const COPY_BUTTON_WIDTH := 120
const TOGGLE_BUTTON_WIDTH := 130
const TREE_MIN_HEIGHT := 200
const TREE_FONT_SIZE := 15
const TIMESTAMP_COLUMN_WIDTH := 110
const ENTRY_TOGGLE_GUTTER_X := 22.0
const CONTEXT_MENU_Y_OFFSET := 12
var _scanner: DebuggerErrorScanner
var _formatter: DebuggerErrorFormatter
var _settings
var _refresh_button: Button
var _copy_all_button: Button
var _expand_all_button: Button
var _collapse_all_button: Button
var _colors_button: Button
var _filter_edit: LineEdit
var _include_stack_trace_checkbox: CheckButton
var _use_short_type_names_checkbox: CheckButton
var _include_duplicates_checkbox: CheckButton
var _show_timestamps_checkbox: CheckButton
var _show_errors_checkbox: CheckButton
var _show_warnings_checkbox: CheckButton
var _dev_mode_checkbox: CheckButton
var _error_tree: Tree
var _tree_scroll_container: ScrollContainer
var _tree_panel_style: StyleBoxFlat
var _entry_context_menu: PopupMenu
var _settings_popup
var _all_entries: PackedStringArray = []
var _visible_entries: PackedStringArray = []
var _expanded_entries: Dictionary = {}
var _context_entry_to_copy: String = ""
var _source_path_cache: Dictionary = {}
var _editor_root: Control
var _debugger_root: Control
var _watched_debugger_trees: Array[Tree] = []
var _debugger_tree_signatures: Dictionary = {}
var _debugger_event_bridge
var _run_started_at_msec: int = 0
var _entry_timestamps_by_raw: Dictionary = {}
var _pending_error_capture_elapsed_msec: Array = []

func _ready() -> void:
	_scanner = DebuggerErrorScannerScript.new()
	_formatter = DebuggerErrorFormatterScript.new()
	_settings = DebuggerPlusSettingsScript.new()
	_create_controls()
	_load_persistent_state()
	_build_layout()
	_register_events()
	_apply_timestamp_column_visibility()
	_apply_color_theme()
	_bind_debugger_event_hooks()
	_update_dock_title()
	_refresh_errors()

func prepare_for_disable() -> void:
	_unregister_events()
	_attach_bridge_signal(false)

func _create_controls() -> void:
	_refresh_button = Button.new()
	_refresh_button.text = "Refresh"
	_refresh_button.custom_minimum_size = Vector2(REFRESH_BUTTON_WIDTH, 0)

	_copy_all_button = Button.new()
	_copy_all_button.text = "Copy All"
	_copy_all_button.custom_minimum_size = Vector2(COPY_BUTTON_WIDTH, 0)

	_expand_all_button = Button.new()
	_expand_all_button.text = "Expand All"
	_expand_all_button.custom_minimum_size = Vector2(TOGGLE_BUTTON_WIDTH, 0)

	_collapse_all_button = Button.new()
	_collapse_all_button.text = "Collapse All"
	_collapse_all_button.custom_minimum_size = Vector2(TOGGLE_BUTTON_WIDTH, 0)

	_colors_button = Button.new()
	_colors_button.text = "Settings"
	_colors_button.custom_minimum_size = Vector2(TOGGLE_BUTTON_WIDTH, 0)
	_colors_button.tooltip_text = "Open Debugger+ settings."

	_filter_edit = LineEdit.new()
	_filter_edit.placeholder_text = "Filter errors (message, source, stack)"
	_filter_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	_include_stack_trace_checkbox = CheckButton.new()
	_include_stack_trace_checkbox.text = "Stack Trace"
	_include_stack_trace_checkbox.tooltip_text = "Include stack trace details for errors."
	_include_stack_trace_checkbox.button_pressed = true

	_use_short_type_names_checkbox = CheckButton.new()
	_use_short_type_names_checkbox.text = "Short Type Names"
	_use_short_type_names_checkbox.tooltip_text = "Use compact type names in messages and stack frames."
	_use_short_type_names_checkbox.button_pressed = true

	_include_duplicates_checkbox = CheckButton.new()
	_include_duplicates_checkbox.text = "Duplicates"
	_include_duplicates_checkbox.tooltip_text = "Show duplicate entries instead of collapsing identical ones."
	_include_duplicates_checkbox.button_pressed = false

	_show_timestamps_checkbox = CheckButton.new()
	_show_timestamps_checkbox.text = "Timestamps"
	_show_timestamps_checkbox.tooltip_text = "Show elapsed timestamps for entries."
	_show_timestamps_checkbox.button_pressed = true

	_show_errors_checkbox = CheckButton.new()
	_show_errors_checkbox.text = "Errors"
	_show_errors_checkbox.tooltip_text = "Toggle visibility of errors."
	_show_errors_checkbox.button_pressed = true

	_show_warnings_checkbox = CheckButton.new()
	_show_warnings_checkbox.text = "Warnings"
	_show_warnings_checkbox.tooltip_text = "Toggle visibility of warnings."
	_show_warnings_checkbox.button_pressed = true

	_dev_mode_checkbox = CheckButton.new()
	_dev_mode_checkbox.text = "Dev"
	_dev_mode_checkbox.tooltip_text = "Enables additional debug info meant for project maintainers."
	_dev_mode_checkbox.button_pressed = false

	_error_tree = Tree.new()
	_error_tree.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_error_tree.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_error_tree.custom_minimum_size = Vector2(0, TREE_MIN_HEIGHT)
	_error_tree.select_mode = Tree.SelectMode.SELECT_ROW
	_error_tree.columns = 2
	_error_tree.set_column_expand(0, false)
	_error_tree.set_column_custom_minimum_width(0, TIMESTAMP_COLUMN_WIDTH)
	_error_tree.set_column_expand(1, true)
	_error_tree.hide_root = true
	_error_tree.auto_tooltip = false
	_error_tree.add_theme_font_size_override("font_size", TREE_FONT_SIZE)
	_error_tree.add_theme_color_override("font_color", _settings.color_tree_font)

	_tree_scroll_container = ScrollContainer.new()
	_tree_scroll_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_tree_scroll_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_tree_scroll_container.custom_minimum_size = Vector2(0, TREE_MIN_HEIGHT)
	_tree_panel_style = StyleBoxFlat.new()
	_tree_panel_style.bg_color = _settings.color_panel_background
	_tree_panel_style.border_width_left = 0
	_tree_panel_style.border_width_top = 0
	_tree_panel_style.border_width_right = 0
	_tree_panel_style.border_width_bottom = 0
	_tree_scroll_container.add_theme_stylebox_override("panel", _tree_panel_style)
	_tree_scroll_container.add_child(_error_tree)

	_entry_context_menu = PopupMenu.new()
	_entry_context_menu.add_item("Copy Error", 0)

	_settings_popup = DebuggerSettingsPopupScript.new()
	_settings_popup.color_changed.connect(_on_color_picker_changed)
	_settings_popup.reset_defaults_requested.connect(_on_reset_default_colors_requested)
	_settings_popup.colors_enabled_toggled.connect(_on_colors_enabled_toggled)
	_settings_popup.warning_prefix_case_changed.connect(_on_warning_prefix_case_changed)
	_settings_popup.error_prefix_case_changed.connect(_on_error_prefix_case_changed)

func _build_layout() -> void:
	add_theme_constant_override("separation", 8)

	var toolbar: HBoxContainer = HBoxContainer.new()
	toolbar.add_theme_constant_override("separation", 8)
	toolbar.add_child(_refresh_button)
	toolbar.add_child(_copy_all_button)
	toolbar.add_child(_expand_all_button)
	toolbar.add_child(_collapse_all_button)
	toolbar.add_child(_colors_button)

	var sections: VBoxContainer = VBoxContainer.new()
	sections.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	sections.size_flags_vertical = Control.SIZE_EXPAND_FILL
	sections.add_theme_constant_override("separation", 6)

	sections.add_child(_tree_scroll_container)

	add_child(toolbar)
	add_child(_filter_edit)
	add_child(sections)
	add_child(_entry_context_menu)
	add_child(_settings_popup)
	_settings_popup.set_option_controls([
		_include_stack_trace_checkbox,
		_use_short_type_names_checkbox,
		_include_duplicates_checkbox,
		_show_timestamps_checkbox,
		_show_errors_checkbox,
		_show_warnings_checkbox
	])
	_settings_popup.set_dev_control(_dev_mode_checkbox)
	_settings_popup.set_prefix_cases(_settings.warning_prefix_case, _settings.error_prefix_case)

func _register_events() -> void:
	_refresh_button.pressed.connect(_on_refresh_pressed)
	_copy_all_button.pressed.connect(_on_copy_all_pressed)
	_expand_all_button.pressed.connect(_on_expand_all_pressed)
	_collapse_all_button.pressed.connect(_on_collapse_all_pressed)
	_colors_button.pressed.connect(_on_colors_button_pressed)
	_filter_edit.text_changed.connect(_on_filter_text_changed)
	_include_stack_trace_checkbox.toggled.connect(_on_options_toggled)
	_use_short_type_names_checkbox.toggled.connect(_on_options_toggled)
	_include_duplicates_checkbox.toggled.connect(_on_options_toggled)
	_show_timestamps_checkbox.toggled.connect(_on_options_toggled)
	_show_errors_checkbox.toggled.connect(_on_options_toggled)
	_show_warnings_checkbox.toggled.connect(_on_options_toggled)
	_dev_mode_checkbox.toggled.connect(_on_options_toggled)
	_error_tree.item_selected.connect(_on_tree_item_selected)
	_error_tree.item_activated.connect(_on_tree_item_activated)
	_error_tree.gui_input.connect(_on_tree_gui_input)
	_entry_context_menu.id_pressed.connect(_on_entry_context_menu_id_pressed)

func _unregister_events() -> void:
	_disconnect_signal(_refresh_button, "pressed", "_on_refresh_pressed")
	_disconnect_signal(_copy_all_button, "pressed", "_on_copy_all_pressed")
	_disconnect_signal(_expand_all_button, "pressed", "_on_expand_all_pressed")
	_disconnect_signal(_collapse_all_button, "pressed", "_on_collapse_all_pressed")
	_disconnect_signal(_colors_button, "pressed", "_on_colors_button_pressed")
	_disconnect_signal(_filter_edit, "text_changed", "_on_filter_text_changed")
	_disconnect_signal(_include_stack_trace_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_use_short_type_names_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_include_duplicates_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_show_timestamps_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_show_errors_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_show_warnings_checkbox, "toggled", "_on_options_toggled")
	_disconnect_signal(_dev_mode_checkbox, "toggled", "_on_options_toggled")
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

func _on_debugger_bridge_event(event_name: StringName = &"capture", _message: String = "") -> void:
	if event_name == &"started":
		_run_started_at_msec = Time.get_ticks_msec()
		_entry_timestamps_by_raw.clear()
		_pending_error_capture_elapsed_msec.clear()
	elif event_name == &"capture":
		_pending_error_capture_elapsed_msec.append(_current_elapsed_msec(Time.get_ticks_msec()))
	_refresh_errors()

func _disconnect_signal(source: Object, signal_name: StringName, method_name: String) -> void:
	if source != null and source.is_connected(signal_name, Callable(self, method_name)):
		source.disconnect(signal_name, Callable(self, method_name))

func _refresh_errors() -> void:
	if _scanner == null or _formatter == null:
		return
	var latest_raw_entries: PackedStringArray = _collect_errors_with_scanner()
	var latest_entries: PackedStringArray = _apply_timestamps_to_entries(latest_raw_entries)
	if _show_timestamps_checkbox != null and not _show_timestamps_checkbox.button_pressed:
		latest_entries = _entries_without_timestamp(latest_entries)
	var has_changed: bool = not _packed_string_arrays_equal(_all_entries, latest_entries)
	if has_changed:
		_all_entries = latest_entries
	_apply_filter()
	if _visible_entries.is_empty():
		_show_feedback("No debugger errors found.", _settings.color_feedback_warning)
	else:
		_show_feedback("Showing %d of %d errors." % [_visible_entries.size(), _all_entries.size()], _settings.color_feedback_success)

func _apply_timestamps_to_entries(raw_entries: PackedStringArray) -> PackedStringArray:
	var now_msec: int = Time.get_ticks_msec()
	var active_occurrence_counts: Dictionary = {}
	var result: PackedStringArray = []

	for raw_entry in raw_entries:
		var key: String = raw_entry.strip_edges()
		if key.is_empty():
			continue
		if _entry_has_timestamp_prefix(raw_entry):
			result.append(raw_entry)
			continue
		var occurrence_index: int = int(active_occurrence_counts.get(key, 0))
		active_occurrence_counts[key] = occurrence_index + 1

		var timestamps: Array = []
		if _entry_timestamps_by_raw.has(key):
			timestamps = _entry_timestamps_by_raw[key] as Array
		if timestamps == null:
			timestamps = []
		while timestamps.size() <= occurrence_index:
			timestamps.append(_consume_next_entry_timestamp(now_msec))
		_entry_timestamps_by_raw[key] = timestamps
		var timestamp: String = str(timestamps[occurrence_index])
		result.append(_prefix_entry_with_timestamp(raw_entry, timestamp))

	var stale_keys: Array = []
	for existing_key in _entry_timestamps_by_raw.keys():
		if not active_occurrence_counts.has(existing_key):
			stale_keys.append(existing_key)
			continue
		var keep_count: int = int(active_occurrence_counts[existing_key])
		var existing_timestamps: Array = _entry_timestamps_by_raw[existing_key] as Array
		if existing_timestamps != null and existing_timestamps.size() > keep_count:
			existing_timestamps.resize(keep_count)
			_entry_timestamps_by_raw[existing_key] = existing_timestamps
	for stale_key in stale_keys:
		_entry_timestamps_by_raw.erase(stale_key)

	return result

func _entries_without_timestamp(entries: PackedStringArray) -> PackedStringArray:
	var stripped: PackedStringArray = []
	for entry in entries:
		stripped.append(_strip_timestamp_from_entry(entry))
	return stripped

func _strip_timestamp_from_entry(entry: String) -> String:
	if entry.is_empty():
		return entry
	var lines: PackedStringArray = entry.split("\n", false)
	if lines.is_empty():
		return entry
	var first_line: String = lines[0]
	var regex: RegEx = RegEx.new()
	if regex.compile("^\\d+:\\d{2}:\\d{2}:\\d{3}\\s+") == OK:
		lines[0] = regex.sub(first_line, "", false)
	return "\n".join(lines)

func _next_elapsed_timestamp(now_msec: int) -> String:
	var elapsed: int = _current_elapsed_msec(now_msec)
	return _format_elapsed_from_msec(elapsed)

func _consume_next_entry_timestamp(now_msec: int) -> String:
	if not _pending_error_capture_elapsed_msec.is_empty():
		var elapsed_from_capture: int = int(_pending_error_capture_elapsed_msec[0])
		_pending_error_capture_elapsed_msec.remove_at(0)
		return _format_elapsed_from_msec(elapsed_from_capture)
	return _next_elapsed_timestamp(now_msec)

func _current_elapsed_msec(now_msec: int) -> int:
	var base_msec: int = _run_started_at_msec
	if base_msec <= 0:
		base_msec = now_msec
	return maxi(0, now_msec - base_msec)

func _format_elapsed_from_msec(elapsed: int) -> String:
	var total_seconds: int = elapsed / 1000
	var hours: int = total_seconds / 3600
	var minutes: int = (total_seconds % 3600) / 60
	var seconds: int = total_seconds % 60
	var millis: int = elapsed % 1000
	return "%d:%02d:%02d:%03d" % [hours, minutes, seconds, millis]

func _prefix_entry_with_timestamp(entry: String, timestamp: String) -> String:
	if entry.is_empty():
		return entry
	var lines: PackedStringArray = entry.split("\n", false)
	if lines.is_empty():
		return "%s %s" % [timestamp, entry]
	lines[0] = "%s %s" % [timestamp, lines[0]]
	return "\n".join(lines)
func _entry_has_timestamp_prefix(entry: String) -> bool:
	if entry.is_empty():
		return false
	var first_line: String = entry.split("\n", false)[0].strip_edges()
	var regex: RegEx = RegEx.new()
	if regex.compile("^\\d+:\\d{2}:\\d{2}:\\d{3}\\b") != OK:
		return false
	return regex.search(first_line) != null

func _apply_filter() -> void:
	var needle: String = _filter_edit.text.strip_edges().to_lower()
	_visible_entries = []
	for entry in _all_entries:
		if not _should_include_entry(entry):
			continue
		if needle.is_empty() or entry.to_lower().contains(needle):
			_visible_entries.append(entry)
	_rebuild_error_tree()
	_update_dock_title()

func _should_include_entry(entry: String) -> bool:
	if _show_errors_checkbox != null and not _show_errors_checkbox.button_pressed and _entry_is_error(entry):
		return false
	if _show_warnings_checkbox != null and not _show_warnings_checkbox.button_pressed and _entry_is_warning(entry):
		return false
	return true

func _entry_is_warning(entry: String) -> bool:
	var meta_type: String = _entry_meta_type(entry)
	if meta_type == "warning":
		return true
	return _single_line_summary(entry).to_lower().contains("warning")

func _entry_is_error(entry: String) -> bool:
	var meta_type: String = _entry_meta_type(entry)
	if meta_type == "error":
		return true
	var summary: String = _single_line_summary(entry).to_lower()
	return summary.contains("error") or summary.contains("exception")

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
		_show_feedback("No errors match filter.", _settings.color_feedback_warning)
	else:
		_show_feedback("Showing %d of %d errors." % [_visible_entries.size(), _all_entries.size()], _settings.color_feedback_success)

func _on_options_toggled(_enabled: bool) -> void:
	_save_persistent_state()
	_apply_timestamp_column_visibility()
	_refresh_errors()

func _apply_timestamp_column_visibility() -> void:
	if _error_tree == null or _show_timestamps_checkbox == null:
		return
	if _show_timestamps_checkbox.button_pressed:
		_error_tree.columns = 2
		_error_tree.set_column_expand(0, false)
		_error_tree.set_column_custom_minimum_width(0, TIMESTAMP_COLUMN_WIDTH)
		_error_tree.set_column_expand(1, true)
	else:
		_error_tree.columns = 1
		_error_tree.set_column_expand(0, true)
		_error_tree.set_column_custom_minimum_width(0, 0)

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
	if mouse_event.position.x <= ENTRY_TOGGLE_GUTTER_X:
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
	_entry_context_menu.position = DisplayServer.mouse_get_position() + Vector2i(0, CONTEXT_MENU_Y_OFFSET)
	_entry_context_menu.reset_size()
	_entry_context_menu.popup()

func _on_entry_context_menu_id_pressed(id: int) -> void:
	if id != 0 or _context_entry_to_copy.is_empty():
		return
	DisplayServer.clipboard_set(_context_entry_to_copy)
	_show_feedback("Copied selected error to clipboard.", _settings.color_feedback_success)

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
		_show_feedback("No errors to copy.", _settings.color_feedback_warning)
		return
	DisplayServer.clipboard_set("\n\n".join(_visible_entries))
	_show_feedback("Copied %d errors." % _visible_entries.size(), _settings.color_feedback_success)

func _on_expand_all_pressed() -> void:
	_set_all_entries_collapsed(false)
	_show_feedback("Expanded all errors.", _settings.color_feedback_success)

func _on_collapse_all_pressed() -> void:
	_set_all_entries_collapsed(true)
	_show_feedback("Collapsed all errors.", _settings.color_feedback_success)

func _collect_errors_with_scanner() -> PackedStringArray:
	_bind_debugger_event_hooks()
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return []

	var panel_roots: Array[Control] = _scanner.find_debugger_related_tab_controls(root)
	if panel_roots.is_empty():
		return []

	var rows: PackedStringArray = []
	for panel_root in panel_roots:
		if panel_root == null:
			continue
		var targeted_rows: PackedStringArray = _collect_errors_from_known_panel(panel_root)
		if not targeted_rows.is_empty():
			rows.append_array(targeted_rows)
			continue
		var fallback_rows: PackedStringArray = _collect_errors_with_recursive_fallback(panel_root)
		if _dev_mode_checkbox != null and _dev_mode_checkbox.button_pressed and not fallback_rows.is_empty():
			push_warning("Debugger+ fallback scan used for panel: %s (recovered %d entries)" % [str(panel_root.get_path()), fallback_rows.size()])
		rows.append_array(fallback_rows)

	if _include_duplicates_checkbox != null and _include_duplicates_checkbox.button_pressed:
		return _non_empty_rows(rows)
	return _dedupe_non_empty_rows_ignoring_timestamps(rows)

func _non_empty_rows(rows: PackedStringArray) -> PackedStringArray:
	var filtered: PackedStringArray = []
	for row in rows:
		if not row.strip_edges().is_empty():
			filtered.append(row)
	return filtered

func _dedupe_non_empty_rows_ignoring_timestamps(rows: PackedStringArray) -> PackedStringArray:
	var deduped: PackedStringArray = []
	var seen_keys: Dictionary = {}
	for row in rows:
		var trimmed_row: String = row.strip_edges()
		if trimmed_row.is_empty():
			continue
		var normalized_key: String = _strip_timestamp_from_entry(trimmed_row).strip_edges()
		if normalized_key.is_empty() or seen_keys.has(normalized_key):
			continue
		seen_keys[normalized_key] = true
		deduped.append(row)
	return deduped

func _rebuild_error_tree() -> void:
	_snapshot_expanded_state()
	_error_tree.clear()
	var root: TreeItem = _error_tree.create_item()
	var show_timestamps: bool = _show_timestamps_checkbox != null and _show_timestamps_checkbox.button_pressed
	var message_column: int = 1 if show_timestamps else 0
	var detail_indent: String = DETAIL_ROW_INDENT + DETAIL_ROW_INDENT if show_timestamps else DETAIL_ROW_INDENT
	for entry in _visible_entries:
		var entry_item: TreeItem = _error_tree.create_item(root)
		var summary: String = _single_line_summary(entry)
		var parts: Dictionary = _split_timestamp_prefix(summary)
		var timestamp: String = str(parts.get("timestamp", ""))
		var message: String = str(parts.get("message", summary))
		message = _apply_prefix_to_message(message, entry)
		if show_timestamps and not timestamp.is_empty():
			entry_item.set_text(0, timestamp)
			entry_item.set_custom_color(0, _settings.color_timestamp if _settings.colors_enabled else _settings.DEFAULT_COLOR_TIMESTAMP_TEXT)
		elif show_timestamps:
			entry_item.set_text(0, "")
		entry_item.set_text(message_column, message)
		entry_item.set_custom_color(message_column, _entry_color_for_message(message, entry))
		entry_item.set_tooltip_text(0, "")
		if show_timestamps:
			entry_item.set_tooltip_text(1, "")
		entry_item.collapsed = not bool(_expanded_entries.get(entry, false))
		entry_item.set_metadata(0, {"kind": "entry", "entry": entry})

		for detail_line in _extract_detail_lines(entry):
			var detail_item: TreeItem = _error_tree.create_item(entry_item)
			detail_item.set_tooltip_text(0, "")
			if show_timestamps:
				detail_item.set_tooltip_text(1, "")
			var source_data: Dictionary = _parse_source_line(detail_line)
			if source_data.is_empty():
				var stack_source: Dictionary = _parse_stack_trace_line(detail_line)
				if stack_source.is_empty():
					detail_item.set_text(message_column, "%s%s" % [detail_indent, detail_line])
					detail_item.set_custom_color(message_column, _detail_color_for_line(detail_line))
					detail_item.set_metadata(0, {"kind": "detail", "entry": entry})
					continue
				detail_item.set_text(message_column, "%s%s" % [detail_indent, detail_line])
				detail_item.set_custom_color(message_column, _settings.color_stack_frame if _settings.colors_enabled else _settings.DEFAULT_COLOR_DETAIL_STACK_FRAME)
				detail_item.set_metadata(0, {
					"kind": "source",
					"entry": entry,
					"path": str(stack_source.get("path", "")),
					"line": int(stack_source.get("line", 1))
				})
				continue

			var path: String = str(source_data.get("path", ""))
			var line_number: int = int(source_data.get("line", 1))
			detail_item.set_text(message_column, "%sSource: %s:%d" % [detail_indent, path, line_number])
			detail_item.set_custom_color(message_column, _settings.color_source if _settings.colors_enabled else _settings.DEFAULT_COLOR_SOURCE_TEXT)
			detail_item.set_metadata(0, {
				"kind": "source",
				"entry": entry,
				"path": path,
				"line": line_number
			})

func _entry_color_for_message(message: String, entry: String = "") -> Color:
	if not _settings.colors_enabled:
		return _settings.DEFAULT_COLOR_ENTRY_DEFAULT
	if not entry.is_empty():
		var meta_type: String = _entry_meta_type(entry)
		if meta_type == "warning":
			return _settings.color_entry_warning
		if meta_type == "error":
			return _settings.color_entry_error
	var lowered: String = message.to_lower()
	if lowered.contains("warning"):
		return _settings.color_entry_warning
	if lowered.contains("nullreferenceexception") or lowered.contains("argumentnullexception"):
		return _settings.color_entry_error
	if lowered.contains("exception") or lowered.contains("error"):
		return _settings.color_entry_error
	return _settings.color_entry_default

func _apply_prefix_to_message(message: String, entry: String = "") -> String:
	var trimmed: String = message.strip_edges()
	var lowered: String = trimmed.to_lower()
	if lowered.begins_with("warning:") or lowered.begins_with("error:"):
		return message
	var meta_type: String = _entry_meta_type(entry)
	if meta_type == "warning":
		return _with_prefix(_prefix_text("warning", _settings.warning_prefix_case), message)
	if meta_type == "error":
		return _with_prefix(_prefix_text("error", _settings.error_prefix_case), message)
	if lowered.contains("warning"):
		return _with_prefix(_prefix_text("warning", _settings.warning_prefix_case), message)
	if lowered.contains("exception") or lowered.contains("error"):
		return _with_prefix(_prefix_text("error", _settings.error_prefix_case), message)
	return message

func _with_prefix(prefix: String, message: String) -> String:
	if prefix.is_empty():
		return message
	return "%s: %s" % [prefix, message]

func _prefix_text(base: String, mode: int) -> String:
	if mode == 0:
		return ""
	if mode == 1:
		return "%s%s" % [base.substr(0, 1).to_upper(), base.substr(1, base.length() - 1).to_lower()]
	if mode == 2:
		return base.to_upper()
	return base

func _entry_meta_type(entry: String) -> String:
	for line in entry.split("\n", false):
		var trimmed: String = line.strip_edges().to_lower()
		if trimmed.begins_with("__meta_type:"):
			return trimmed.trim_prefix("__meta_type:").strip_edges()
	return ""

func _on_colors_button_pressed() -> void:
	if _settings_popup == null:
		return
	_settings_popup.popup_centered_with_state(_current_color_map(), _settings.colors_enabled, _settings.warning_prefix_case, _settings.error_prefix_case)

func _on_color_picker_changed(color_key: String, color: Color) -> void:
	_set_color_by_key(color_key, color)
	_save_persistent_state()
	_apply_color_theme()

func _on_reset_default_colors_requested() -> void:
	_settings.reset_defaults()
	_include_stack_trace_checkbox.button_pressed = _settings.stack_trace
	_use_short_type_names_checkbox.button_pressed = _settings.short_type_names
	_include_duplicates_checkbox.button_pressed = _settings.duplicates
	_show_timestamps_checkbox.button_pressed = _settings.timestamps
	_show_errors_checkbox.button_pressed = _settings.errors
	_show_warnings_checkbox.button_pressed = _settings.warnings
	_dev_mode_checkbox.button_pressed = _settings.dev
	_save_persistent_state()
	_apply_timestamp_column_visibility()
	_apply_color_theme()
	_refresh_errors()
	_settings_popup.popup_centered_with_state(_current_color_map(), _settings.colors_enabled, _settings.warning_prefix_case, _settings.error_prefix_case)

func _on_warning_prefix_case_changed(mode: int) -> void:
	_settings.warning_prefix_case = clampi(mode, 0, 2)
	_save_persistent_state()
	_refresh_errors()

func _on_error_prefix_case_changed(mode: int) -> void:
	_settings.error_prefix_case = clampi(mode, 0, 2)
	_save_persistent_state()
	_refresh_errors()

func _current_color_map() -> Dictionary:
	return {
		"enabled": _settings.colors_enabled,
		"tree_font": _settings.color_tree_font,
		"panel_background": _settings.color_panel_background,
		"timestamp": _settings.color_timestamp,
		"source": _settings.color_source,
		"entry_default": _settings.color_entry_default,
		"entry_error": _settings.color_entry_error,
		"entry_warning": _settings.color_entry_warning,
		"detail_default": _settings.color_detail_default,
		"stack_header": _settings.color_stack_header,
		"stack_frame": _settings.color_stack_frame
	}

func _set_color_by_key(color_key: String, color: Color) -> void:
	match color_key:
		"tree_font":
			_settings.color_tree_font = color
		"panel_background":
			_settings.color_panel_background = color
		"timestamp":
			_settings.color_timestamp = color
		"source":
			_settings.color_source = color
		"entry_default":
			_settings.color_entry_default = color
		"entry_error":
			_settings.color_entry_error = color
		"entry_warning":
			_settings.color_entry_warning = color
		"detail_default":
			_settings.color_detail_default = color
		"stack_header":
			_settings.color_stack_header = color
		"stack_frame":
			_settings.color_stack_frame = color

func _on_colors_enabled_toggled(enabled: bool) -> void:
	_settings.colors_enabled = enabled
	_save_persistent_state()
	_apply_color_theme()

func _apply_color_theme() -> void:
	if _error_tree != null:
		_error_tree.add_theme_color_override("font_color", _settings.color_tree_font if _settings.colors_enabled else _settings.DEFAULT_COLOR_TREE_FONT)
	if _tree_panel_style != null:
		_tree_panel_style.bg_color = _settings.color_panel_background if _settings.colors_enabled else _settings.DEFAULT_COLOR_PANEL_BACKGROUND
	_rebuild_error_tree()

func _load_persistent_state() -> void:
	if _settings == null:
		return
	_settings.load()
	_include_stack_trace_checkbox.button_pressed = _settings.stack_trace
	_use_short_type_names_checkbox.button_pressed = _settings.short_type_names
	_include_duplicates_checkbox.button_pressed = _settings.duplicates
	_show_timestamps_checkbox.button_pressed = _settings.timestamps
	_show_errors_checkbox.button_pressed = _settings.errors
	_show_warnings_checkbox.button_pressed = _settings.warnings
	_dev_mode_checkbox.button_pressed = _settings.dev
	if _settings_popup != null:
		_settings_popup.set_prefix_cases(_settings.warning_prefix_case, _settings.error_prefix_case)

func _save_persistent_state() -> void:
	if _settings == null:
		return
	_settings.stack_trace = _include_stack_trace_checkbox.button_pressed
	_settings.short_type_names = _use_short_type_names_checkbox.button_pressed
	_settings.duplicates = _include_duplicates_checkbox.button_pressed
	_settings.timestamps = _show_timestamps_checkbox.button_pressed
	_settings.errors = _show_errors_checkbox.button_pressed
	_settings.warnings = _show_warnings_checkbox.button_pressed
	_settings.dev = _dev_mode_checkbox.button_pressed
	_settings.save()

func _collect_errors_from_known_panel(panel_root: Control) -> PackedStringArray:
	var panel_rows: PackedStringArray = []
	var panel_path: String = str(panel_root.get_path())
	var trees: Array[Tree] = _find_descendant_trees(panel_root)

	if panel_path.ends_with("/Debugger"):
		for tree in trees:
			var tree_path: String = str(tree.get_path())
			if tree.columns == 2 and tree_path.contains("/Errors"):
				panel_rows.append_array(_scanner.collect_tree_error_rows(tree, _formatter, _include_stack_trace_checkbox.button_pressed, _use_short_type_names_checkbox.button_pressed))
		return panel_rows

	if panel_path.ends_with("/MSBuild"):
		for tree in trees:
			var tree_path: String = str(tree.get_path())
			if tree.columns == 1 and tree_path.contains("/Problems"):
				panel_rows.append_array(_scanner.collect_tree_error_rows(tree, _formatter, _include_stack_trace_checkbox.button_pressed, _use_short_type_names_checkbox.button_pressed))
		return panel_rows

	return panel_rows

func _collect_errors_with_recursive_fallback(panel_root: Control) -> PackedStringArray:
	var rows: PackedStringArray = []
	var pending: Array[Node] = [panel_root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is Tree:
			var tree: Tree = node as Tree
			rows.append_array(_scanner.collect_tree_error_rows(tree, _formatter, _include_stack_trace_checkbox.button_pressed, _use_short_type_names_checkbox.button_pressed))
	return rows

func _find_descendant_trees(root_node: Node) -> Array[Tree]:
	var found: Array[Tree] = []
	var pending: Array[Node] = [root_node]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is Tree:
			found.append(node as Tree)
	return found

func _detail_color_for_line(detail_line: String) -> Color:
	if not _settings.colors_enabled:
		return _settings.DEFAULT_COLOR_DETAIL_DEFAULT
	var lowered: String = detail_line.to_lower()
	if lowered.begins_with("stack trace"):
		return _settings.color_stack_header
	return _settings.color_detail_default

func _split_timestamp_prefix(line: String) -> Dictionary:
	var regex: RegEx = RegEx.new()
	if regex.compile("^(\\d+:\\d{2}:\\d{2}:\\d{3})\\s+(.*)$") != OK:
		return {"timestamp": "", "message": line}
	var match: RegExMatch = regex.search(line)
	if match == null:
		return {"timestamp": "", "message": line}
	return {
		"timestamp": match.get_string(1),
		"message": match.get_string(2)
	}

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
		if trimmed.begins_with("__meta_type:"):
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
	var panel_roots: Array[Control] = _scanner.find_debugger_related_tab_controls(root)
	if panel_roots.is_empty():
		return

	var seen_tree_ids: Dictionary = {}
	for panel_root in panel_roots:
		if panel_root == null:
			continue
		var pending: Array[Node] = [panel_root]
		while not pending.is_empty():
			var node: Node = pending.pop_back()
			for child in node.get_children():
				if child is Node:
					pending.append(child)
			if node is Tree:
				var tree: Tree = node as Tree
				seen_tree_ids[tree.get_instance_id()] = true
				_watch_debugger_tree(tree)

	_prune_watched_debugger_trees(seen_tree_ids)

func _prune_watched_debugger_trees(keep_tree_ids: Dictionary) -> void:
	var retained: Array[Tree] = []
	for tree in _watched_debugger_trees:
		if tree == null or not is_instance_valid(tree):
			continue
		var tree_id: int = tree.get_instance_id()
		if keep_tree_ids.has(tree_id):
			retained.append(tree)
			continue
		_disconnect_signal(tree, "item_selected", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "item_activated", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "nothing_selected", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "minimum_size_changed", "_on_debugger_tree_changed")
		_disconnect_signal(tree, "draw", "_on_debugger_tree_draw")
		_debugger_tree_signatures.erase(tree_id)
	_watched_debugger_trees = retained

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
	if not tree.is_connected("draw", Callable(self, "_on_debugger_tree_draw")):
		tree.draw.connect(_on_debugger_tree_draw)
	_debugger_tree_signatures[tree.get_instance_id()] = _compute_tree_signature(tree)

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
		_disconnect_signal(tree, "draw", "_on_debugger_tree_draw")
	_watched_debugger_trees.clear()
	_debugger_tree_signatures.clear()

func _on_editor_tree_structure_changed(_node: Node = null) -> void:
	_bind_debugger_event_hooks()
	_refresh_errors()

func _on_debugger_tree_changed() -> void:
	_refresh_if_debugger_tree_changed()
	_refresh_errors()

func _on_debugger_tree_draw() -> void:
	_refresh_if_debugger_tree_changed()

func _refresh_if_debugger_tree_changed() -> void:
	var changed: bool = false
	for tree in _watched_debugger_trees:
		if tree == null or not is_instance_valid(tree):
			continue
		var key: int = tree.get_instance_id()
		var latest: int = _compute_tree_signature(tree)
		if int(_debugger_tree_signatures.get(key, -1)) != latest:
			_debugger_tree_signatures[key] = latest
			changed = true
	if changed:
		_refresh_errors()

func _compute_tree_signature(tree: Tree) -> int:
	if tree == null:
		return 0
	var root: TreeItem = tree.get_root()
	if root == null:
		return 0
	return _accumulate_tree_signature(root.get_first_child(), 17, 0, tree.columns)

func _accumulate_tree_signature(item: TreeItem, signature: int, depth: int, column_count: int) -> int:
	var current: TreeItem = item
	var result: int = signature
	var index: int = 0
	while current != null:
		var a: String = current.get_text(0)
		var b: String = current.get_text(1) if column_count > 1 else ""
		result = result * 31 + (a.hash() ^ (b.hash() * 7) ^ (depth * 17) ^ (index * 13))
		var child: TreeItem = current.get_first_child()
		if child != null:
			result = _accumulate_tree_signature(child, result, depth + 1, column_count)
		index += 1
		current = current.get_next()
	return result

func _packed_string_arrays_equal(left: PackedStringArray, right: PackedStringArray) -> bool:
	if left.size() != right.size():
		return false
	for index in range(left.size()):
		if left[index] != right[index]:
			return false
	return true
