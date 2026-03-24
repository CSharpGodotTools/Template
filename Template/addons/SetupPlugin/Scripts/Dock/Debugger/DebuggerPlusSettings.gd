@tool
extends RefCounted

const SETTINGS_PREFIX := "setup_plugin/debugger_plus/"

const DEFAULT_COLOR_TREE_FONT := Color(0.9, 0.9, 0.9)
const DEFAULT_COLOR_PANEL_BACKGROUND := Color(0.05, 0.05, 0.05)
const DEFAULT_COLOR_FEEDBACK_WARNING := Color(0.9, 0.9, 0.9)
const DEFAULT_COLOR_FEEDBACK_SUCCESS := Color(0.9, 0.9, 0.9)
const DEFAULT_COLOR_TIMESTAMP_TEXT := Color(0.68, 0.68, 0.68)
const DEFAULT_COLOR_SOURCE_TEXT := Color(0.45, 0.75, 1.0)
const DEFAULT_COLOR_ENTRY_DEFAULT := Color(0.9, 0.9, 0.9)
const DEFAULT_COLOR_ENTRY_ERROR := Color(0.95, 0.35, 0.35)
const DEFAULT_COLOR_ENTRY_WARNING := Color(1.0, 0.8, 0.5)
const DEFAULT_COLOR_DETAIL_DEFAULT := Color(0.9, 0.9, 0.9)
const DEFAULT_COLOR_DETAIL_STACK_HEADER := Color(0.9, 0.9, 0.9)
const DEFAULT_COLOR_DETAIL_STACK_FRAME := Color(0.9, 0.9, 0.9)

var stack_trace: bool = true
var short_type_names: bool = true
var duplicates: bool = false
var timestamps: bool = true
var errors: bool = true
var warnings: bool = true
var dev: bool = false
var colors_enabled: bool = true
var warning_prefix_case: int = 1
var error_prefix_case: int = 1

var color_tree_font: Color = DEFAULT_COLOR_TREE_FONT
var color_panel_background: Color = DEFAULT_COLOR_PANEL_BACKGROUND
var color_feedback_warning: Color = DEFAULT_COLOR_FEEDBACK_WARNING
var color_feedback_success: Color = DEFAULT_COLOR_FEEDBACK_SUCCESS
var color_timestamp: Color = DEFAULT_COLOR_TIMESTAMP_TEXT
var color_source: Color = DEFAULT_COLOR_SOURCE_TEXT
var color_entry_default: Color = DEFAULT_COLOR_ENTRY_DEFAULT
var color_entry_error: Color = DEFAULT_COLOR_ENTRY_ERROR
var color_entry_warning: Color = DEFAULT_COLOR_ENTRY_WARNING
var color_detail_default: Color = DEFAULT_COLOR_DETAIL_DEFAULT
var color_stack_header: Color = DEFAULT_COLOR_DETAIL_STACK_HEADER
var color_stack_frame: Color = DEFAULT_COLOR_DETAIL_STACK_FRAME

func load() -> void:
	var settings: EditorSettings = _editor_settings()
	if settings == null:
		return
	var defaults: Dictionary = _to_state_dict()
	for key in defaults.keys():
		var full_key: String = SETTINGS_PREFIX + str(key)
		if settings.has_setting(full_key):
			defaults[key] = settings.get_setting(full_key)
	_apply_state_dict(defaults)

func save() -> void:
	var settings: EditorSettings = _editor_settings()
	if settings == null:
		return
	var values: Dictionary = _to_state_dict()
	for key in values.keys():
		settings.set_setting(SETTINGS_PREFIX + str(key), values[key])

func reset_defaults() -> void:
	stack_trace = true
	short_type_names = true
	duplicates = false
	timestamps = true
	errors = true
	warnings = true
	dev = false
	colors_enabled = true
	warning_prefix_case = 1
	error_prefix_case = 1

	color_tree_font = DEFAULT_COLOR_TREE_FONT
	color_panel_background = DEFAULT_COLOR_PANEL_BACKGROUND
	color_feedback_warning = DEFAULT_COLOR_FEEDBACK_WARNING
	color_feedback_success = DEFAULT_COLOR_FEEDBACK_SUCCESS
	color_timestamp = DEFAULT_COLOR_TIMESTAMP_TEXT
	color_source = DEFAULT_COLOR_SOURCE_TEXT
	color_entry_default = DEFAULT_COLOR_ENTRY_DEFAULT
	color_entry_error = DEFAULT_COLOR_ENTRY_ERROR
	color_entry_warning = DEFAULT_COLOR_ENTRY_WARNING
	color_detail_default = DEFAULT_COLOR_DETAIL_DEFAULT
	color_stack_header = DEFAULT_COLOR_DETAIL_STACK_HEADER
	color_stack_frame = DEFAULT_COLOR_DETAIL_STACK_FRAME

func _to_state_dict() -> Dictionary:
	return {
		"stack_trace": stack_trace,
		"short_type_names": short_type_names,
		"duplicates": duplicates,
		"timestamps": timestamps,
		"errors": errors,
		"warnings": warnings,
		"dev": dev,
		"colors_enabled": colors_enabled,
		"warning_prefix_case": warning_prefix_case,
		"error_prefix_case": error_prefix_case,
		"color_tree_font": color_tree_font,
		"color_panel_background": color_panel_background,
		"color_feedback_warning": color_feedback_warning,
		"color_feedback_success": color_feedback_success,
		"color_timestamp": color_timestamp,
		"color_source": color_source,
		"color_entry_default": color_entry_default,
		"color_entry_error": color_entry_error,
		"color_entry_warning": color_entry_warning,
		"color_detail_default": color_detail_default,
		"color_stack_header": color_stack_header,
		"color_stack_frame": color_stack_frame
	}

func _apply_state_dict(state: Dictionary) -> void:
	stack_trace = bool(state.get("stack_trace", stack_trace))
	short_type_names = bool(state.get("short_type_names", short_type_names))
	duplicates = bool(state.get("duplicates", duplicates))
	timestamps = bool(state.get("timestamps", timestamps))
	errors = bool(state.get("errors", errors))
	warnings = bool(state.get("warnings", warnings))
	dev = bool(state.get("dev", dev))
	colors_enabled = bool(state.get("colors_enabled", colors_enabled))
	warning_prefix_case = int(state.get("warning_prefix_case", warning_prefix_case))
	error_prefix_case = int(state.get("error_prefix_case", error_prefix_case))

	color_tree_font = _as_color(state.get("color_tree_font", color_tree_font), color_tree_font)
	color_panel_background = _as_color(state.get("color_panel_background", color_panel_background), color_panel_background)
	color_feedback_warning = _as_color(state.get("color_feedback_warning", color_feedback_warning), color_feedback_warning)
	color_feedback_success = _as_color(state.get("color_feedback_success", color_feedback_success), color_feedback_success)
	color_timestamp = _as_color(state.get("color_timestamp", color_timestamp), color_timestamp)
	color_source = _as_color(state.get("color_source", color_source), color_source)
	color_entry_default = _as_color(state.get("color_entry_default", color_entry_default), color_entry_default)
	color_entry_error = _as_color(state.get("color_entry_error", color_entry_error), color_entry_error)
	color_entry_warning = _as_color(state.get("color_entry_warning", color_entry_warning), color_entry_warning)
	color_detail_default = _as_color(state.get("color_detail_default", color_detail_default), color_detail_default)
	color_stack_header = _as_color(state.get("color_stack_header", color_stack_header), color_stack_header)
	color_stack_frame = _as_color(state.get("color_stack_frame", color_stack_frame), color_stack_frame)

func _as_color(value: Variant, fallback: Color) -> Color:
	if value is Color:
		return value as Color
	return fallback

func _editor_settings() -> EditorSettings:
	if not Engine.is_editor_hint():
		return null
	return EditorInterface.get_editor_settings()
