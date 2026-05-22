@tool
extends VBoxContainer

const _DEBUGGER_ERROR_SCANNER_SCRIPT: Script = preload("../DebuggerErrorScanner.gd")
const _DEBUGGER_ERROR_FORMATTER_SCRIPT: Script = preload("../DebuggerErrorFormatter.gd")
const _DEBUGGER_SETTINGS_POPUP_SCRIPT: Script = preload("DebuggerSettingsPopup.gd")
const _DEBUGGER_PLUS_SETTINGS_SCRIPT: Script = preload("DebuggerPlusSettings.gd")
const _DEBUGGER_PLUS_VIEW_SCRIPT: Script = preload("Components/DebuggerPlusView.gd")
const _DEBUGGER_PLUS_OPTIONS_STATE_SCRIPT: Script = preload("Components/DebuggerPlusOptionsState.gd")
const _DEBUGGER_PLUS_TIMESTAMP_SCRIPT: Script = preload("Components/DebuggerPlusTimestampService.gd")
const _DEBUGGER_PLUS_ENTRY_TEXT_SCRIPT: Script = preload("Components/DebuggerPlusEntryText.gd")
const _DEBUGGER_PLUS_ENTRY_COLLECTOR_SCRIPT: Script = preload("Components/DebuggerPlusEntryCollector.gd")
const _DEBUGGER_PLUS_ENTRY_FILTER_SCRIPT: Script = preload("Components/DebuggerPlusEntryFilter.gd")
const _DEBUGGER_PLUS_SOURCE_NAVIGATOR_SCRIPT: Script = preload("Components/DebuggerPlusSourceNavigator.gd")
const _DEBUGGER_PLUS_TREE_RENDERER_SCRIPT: Script = preload("Components/DebuggerPlusTreeRenderer.gd")
const _DEBUGGER_PLUS_INTERACTION_CONTROLLER_SCRIPT: Script = preload("Components/DebuggerPlusInteractionController.gd")
const _DEBUGGER_PLUS_DEBUGGER_MONITOR_SCRIPT: Script = preload("Components/DebuggerPlusDebuggerMonitor.gd")

var _dependencies: Dictionary = {}
var _scanner
var _formatter
var _settings
var _view
var _options_state
var _timestamp_service
var _entry_text
var _entry_collector
var _entry_filter
var _source_navigator
var _tree_renderer
var _interaction_controller
var _debugger_monitor
var _debugger_event_bridge
var _events_wired: bool = false

func _init(dependencies: Dictionary = {}) -> void:
	_dependencies = {
		"scanner_script": dependencies.get("scanner_script", _DEBUGGER_ERROR_SCANNER_SCRIPT),
		"formatter_script": dependencies.get("formatter_script", _DEBUGGER_ERROR_FORMATTER_SCRIPT),
		"settings_script": dependencies.get("settings_script", _DEBUGGER_PLUS_SETTINGS_SCRIPT),
		"settings_popup_script": dependencies.get("settings_popup_script", _DEBUGGER_SETTINGS_POPUP_SCRIPT),
		"view_script": dependencies.get("view_script", _DEBUGGER_PLUS_VIEW_SCRIPT),
		"options_state_script": dependencies.get("options_state_script", _DEBUGGER_PLUS_OPTIONS_STATE_SCRIPT),
		"timestamp_service_script": dependencies.get("timestamp_service_script", _DEBUGGER_PLUS_TIMESTAMP_SCRIPT),
		"entry_text_script": dependencies.get("entry_text_script", _DEBUGGER_PLUS_ENTRY_TEXT_SCRIPT),
		"entry_collector_script": dependencies.get("entry_collector_script", _DEBUGGER_PLUS_ENTRY_COLLECTOR_SCRIPT),
		"entry_filter_script": dependencies.get("entry_filter_script", _DEBUGGER_PLUS_ENTRY_FILTER_SCRIPT),
		"source_navigator_script": dependencies.get("source_navigator_script", _DEBUGGER_PLUS_SOURCE_NAVIGATOR_SCRIPT),
		"tree_renderer_script": dependencies.get("tree_renderer_script", _DEBUGGER_PLUS_TREE_RENDERER_SCRIPT),
		"interaction_controller_script": dependencies.get("interaction_controller_script", _DEBUGGER_PLUS_INTERACTION_CONTROLLER_SCRIPT),
		"debugger_monitor_script": dependencies.get("debugger_monitor_script", _DEBUGGER_PLUS_DEBUGGER_MONITOR_SCRIPT)
	}

func _ready() -> void:
	_scanner = _dependencies["scanner_script"].new()
	_formatter = _dependencies["formatter_script"].new()
	_settings = _dependencies["settings_script"].new()
	_view = _dependencies["view_script"].new(self, _dependencies["settings_popup_script"], _settings)
	_options_state = _dependencies["options_state_script"].new(_settings, _view)
	_timestamp_service = _dependencies["timestamp_service_script"].new()
	_entry_text = _dependencies["entry_text_script"].new()
	_entry_collector = _dependencies["entry_collector_script"].new(_scanner, _formatter, _timestamp_service)
	_entry_filter = _dependencies["entry_filter_script"].new(_entry_text)
	_source_navigator = _dependencies["source_navigator_script"].new()
	_tree_renderer = _dependencies["tree_renderer_script"].new(_view.get_error_tree(), _entry_text, _source_navigator, _settings)
	_interaction_controller = _dependencies["interaction_controller_script"].new(_view, _tree_renderer, _source_navigator, _entry_filter)
	_debugger_monitor = _dependencies["debugger_monitor_script"].new(_scanner)
	_options_state.load()
	_view.apply_timestamp_column_visibility(_options_state.show_timestamps())
	_view.apply_color_theme(_options_state.get_settings())
	_wire_events(true)
	_debugger_monitor.bind()
	_refresh_errors()

func prepare_for_disable() -> void:
	_wire_events(false)
	_attach_bridge_signal(false)
	if _debugger_monitor != null:
		_debugger_monitor.unbind()

func attach_debugger_event_bridge(bridge: Object) -> void:
	if _debugger_event_bridge == bridge:
		return
	_attach_bridge_signal(false)
	_debugger_event_bridge = bridge
	_attach_bridge_signal(true)

func _wire_events(connect_events: bool) -> void:
	if _events_wired == connect_events:
		return
	_events_wired = connect_events
	_set_signal_connected(_view.get_button(&"refresh"), &"pressed", Callable(self, "_on_refresh_pressed"), connect_events)
	_set_signal_connected(_view.get_button(&"copy_all"), &"pressed", Callable(_interaction_controller, "on_copy_all_pressed"), connect_events)
	_set_signal_connected(_view.get_button(&"expand_all"), &"pressed", Callable(_interaction_controller, "on_expand_all_pressed"), connect_events)
	_set_signal_connected(_view.get_button(&"collapse_all"), &"pressed", Callable(_interaction_controller, "on_collapse_all_pressed"), connect_events)
	_set_signal_connected(_view.get_button(&"settings"), &"pressed", Callable(self, "_on_settings_button_pressed"), connect_events)
	_set_signal_connected(_view.get_filter_edit(), &"text_changed", Callable(self, "_on_filter_text_changed"), connect_events)
	for checkbox in _view.get_option_checkboxes():
		_set_signal_connected(checkbox, &"toggled", Callable(self, "_on_options_toggled"), connect_events)
	_set_signal_connected(_view.get_checkbox(&"dev"), &"toggled", Callable(self, "_on_options_toggled"), connect_events)
	_set_signal_connected(_view.get_error_tree(), &"item_activated", Callable(_interaction_controller, "on_tree_item_activated"), connect_events)
	_set_signal_connected(_view.get_error_tree(), &"gui_input", Callable(_interaction_controller, "on_tree_gui_input"), connect_events)
	_set_signal_connected(_view.get_entry_context_menu(), &"id_pressed", Callable(_interaction_controller, "on_entry_context_menu_id_pressed"), connect_events)
	_set_signal_connected(_view.get_settings_popup(), &"color_changed", Callable(self, "_on_color_picker_changed"), connect_events)
	_set_signal_connected(_view.get_settings_popup(), &"reset_defaults_requested", Callable(self, "_on_reset_default_colors_requested"), connect_events)
	_set_signal_connected(_view.get_settings_popup(), &"colors_enabled_toggled", Callable(self, "_on_colors_enabled_toggled"), connect_events)
	_set_signal_connected(_view.get_settings_popup(), &"warning_prefix_case_changed", Callable(self, "_on_warning_prefix_case_changed"), connect_events)
	_set_signal_connected(_view.get_settings_popup(), &"error_prefix_case_changed", Callable(self, "_on_error_prefix_case_changed"), connect_events)
	_set_signal_connected(_debugger_monitor, &"debugger_tree_changed", Callable(self, "_on_debugger_tree_changed"), connect_events)

func _refresh_errors() -> void:
	var raw_entries: PackedStringArray = _entry_collector.collect_entries(_options_state.include_stack_trace(), _options_state.use_short_type_names(), _options_state.include_duplicates(), _options_state.dev_mode())
	var entries: PackedStringArray = _timestamp_service.apply_timestamps(raw_entries)
	if not _options_state.show_timestamps():
		entries = _timestamp_service.entries_without_timestamp(entries)
	_entry_filter.set_entries(entries)
	_apply_filter()

func _apply_filter() -> void:
	_entry_filter.set_filter_text(_view.get_filter_edit().text)
	var visible_entries: PackedStringArray = _entry_filter.apply(_options_state.show_errors(), _options_state.show_warnings())
	_tree_renderer.rebuild(visible_entries, _options_state.show_timestamps(), _options_state.warning_prefix_case(), _options_state.error_prefix_case())
	_view.update_dock_title(_entry_filter.visible_count())

func _on_refresh_pressed() -> void:
	_refresh_errors()

func _on_filter_text_changed(_text: String) -> void:
	_apply_filter()

func _on_options_toggled(_enabled: bool) -> void:
	_options_state.save()
	_view.apply_timestamp_column_visibility(_options_state.show_timestamps())
	_refresh_errors()

func _on_settings_button_pressed() -> void:
	var popup_state: Dictionary = _options_state.open_popup_state()
	_view.get_settings_popup().call("popup_centered_with_state", popup_state["colors"], popup_state["colors_enabled"], popup_state["warning_case"], popup_state["error_case"])

func _on_color_picker_changed(color_key: String, color: Color) -> void:
	_options_state.set_color_by_key(color_key, color)
	_view.apply_color_theme(_options_state.get_settings())
	_apply_filter()

func _on_reset_default_colors_requested() -> void:
	_options_state.reset_defaults()
	_options_state.save()
	_view.apply_timestamp_column_visibility(_options_state.show_timestamps())
	_view.apply_color_theme(_options_state.get_settings())
	_refresh_errors()
	_on_settings_button_pressed()

func _on_colors_enabled_toggled(enabled: bool) -> void:
	_options_state.set_colors_enabled(enabled)
	_view.apply_color_theme(_options_state.get_settings())
	_apply_filter()

func _on_warning_prefix_case_changed(mode: int) -> void:
	_options_state.set_warning_prefix_case(mode)
	_refresh_errors()

func _on_error_prefix_case_changed(mode: int) -> void:
	_options_state.set_error_prefix_case(mode)
	_refresh_errors()

func _on_debugger_tree_changed() -> void:
	_refresh_errors()

func _on_debugger_bridge_event(event_name: StringName = &"capture", _message: String = "") -> void:
	_timestamp_service.on_debugger_event(event_name)
	_refresh_errors()

func _attach_bridge_signal(connect_signal: bool) -> void:
	_set_signal_connected(_debugger_event_bridge, &"debugger_event", Callable(self, "_on_debugger_bridge_event"), connect_signal)

func _set_signal_connected(source: Object, signal_name: StringName, handler: Callable, connect_signal: bool) -> void:
	if source == null or not source.has_signal(signal_name):
		return
	if connect_signal:
		if not source.is_connected(signal_name, handler):
			source.connect(signal_name, handler)
		return
	if source.is_connected(signal_name, handler):
		source.disconnect(signal_name, handler)
