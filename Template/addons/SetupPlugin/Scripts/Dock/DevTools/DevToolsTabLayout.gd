@tool
# Constructs and owns all UI controls for the Dev Tools dock.
# Provides three tab sections:
#   Dev    – editor utility buttons (UID cleanup, nullable toggle, scene actions, etc.)
#   Visual – rendering settings (clear colour, anti-aliasing, hierarchy controls)
#   Update – template synchronisation buttons and backup reminder
extends VBoxContainer

const LABEL_PADDING: int = 120
const FEEDBACK_DURATION: float = 5.0
const UPDATE_FEEDBACK_DURATION: float = 5.0
const UPDATE_WARNING_DEFAULT_TEXT: String = "Remember to back up your project [b]before[/b] updating!"
const TAB_MARGIN_HORIZONTAL_PX: int = 5
const TAB_MARGIN_TOP_PX: int = 10
const TAB_MARGIN_BOTTOM_PX: int = 0
const ANTI_ALIASING_PATH_2D: String = "rendering/anti_aliasing/quality/msaa_2d"
const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"

var _status_label: Label
var _visual_status_label: Label
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
var _check_updates_button: Button
var _reset_update_cache_button: Button
var _view_template_repo_button: Button
var _check_updates_on_startup_checkbox: CheckButton
var _tracked_main_commit_label: Label
var _tracked_release_version_label: Label
var _latest_main_commit_label: Label
var _latest_release_version_label: Label
var _update_warning_label: RichTextLabel
var _feedback_timer: Timer
var _update_feedback_timer: Timer

# Instantiates every UI control node.
# Must be called before _build_layout so all controls exist when they are
# added to the scene tree.
func _create_controls() -> void:
	_status_label = Label.new()
	_status_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	_status_label.clip_text = false
	_status_label.custom_minimum_size = Vector2(0, 22)
	_status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_status_label.text = " "

	_visual_status_label = Label.new()
	_visual_status_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	_visual_status_label.clip_text = false
	_visual_status_label.custom_minimum_size = Vector2(0, 22)
	_visual_status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_visual_status_label.text = " "

	_cleanup_uids_button = _create_button("Cleanup uids", 150)
	_nullable_button = _create_button("Nullable", 150)
	_remove_empty_folders_button = _create_button("Remove Empty Folders", 180)
	_copy_debugger_errors_button = _create_fill_button("Copy Debugger Errors")
	_close_all_scene_tabs_button = _create_button("Close All Scene Tabs", 180)
	_restart_editor_button = _create_button("Restart Editor", 180)
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

	_update_from_main_button = _create_button("Update From Main Branch", 230)
	_update_from_release_button = _create_button("Update From Latest Release", 230)
	_check_updates_button = _create_button("Check For Updates", 230)
	_reset_update_cache_button = _create_button("Reset Update Cache", 230)
	_view_template_repo_button = _create_button("View Template Repository")
	_check_updates_on_startup_checkbox = _create_checkbox("Check for updates when project starts up", true)

	_tracked_main_commit_label = Label.new()
	_tracked_main_commit_label.text = "Not tracked"
	_tracked_main_commit_label.clip_text = false
	_tracked_main_commit_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_OFF
	_tracked_main_commit_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_tracked_release_version_label = Label.new()
	_tracked_release_version_label.text = "Not tracked"
	_tracked_release_version_label.clip_text = false
	_tracked_release_version_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_OFF
	_tracked_release_version_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_latest_main_commit_label = Label.new()
	_latest_main_commit_label.text = "Unknown"
	_latest_main_commit_label.clip_text = false
	_latest_main_commit_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_OFF
	_latest_main_commit_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_latest_release_version_label = Label.new()
	_latest_release_version_label.text = "Unknown"
	_latest_release_version_label.clip_text = false
	_latest_release_version_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_OFF
	_latest_release_version_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	_update_warning_label = RichTextLabel.new()
	_update_warning_label.bbcode_enabled = true
	_update_warning_label.text = UPDATE_WARNING_DEFAULT_TEXT
	_update_warning_label.fit_content = true
	_update_warning_label.scroll_active = false
	_update_warning_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL

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

	_update_feedback_timer = Timer.new()
	_update_feedback_timer.wait_time = UPDATE_FEEDBACK_DURATION
	_update_feedback_timer.one_shot = true

# Assembles the three-tab container and attaches it to this VBoxContainer.
# Must be called after _create_controls.
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
	add_child(_update_feedback_timer)
	add_child(content)

# Builds and returns the Dev tab content:
# clipboard errors, uid cleanup, nullable toggle, hierarchy controls, scene actions.
func _build_dev_tab() -> VBoxContainer:
	var dev_tab: VBoxContainer = VBoxContainer.new()
	dev_tab.name = "Dev"

	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 8)
	content.add_child(_create_row([_copy_debugger_errors_button], 0))
	content.add_child(_create_row([_include_stack_trace_checkbox, _use_short_type_names_checkbox], 12))
	content.add_child(_create_row([_cleanup_uids_button, _nullable_button, _remove_empty_folders_button, _view_template_repo_button, _close_all_scene_tabs_button, _restart_editor_button], 8))
	content.add_child(_status_label)

	dev_tab.add_child(_wrap_with_tab_margin(content))
	return dev_tab

# Builds and returns the Visual tab content:
# viewport clear colour picker, MSAA dropdown, and hierarchy depth controls.
func _build_visual_tab() -> VBoxContainer:
	var visual_tab: VBoxContainer = VBoxContainer.new()
	visual_tab.name = "Visual"

	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 8)

	var split_row: HBoxContainer = HBoxContainer.new()
	split_row.add_theme_constant_override("separation", 16)

	var rendering_column: VBoxContainer = VBoxContainer.new()
	rendering_column.add_theme_constant_override("separation", 8)
	rendering_column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_add_labeled_control("Clear Color", _clear_color_picker, rendering_column)
	_add_labeled_control("Anti Aliasing", _anti_aliasing_options, rendering_column)

	var hierarchy_column: VBoxContainer = VBoxContainer.new()
	hierarchy_column.add_theme_constant_override("separation", 8)
	hierarchy_column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	var hierarchy_label: Label = Label.new()
	hierarchy_label.text = "Hierarchy"
	hierarchy_column.add_child(hierarchy_label)
	hierarchy_column.add_child(_create_row([_expand_to_level_button, _hierarchy_level_spinbox, _fully_expand_button, _fully_collapse_button], 8))

	split_row.add_child(rendering_column)
	split_row.add_child(hierarchy_column)
	content.add_child(split_row)
	content.add_child(_visual_status_label)
	visual_tab.add_child(_wrap_with_tab_margin(content))
	return visual_tab

# Builds and returns the Update tab content:
# update-from-main/release buttons and a backup reminder label.
func _build_update_tab() -> VBoxContainer:
	var update_tab: VBoxContainer = VBoxContainer.new()
	update_tab.name = "Update"

	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 8)

	var top_row: HBoxContainer = HBoxContainer.new()
	top_row.add_theme_constant_override("separation", 16)
	top_row.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	var left_column: VBoxContainer = VBoxContainer.new()
	left_column.add_theme_constant_override("separation", 8)
	left_column.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	var title_label: Label = Label.new()
	title_label.text = "Template Updater"
	title_label.add_theme_font_size_override("font_size", 18)
	left_column.add_child(title_label)

	var subtitle_label: Label = Label.new()
	subtitle_label.text = "Sync your project with upstream commits or releases."
	subtitle_label.modulate = Color(0.82, 0.82, 0.82)
	subtitle_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	left_column.add_child(subtitle_label)
	left_column.add_child(_check_updates_on_startup_checkbox)

	var right_column: VBoxContainer = VBoxContainer.new()
	right_column.add_theme_constant_override("separation", 0)
	right_column.size_flags_horizontal = Control.SIZE_SHRINK_END

	var metadata_grid: GridContainer = GridContainer.new()
	metadata_grid.columns = 2
	metadata_grid.add_theme_constant_override("h_separation", 25)
	metadata_grid.add_theme_constant_override("v_separation", 8)
	metadata_grid.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	metadata_grid.add_child(_create_update_info_block("Tracked Main Commit", _tracked_main_commit_label))
	metadata_grid.add_child(_create_update_info_block("Latest Main Branch", _latest_main_commit_label))
	metadata_grid.add_child(_create_update_info_block("Tracked Release", _tracked_release_version_label))
	metadata_grid.add_child(_create_update_info_block("Latest Release", _latest_release_version_label))
	right_column.add_child(metadata_grid)

	top_row.add_child(left_column)
	top_row.add_child(right_column)
	content.add_child(top_row)

	var action_grid: GridContainer = GridContainer.new()
	action_grid.columns = 2
	action_grid.add_theme_constant_override("h_separation", 8)
	action_grid.add_theme_constant_override("v_separation", 8)
	for action_button in [_update_from_main_button, _update_from_release_button, _check_updates_button, _reset_update_cache_button]:
		action_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		action_grid.add_child(action_button)
	content.add_child(action_grid)
	content.add_child(_update_warning_label)

	update_tab.add_child(_wrap_with_tab_margin(content))
	return update_tab

# Wraps content in a consistent margin for all tab pages.
func _wrap_with_tab_margin(content: Control) -> MarginContainer:
	var margin: MarginContainer = MarginContainer.new()
	margin.add_theme_constant_override("margin_left", TAB_MARGIN_HORIZONTAL_PX)
	margin.add_theme_constant_override("margin_top", TAB_MARGIN_TOP_PX)
	margin.add_theme_constant_override("margin_right", TAB_MARGIN_HORIZONTAL_PX)
	margin.add_theme_constant_override("margin_bottom", TAB_MARGIN_BOTTOM_PX)
	margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	margin.size_flags_vertical = Control.SIZE_EXPAND_FILL
	margin.add_child(content)
	return margin

# Creates a compact, two-line info block used in the update metadata grid.
func _create_update_info_block(title: String, value_label: Label) -> VBoxContainer:
	var block: VBoxContainer = VBoxContainer.new()
	block.add_theme_constant_override("separation", 2)
	block.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	var title_label: Label = Label.new()
	title_label.text = title
	title_label.modulate = Color(0.82, 0.82, 0.82)
	title_label.clip_text = false
	title_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_OFF
	title_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	block.add_child(title_label)
	block.add_child(value_label)
	return block

# Wraps an array of controls in an HBoxContainer.
# Pass separation > 0 to set a custom gap between items.
func _create_row(controls: Array[Control], separation: int) -> HBoxContainer:
	var row: HBoxContainer = HBoxContainer.new()
	if separation > 0:
		row.add_theme_constant_override("separation", separation)
	for control in controls:
		row.add_child(control)
	return row

# Adds a right-aligned label paired with a control as a row inside `container`.
# Used in labeled settings rows such as "Clear Color:" + ColorPickerButton.
func _add_labeled_control(label_text: String, control: Control, container: VBoxContainer) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	var label: Label = Label.new()
	label.text = "%s:" % label_text
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	label.custom_minimum_size = Vector2(LABEL_PADDING, 0)
	row.add_child(label)
	row.add_child(control)
	container.add_child(row)

# Creates a Button that shrinks to fit its text.
# Provide min_width to set a minimum pixel width.
func _create_button(text: String, min_width: int = 0) -> Button:
	var button: Button = Button.new()
	button.text = text
	button.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	if min_width > 0:
		button.custom_minimum_size = Vector2(min_width, 0)
	return button

# Creates a Button that expands horizontally to fill all available space.
func _create_fill_button(text: String) -> Button:
	var button: Button = _create_button(text, 150)
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	return button

# Creates a CheckButton with the given label and initial toggle state.
func _create_checkbox(text: String, pressed: bool) -> CheckButton:
	var checkbox: CheckButton = CheckButton.new()
	checkbox.text = text
	checkbox.button_pressed = pressed
	checkbox.alignment = HORIZONTAL_ALIGNMENT_LEFT
	checkbox.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	return checkbox