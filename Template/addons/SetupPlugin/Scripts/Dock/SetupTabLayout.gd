@tool
class_name SetupTabLayout
extends VBoxContainer

const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"
const LABEL_PADDING: int = 120
const MARGIN_PADDING: int = 30

var _confirm_restart_dialog: ConfirmationDialog
var _feedback_reset_timer: Timer
var _game_name_validator: GameNameValidator

var _template_type: OptionButton
var _project_type: OptionButton
var _game_name_line_edit: LineEdit
var _default_clear_color_picker: ColorPickerButton
var _anti_aliasing_options: OptionButton
var _apply_button: Button
var _content_container: VBoxContainer
var _game_name_preview: Label
var _status_label: Label

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
	_game_name_preview.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_game_name_preview.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_game_name_preview.custom_minimum_size = Vector2(200, 0)

	_game_name_line_edit = LineEdit.new()
	_game_name_line_edit.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	_game_name_line_edit.custom_minimum_size = Vector2(200, 0)

	_project_type = OptionButton.new()
	_template_type = OptionButton.new()

	_default_clear_color_picker = ColorPickerButton.new()
	_default_clear_color_picker.custom_minimum_size = Vector2(75, 35)
	_default_clear_color_picker.color = ProjectSettings.get_setting(DEFAULT_CLEAR_COLOR_PATH)

	_anti_aliasing_options = OptionButton.new()
	_anti_aliasing_options.add_item("Disabled (Fastest)")
	_anti_aliasing_options.add_item("2x (Average)")
	_anti_aliasing_options.add_item("4x (Slow)")
	_anti_aliasing_options.add_item("8x (Slowest)")

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
	_add_labeled_control("Anti Aliasing", _anti_aliasing_options)
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
