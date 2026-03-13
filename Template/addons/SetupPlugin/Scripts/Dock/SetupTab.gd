@tool
class_name SetupTab
extends SetupTabState

var _events_registered: bool

func _ready() -> void:
	_initialize_services()
	_create_controls()
	_build_layout()
	_validate_and_initialize_state()
	_register_events()

func prepare_for_disable() -> void:
	_unregister_events()
	_release_restart_dialog()

func _register_events() -> void:
	if _events_registered:
		return

	_confirm_restart_dialog.confirmed.connect(_on_confirmed)
	_feedback_reset_timer.timeout.connect(_on_feedback_reset_timer_timeout)
	_game_name_line_edit.text_changed.connect(_on_project_name_changed)
	_project_type.item_selected.connect(_on_project_type_selected)
	_template_type.item_selected.connect(_on_template_type_selected)
	_apply_button.pressed.connect(_on_apply_pressed)
	_events_registered = true

func _unregister_events() -> void:
	if not _events_registered:
		return

	_events_registered = false

	if _confirm_restart_dialog.is_connected("confirmed", Callable(self, "_on_confirmed")):
		_confirm_restart_dialog.confirmed.disconnect(Callable(self, "_on_confirmed"))

	if _feedback_reset_timer.is_connected("timeout", Callable(self, "_on_feedback_reset_timer_timeout")):
		_feedback_reset_timer.timeout.disconnect(Callable(self, "_on_feedback_reset_timer_timeout"))

	if _game_name_line_edit.is_connected("text_changed", Callable(self, "_on_project_name_changed")):
		_game_name_line_edit.text_changed.disconnect(Callable(self, "_on_project_name_changed"))

	if _project_type.is_connected("item_selected", Callable(self, "_on_project_type_selected")):
		_project_type.item_selected.disconnect(Callable(self, "_on_project_type_selected"))

	if _template_type.is_connected("item_selected", Callable(self, "_on_template_type_selected")):
		_template_type.item_selected.disconnect(Callable(self, "_on_template_type_selected"))

	if _apply_button.is_connected("pressed", Callable(self, "_on_apply_pressed")):
		_apply_button.pressed.disconnect(Callable(self, "_on_apply_pressed"))

func _on_project_type_selected(index: int) -> void:
	if not _ensure_runtime_state_valid("ProjectTypeSelected"):
		return

	if index < 0 or index >= _project_type.item_count:
		_report_user_error("Selected project type is out of range.")
		return

	_selected_project_type = _project_type.get_item_text(index)
	_populate_template_type_options(_selected_project_type)

func _on_template_type_selected(index: int) -> void:
	if not _ensure_runtime_state_valid("TemplateTypeSelected"):
		return

	if index < 0 or index >= _template_type.item_count:
		_report_user_error("Selected template type is out of range.")
		return

	_selected_template_type = _template_type.get_item_text(index)



func _on_confirmed() -> void:
	if not _ensure_runtime_state_valid("Confirmed"):
		return

	_set_anti_aliasing_type(_selected_anti_aliasing_type)

	var formatted_game_name: String = GameNameRules.format_game_name(_game_name_line_edit.text)
	_project_setup_runner.run(formatted_game_name, _selected_project_type, _selected_template_type)

func _on_feedback_reset_timer_timeout() -> void:
	if _game_name_validator == null:
		return

	_game_name_validator.restore_previous_game_name_preview()

func _on_project_name_changed(game_name: String) -> void:
	if not _ensure_runtime_state_valid("ProjectNameChanged"):
		return

	_game_name_validator.validate(game_name)

func _on_apply_pressed() -> void:
	if not _ensure_runtime_state_valid("ApplyPressed"):
		return

	var validation_error: String
	if not GameNameRules.try_validate_for_setup(_game_name_line_edit.text, validation_error):
		_report_user_error(validation_error)
		return

	_confirm_restart_dialog.popup_centered()
