@tool
extends RefCounted

var _settings
var _view

func _init(settings, view) -> void:
	_settings = settings
	_view = view

func load() -> void:
	if _settings == null:
		return
	_settings.load()
	_apply_settings_to_controls()

func save() -> void:
	if _settings == null:
		return
	_apply_controls_to_settings()
	_settings.save()

func reset_defaults() -> void:
	if _settings == null:
		return
	_settings.reset_defaults()
	_apply_settings_to_controls()

func get_settings():
	return _settings

func include_stack_trace() -> bool:
	return _view.get_checkbox(&"stack_trace").button_pressed

func use_short_type_names() -> bool:
	return _view.get_checkbox(&"short_type_names").button_pressed

func include_duplicates() -> bool:
	return _view.get_checkbox(&"duplicates").button_pressed

func show_timestamps() -> bool:
	return _view.get_checkbox(&"timestamps").button_pressed

func show_errors() -> bool:
	return _view.get_checkbox(&"errors").button_pressed

func show_warnings() -> bool:
	return _view.get_checkbox(&"warnings").button_pressed

func dev_mode() -> bool:
	return _view.get_checkbox(&"dev").button_pressed

func warning_prefix_case() -> int:
	return int(_settings.warning_prefix_case)

func error_prefix_case() -> int:
	return int(_settings.error_prefix_case)

func set_warning_prefix_case(mode: int) -> void:
	_settings.warning_prefix_case = clampi(mode, 0, 2)
	_settings.save()
	_view.update_popup_prefix_cases(warning_prefix_case(), error_prefix_case())

func set_error_prefix_case(mode: int) -> void:
	_settings.error_prefix_case = clampi(mode, 0, 2)
	_settings.save()
	_view.update_popup_prefix_cases(warning_prefix_case(), error_prefix_case())

func set_colors_enabled(enabled: bool) -> void:
	_settings.colors_enabled = enabled
	_settings.save()

func set_color_by_key(color_key: String, color: Color) -> void:
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
	_settings.save()

func current_color_map() -> Dictionary:
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

func open_popup_state() -> Dictionary:
	return {
		"colors": current_color_map(),
		"colors_enabled": bool(_settings.colors_enabled),
		"warning_case": warning_prefix_case(),
		"error_case": error_prefix_case()
	}

func _apply_settings_to_controls() -> void:
	_view.get_checkbox(&"stack_trace").button_pressed = bool(_settings.stack_trace)
	_view.get_checkbox(&"short_type_names").button_pressed = bool(_settings.short_type_names)
	_view.get_checkbox(&"duplicates").button_pressed = bool(_settings.duplicates)
	_view.get_checkbox(&"timestamps").button_pressed = bool(_settings.timestamps)
	_view.get_checkbox(&"errors").button_pressed = bool(_settings.errors)
	_view.get_checkbox(&"warnings").button_pressed = bool(_settings.warnings)
	_view.get_checkbox(&"dev").button_pressed = bool(_settings.dev)
	_view.update_popup_prefix_cases(warning_prefix_case(), error_prefix_case())

func _apply_controls_to_settings() -> void:
	_settings.stack_trace = include_stack_trace()
	_settings.short_type_names = use_short_type_names()
	_settings.duplicates = include_duplicates()
	_settings.timestamps = show_timestamps()
	_settings.errors = show_errors()
	_settings.warnings = show_warnings()
	_settings.dev = dev_mode()
