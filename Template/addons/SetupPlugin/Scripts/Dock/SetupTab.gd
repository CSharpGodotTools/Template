@tool
# Event-wiring layer for the Setup tab.
# Connects UI signals to business logic and implements each event handler.
# Inherits UI control creation from SetupTabLayout and state management from
# SetupTabState.
class_name SetupTab
extends SetupTabState

var _events_registered: bool

# Initialises services, creates controls, builds the layout, validates runtime
# state, then connects all signals.
func _ready() -> void:
	_initialize_services()
	_create_controls()
	_build_layout()
	_validate_and_initialize_state()
	_register_events()

# Disconnects all signals and releases the confirmation dialog.
# Called before the dock node is freed.
func prepare_for_disable() -> void:
	_unregister_events()
	_release_restart_dialog()

# Connects all UI signals to their handler methods.
# The guard prevents double-wiring on plugin hot-reload.
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

# Disconnects all UI signals.
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

# Updates the template type dropdown when a different project type is selected.
func _on_project_type_selected(index: int) -> void:
	if not _ensure_runtime_state_valid("ProjectTypeSelected"):
		return

	if index < 0 or index >= _project_type.item_count:
		_report_user_error("Selected project type is out of range.")
		return

	_selected_project_type = _project_type.get_item_text(index)
	_populate_template_type_options(_selected_project_type)

# Stores the name of the newly selected template type.
func _on_template_type_selected(index: int) -> void:
	if not _ensure_runtime_state_valid("TemplateTypeSelected"):
		return

	if index < 0 or index >= _template_type.item_count:
		_report_user_error("Selected template type is out of range.")
		return

	_selected_template_type = _template_type.get_item_text(index)



# Called when the user clicks "Yes" in the confirmation dialog.
# Formats the game name and launches the setup runner.
func _on_confirmed() -> void:
	if not _ensure_runtime_state_valid("Confirmed"):
		return

	_set_anti_aliasing_type(_selected_anti_aliasing_type)

	var formatted_game_name: String = GameNameRules.format_game_name(_game_name_line_edit.text)
	_project_setup_runner.run(formatted_game_name, _selected_project_type, _selected_template_type)

# Restores the game name preview after the brief validation-error display period.
func _on_feedback_reset_timer_timeout() -> void:
	if _game_name_validator == null:
		return

	_game_name_validator.restore_previous_game_name_preview()

# Live-validates the typed game name and updates the formatted preview label.
func _on_project_name_changed(game_name: String) -> void:
	if not _ensure_runtime_state_valid("ProjectNameChanged"):
		return

	_game_name_validator.validate(game_name)

# Validates the game name for setup eligibility and opens the confirmation
# dialog if all checks pass.
func _on_apply_pressed() -> void:
	if not _ensure_runtime_state_valid("ApplyPressed"):
		return

	var validation_error: String
	if not GameNameRules.try_validate_for_setup(_game_name_line_edit.text, validation_error):
		_report_user_error(validation_error)
		return

	_confirm_restart_dialog.popup_centered()
