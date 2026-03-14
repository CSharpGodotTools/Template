@tool
extends VBoxContainer

const LABEL_PADDING: int = 120
const FEEDBACK_DURATION: float = 5.0
const ANTI_ALIASING_PATH_2D: String = "rendering/anti_aliasing/quality/msaa_2d"
const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"

var _status_label: Label
var _cleanup_uids_button: Button
var _nullable_button: Button
var _remove_empty_folders_button: Button
var _copy_debugger_errors_button: Button
var _close_all_scene_tabs_button: Button
var _restart_editor_button: Button
var _include_stack_trace_checkbox: CheckButton
var _use_short_type_names_checkbox: CheckButton
var _hierarchy_level_spinbox: SpinBox
var _expand_to_level_button: Button
var _fully_expand_button: Button
var _fully_collapse_button: Button
var _anti_aliasing_options: OptionButton
var _clear_color_picker: ColorPickerButton
var _update_from_main_button: Button
var _update_from_release_button: Button
var _feedback_timer: Timer

func _create_controls() -> void:
	_status_label = Label.new()
	_status_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	_status_label.clip_text = false
	_status_label.custom_minimum_size = Vector2(0, 22)
	_status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_status_label.text = " "

	_cleanup_uids_button = _create_button("Cleanup uids", 150)
	_nullable_button = _create_button("Nullable", 150)
	_remove_empty_folders_button = _create_button("Remove Empty Folders", 180)
	_copy_debugger_errors_button = _create_fill_button("Copy Debugger Errors")
	_close_all_scene_tabs_button = _create_fill_button("Close All Scene Tabs")
	_restart_editor_button = _create_fill_button("Restart Editor")
	_include_stack_trace_checkbox = _create_checkbox("Include Stack Trace", false)
	_use_short_type_names_checkbox = _create_checkbox("Use Short Type Names", true)

	_hierarchy_level_spinbox = SpinBox.new()
	_hierarchy_level_spinbox.min_value = 0
	_hierarchy_level_spinbox.max_value = 20
	_hierarchy_level_spinbox.step = 1
	_hierarchy_level_spinbox.value = 2
	_hierarchy_level_spinbox.custom_minimum_size = Vector2(90, 0)
	_hierarchy_level_spinbox.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN

	_expand_to_level_button = _create_button("Expand To Level", 170)
	_fully_expand_button = _create_button("Fully Expand")
	_fully_collapse_button = _create_button("Fully Collapse")

	_update_from_main_button = _create_fill_button("Update From Main Branch")
	_update_from_release_button = _create_fill_button("Update From Latest Release")

	_clear_color_picker = ColorPickerButton.new()
	_clear_color_picker.custom_minimum_size = Vector2(75, 35)
	_clear_color_picker.color = ProjectSettings.get_setting(DEFAULT_CLEAR_COLOR_PATH)

	_anti_aliasing_options = OptionButton.new()
	for label in ["Disabled (Fastest)", "2x (Average)", "4x (Slow)", "8x (Slowest)"]:
		_anti_aliasing_options.add_item(label)
	var current_aa: int = ProjectSettings.get_setting(ANTI_ALIASING_PATH_2D)
	if typeof(current_aa) == TYPE_INT and current_aa >= 0 and current_aa < _anti_aliasing_options.item_count:
		_anti_aliasing_options.select(current_aa)

	_feedback_timer = Timer.new()
	_feedback_timer.wait_time = FEEDBACK_DURATION
	_feedback_timer.one_shot = true

func _build_layout() -> void:
	var tabs: TabContainer = TabContainer.new()
	tabs.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	tabs.size_flags_vertical = Control.SIZE_EXPAND_FILL
	tabs.add_theme_constant_override("side_margin", 0)
	var no_margin_style: StyleBoxEmpty = StyleBoxEmpty.new()
	tabs.add_theme_stylebox_override("panel", no_margin_style)
	tabs.add_theme_stylebox_override("tabbar_background", no_margin_style)
	tabs.add_child(_build_dev_tab())
	tabs.add_child(_build_visual_tab())
	tabs.add_child(_build_update_tab())

	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 10)
	content.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_child(tabs)
	add_child(_feedback_timer)
	add_child(content)

func _build_dev_tab() -> VBoxContainer:
	var dev_tab: VBoxContainer = VBoxContainer.new()
	dev_tab.name = "Dev"
	dev_tab.add_theme_constant_override("separation", 8)
	dev_tab.add_child(_create_row([_copy_debugger_errors_button], 0))
	dev_tab.add_child(_create_row([_include_stack_trace_checkbox, _use_short_type_names_checkbox], 12))
	dev_tab.add_child(_create_row([_cleanup_uids_button, _nullable_button, _remove_empty_folders_button], 8))
	dev_tab.add_child(_create_row([_close_all_scene_tabs_button, _restart_editor_button], 8))
	dev_tab.add_child(_status_label)
	return dev_tab

func _build_visual_tab() -> VBoxContainer:
	var visual_tab: VBoxContainer = VBoxContainer.new()
	visual_tab.name = "Visual"
	visual_tab.add_theme_constant_override("separation", 8)
	_add_labeled_control("Clear Color", _clear_color_picker, visual_tab)
	_add_labeled_control("Anti Aliasing", _anti_aliasing_options, visual_tab)
	var hierarchy_label: Label = Label.new()
	hierarchy_label.text = "Hierarchy"
	visual_tab.add_child(hierarchy_label)
	visual_tab.add_child(_create_row([_expand_to_level_button, _hierarchy_level_spinbox, _fully_expand_button, _fully_collapse_button], 8))
	return visual_tab

func _build_update_tab() -> VBoxContainer:
	var update_tab: VBoxContainer = VBoxContainer.new()
	update_tab.name = "Update"
	update_tab.add_theme_constant_override("separation", 8)
	update_tab.add_child(_create_row([_update_from_main_button], 0))
	update_tab.add_child(_create_row([_update_from_release_button], 0))
	return update_tab

func _create_row(controls: Array[Control], separation: int) -> HBoxContainer:
	var row: HBoxContainer = HBoxContainer.new()
	if separation > 0:
		row.add_theme_constant_override("separation", separation)
	for control in controls:
		row.add_child(control)
	return row

func _add_labeled_control(label_text: String, control: Control, container: VBoxContainer) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	var label: Label = Label.new()
	label.text = "%s:" % label_text
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	label.custom_minimum_size = Vector2(LABEL_PADDING, 0)
	row.add_child(label)
	row.add_child(control)
	container.add_child(row)

func _create_button(text: String, min_width: int = 0) -> Button:
	var button: Button = Button.new()
	button.text = text
	button.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	if min_width > 0:
		button.custom_minimum_size = Vector2(min_width, 0)
	return button

func _create_fill_button(text: String) -> Button:
	var button: Button = _create_button(text, 150)
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	return button

func _create_checkbox(text: String, pressed: bool) -> CheckButton:
	var checkbox: CheckButton = CheckButton.new()
	checkbox.text = text
	checkbox.button_pressed = pressed
	return checkbox