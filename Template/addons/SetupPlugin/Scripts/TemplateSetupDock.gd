@tool
class_name TemplateSetupDock
extends VBoxContainer

const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"
const MAIN_SCENES_PATH: String = "res://addons/SetupPlugin/MainScenes"
const PROJECT_ROOT_PATH: String = "res://"
const REBUILD_INSTRUCTION: String = "Rebuild the project, then disable and re-enable the Setup Plugin."
const LABEL_PADDING: int = 120
const MARGIN_PADDING: int = 30

var _confirm_restart_dialog: ConfirmationDialog
var _feedback_reset_timer: Timer
var _game_name_validator: GameNameValidator
var _project_setup_runner: ProjectSetupRunner
var _runtime_state_validator: SetupRuntimeStateValidator
var _template_catalog: SetupTemplateCatalog

var _template_type: OptionButton
var _project_type: OptionButton
var _game_name_line_edit: LineEdit
var _default_clear_color_picker: ColorPickerButton
var _apply_button: Button
var _content_container: VBoxContainer
var _game_name_preview: Label
var _status_label: Label

var _selected_project_type: String = ""
var _selected_template_type: String = ""
var _runtime_state_error: String = ""
var _is_runtime_state_valid: bool
var _events_registered: bool
var _is_disposed: bool

func _ready() -> void:
	_initialize_services()
	_create_controls()
	_build_layout()
	_validate_and_initialize_state()
	_register_events()

func _initialize_services() -> void:
	var project_root: String = ProjectSettings.globalize_path(PROJECT_ROOT_PATH)
	var main_scenes_root: String = ProjectSettings.globalize_path(MAIN_SCENES_PATH)

	_runtime_state_validator = SetupRuntimeStateValidator.new(project_root, main_scenes_root)
	_project_setup_runner = ProjectSetupRunner.new(project_root, main_scenes_root)
	
func _exit_tree() -> void:
	prepare_for_plugin_disable()

func prepare_for_plugin_disable() -> void:
	if _is_disposed:
		return

	_is_disposed = true
	_unregister_events()
	_release_restart_dialog()

func _create_controls() -> void:
	_confirm_restart_dialog = ConfirmationDialog.new()
	_confirm_restart_dialog.title = "Setup Confirmation"
	_confirm_restart_dialog.dialog_text = "Godot will restart with your changes. This cannot be undone"
	_confirm_restart_dialog.ok_button_text = "Yes"
	_confirm_restart_dialog.cancel_button_text = "No"

	_feedback_reset_timer = Timer.new()

	_status_label = Label.new()
	_status_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	_status_label.visible = false
	_status_label.modulate = Color(1.0, 0.4, 0.4)

	_game_name_preview = Label.new()
	_game_name_preview.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_game_name_preview.custom_minimum_size = Vector2(200, 0)

	_game_name_line_edit = LineEdit.new()
	_game_name_line_edit.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	_game_name_line_edit.custom_minimum_size = Vector2(200, 0)

	_project_type = OptionButton.new()
	_template_type = OptionButton.new()

	_default_clear_color_picker = ColorPickerButton.new()
	_default_clear_color_picker.custom_minimum_size = Vector2(75, 35)
	_default_clear_color_picker.color = ProjectSettings.get_setting(DEFAULT_CLEAR_COLOR_PATH)

	_apply_button = Button.new()
	_apply_button.text = "Run Setup"
	_apply_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_apply_button.custom_minimum_size = Vector2(200, 0)

	_game_name_validator = GameNameValidator.new(_game_name_preview, _feedback_reset_timer, _game_name_line_edit)

	var editor_main_screen: Node = EditorInterface.get_editor_main_screen()
	editor_main_screen.add_child(_confirm_restart_dialog)

	_set_controls_enabled(false)

func _build_layout() -> void:
	_content_container = VBoxContainer.new()
	_content_container.add_child(_status_label)
	_content_container.add_child(_game_name_preview)

	_add_labeled_control("Project Name", _game_name_line_edit)
	_add_labeled_control("Project", _project_type)
	_add_labeled_control("Template", _template_type)
	_add_labeled_control("Clear Color", _default_clear_color_picker)

	var margin_container: MarginContainer = MarginContainer.new()
	margin_container.add_theme_constant_override("margin_left", MARGIN_PADDING)
	margin_container.add_theme_constant_override("margin_top", MARGIN_PADDING)
	margin_container.add_theme_constant_override("margin_right", MARGIN_PADDING)
	margin_container.add_theme_constant_override("margin_bottom", MARGIN_PADDING)

	margin_container.add_child(_content_container)

	add_child(_feedback_reset_timer)
	add_child(margin_container)
	add_child(_apply_button)

func _register_events() -> void:
	if _events_registered:
		return

	_confirm_restart_dialog.confirmed.connect(_on_confirmed)
	_feedback_reset_timer.timeout.connect(_on_feedback_reset_timer_timeout)
	_game_name_line_edit.text_changed.connect(_on_project_name_changed)
	_project_type.item_selected.connect(_on_project_type_selected)
	_template_type.item_selected.connect(_on_template_type_selected)
	_default_clear_color_picker.color_changed.connect(_on_default_clear_color_changed)
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

	if _default_clear_color_picker.is_connected("color_changed", Callable(self, "_on_default_clear_color_changed")):
		_default_clear_color_picker.color_changed.disconnect(Callable(self, "_on_default_clear_color_changed"))

	if _apply_button.is_connected("pressed", Callable(self, "_on_apply_pressed")):
		_apply_button.pressed.disconnect(Callable(self, "_on_apply_pressed"))

func _release_restart_dialog() -> void:
	if _confirm_restart_dialog == null:
		return

	if not is_instance_valid(_confirm_restart_dialog):
		_confirm_restart_dialog = null
		return

	var parent: Node = _confirm_restart_dialog.get_parent()
	if parent != null:
		parent.remove_child(_confirm_restart_dialog)

	_confirm_restart_dialog.queue_free()
	_confirm_restart_dialog = null

func _validate_and_initialize_state() -> void:
	var runtime_failure: String
	if not _runtime_state_validator.try_validate(runtime_failure):
		_mark_runtime_state_invalid(runtime_failure, null)
		return

	var main_scenes_root: String = ProjectSettings.globalize_path(MAIN_SCENES_PATH)
	var catalog_failure: String
	var template_catalog := SetupTemplateCatalog.try_load(main_scenes_root, catalog_failure)
	if template_catalog == null:
		_mark_runtime_state_invalid(catalog_failure, null)
		return

	_template_catalog = template_catalog

	var selection := _template_catalog.get_first_selection()
	if selection.is_empty():
		_mark_runtime_state_invalid("No setup template options are available.", null)
		return

	_selected_project_type = selection["project_type"]
	_selected_template_type = selection["template_type"]

	_populate_project_type_options()
	_populate_template_type_options(_selected_project_type)

	_project_type.select(0)
	_template_type.select(0)

	_is_runtime_state_valid = true
	_runtime_state_error = ""
	_set_controls_enabled(true)
	_set_status("Setup plugin is ready.", false)

func _populate_project_type_options() -> void:
	_project_type.clear()

	var project_types: Array = _template_catalog.project_types
	for project_type in project_types:
		_project_type.add_item(project_type)
		
func _populate_template_type_options(project_type: String) -> void:
	_template_type.clear()

	var template_types: Array = _template_catalog.get_templates(project_type)
	if template_types.is_empty():
		_selected_template_type = ""
		return

	for template_type in template_types:
		_template_type.add_item(template_type)

	_template_type.select(0)
	_selected_template_type = _template_type.get_item_text(0)

func _add_labeled_control(label_text: String, control: Control) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	var label: Label = Label.new()
	label.text = "%s:" % label_text
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	label.custom_minimum_size = Vector2(LABEL_PADDING, 0)
	row.add_child(label)

	row.add_child(control)
	_content_container.add_child(row)

func _set_controls_enabled(enabled: bool) -> void:
	_game_name_line_edit.editable = enabled
	_project_type.disabled = not enabled
	_template_type.disabled = not enabled
	_default_clear_color_picker.disabled = not enabled
	_apply_button.disabled = not enabled

func _set_status(text: String, is_error: bool) -> void:
	_status_label.text = text
	_status_label.visible = not text.is_empty()
	_status_label.modulate = Color(1.0, 0.4, 0.4) if is_error else Color(0.6, 0.95, 0.6)

func _ensure_runtime_state_valid(operation_name: String) -> bool:
	if _is_runtime_state_valid:
		return true

	var message: String = _runtime_state_error
	if message.is_empty():
		message = REBUILD_INSTRUCTION

	_set_status(message, true)
	printerr("Setup operation blocked (%s). %s" % [operation_name, message])
	return false

func _mark_runtime_state_invalid(reason: String, exception: Variant = null) -> void:
	_is_runtime_state_valid = false
	_runtime_state_error = "%s %s" % [reason, REBUILD_INSTRUCTION]
	_set_controls_enabled(false)
	_set_status(_runtime_state_error, true)

	printerr(_runtime_state_error)

	if exception == null:
		return

	printerr(str(exception))

func _report_user_error(message: String) -> void:
	_game_name_preview.text = message
	_set_status(message, true)
	printerr(message)

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

func _on_default_clear_color_changed(color: Color) -> void:
	if not _ensure_runtime_state_valid("DefaultClearColorChanged"):
		return

	ProjectSettings.set_setting(DEFAULT_CLEAR_COLOR_PATH, color)
	ProjectSettings.save()

func _on_confirmed() -> void:
	if not _ensure_runtime_state_valid("Confirmed"):
		return

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
