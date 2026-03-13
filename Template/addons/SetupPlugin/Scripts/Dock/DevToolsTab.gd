@tool
class_name DevToolsTab
extends VBoxContainer

const PROJECT_ROOT_PATH: String = "res://"
const CSPROJ_PATH: String = "res://Template.csproj"
const EDITORCONFIG_PATH: String = "res://.editorconfig"
const LABEL_PADDING: int = 120
const FEEDBACK_DURATION: float = 5.0
const CS8632_SUPPRESSION: String = "dotnet_diagnostic.CS8632.severity = none # The annotation for nullable reference types should only be used in code within a '#nullable' annotations context."
const IDE0370_SUPPRESSION: String = "dotnet_diagnostic.IDE0370.severity = none # Disable the IDE suggestion to enable nullable reference types."

const ANTI_ALIASING_PATH_2D: String = "rendering/anti_aliasing/quality/msaa_2d"
const ANTI_ALIASING_PATH_3D: String = "rendering/anti_aliasing/quality/msaa_3d"
const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"
const DebuggerErrorClipboard = preload("res://addons/SetupPlugin/Scripts/Dock/DebuggerErrorClipboard.gd")
const EditorSceneActions = preload("res://addons/SetupPlugin/Scripts/Dock/EditorSceneActions.gd")
const SceneHierarchyActions = preload("res://addons/SetupPlugin/Scripts/Dock/SceneHierarchyActions.gd")

var _status_label: Label
var _cleanup_uids_button: Button
var _nullable_button: Button
var _copy_debugger_errors_button: Button
var _close_all_scene_tabs_button: Button
var _restart_editor_button: Button
var _include_stack_trace_checkbox: CheckButton
var _use_short_type_names_checkbox: CheckButton
var _hierarchy_level_spinbox: SpinBox
var _expand_to_level_button: Button
var _fully_expand_button: Button
var _anti_aliasing_options: OptionButton
var _clear_color_picker: ColorPickerButton
var _feedback_timer: Timer
var _events_registered: bool
var _debugger_error_clipboard: DebuggerErrorClipboard
var _editor_scene_actions: EditorSceneActions
var _scene_hierarchy_actions: SceneHierarchyActions

func _ready() -> void:
	_create_controls()
	_build_layout()
	_register_events()

func prepare_for_disable() -> void:
	_unregister_events()

func _create_controls() -> void:
	_status_label = Label.new()
	_status_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	_status_label.clip_text = false
	_status_label.custom_minimum_size = Vector2(0, 22)
	_status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_status_label.text = " "

	_cleanup_uids_button = Button.new()
	_cleanup_uids_button.text = "Cleanup uids"
	_cleanup_uids_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_cleanup_uids_button.custom_minimum_size = Vector2(150, 0)

	_nullable_button = Button.new()
	_nullable_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_nullable_button.custom_minimum_size = Vector2(150, 0)
	_update_nullable_button_text()

	_copy_debugger_errors_button = Button.new()
	_copy_debugger_errors_button.text = "Copy Debugger Errors"
	_copy_debugger_errors_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_copy_debugger_errors_button.custom_minimum_size = Vector2(150, 0)

	_close_all_scene_tabs_button = Button.new()
	_close_all_scene_tabs_button.text = "Close All Scene Tabs"
	_close_all_scene_tabs_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_close_all_scene_tabs_button.custom_minimum_size = Vector2(150, 0)

	_restart_editor_button = Button.new()
	_restart_editor_button.text = "Restart Editor"
	_restart_editor_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_restart_editor_button.custom_minimum_size = Vector2(150, 0)

	_include_stack_trace_checkbox = CheckButton.new()
	_include_stack_trace_checkbox.text = "Include Stack Trace"
	_include_stack_trace_checkbox.button_pressed = false

	_use_short_type_names_checkbox = CheckButton.new()
	_use_short_type_names_checkbox.text = "Use Short Type Names"
	_use_short_type_names_checkbox.button_pressed = true

	_hierarchy_level_spinbox = SpinBox.new()
	_hierarchy_level_spinbox.min_value = 0
	_hierarchy_level_spinbox.max_value = 20
	_hierarchy_level_spinbox.step = 1
	_hierarchy_level_spinbox.value = 2
	_hierarchy_level_spinbox.custom_minimum_size = Vector2(90, 0)
	_hierarchy_level_spinbox.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN

	_expand_to_level_button = Button.new()
	_expand_to_level_button.text = "Expand To Level"
	_expand_to_level_button.custom_minimum_size = Vector2(170, 0)
	_expand_to_level_button.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN

	_fully_expand_button = Button.new()
	_fully_expand_button.text = "Fully Expand"
	_fully_expand_button.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN

	_debugger_error_clipboard = DebuggerErrorClipboard.new()
	_editor_scene_actions = EditorSceneActions.new()
	_scene_hierarchy_actions = SceneHierarchyActions.new()

	# moved from setup tab
	_clear_color_picker = ColorPickerButton.new()
	_clear_color_picker.custom_minimum_size = Vector2(75, 35)
	_clear_color_picker.color = ProjectSettings.get_setting(DEFAULT_CLEAR_COLOR_PATH)

	_anti_aliasing_options = OptionButton.new()
	_anti_aliasing_options.add_item("Disabled (Fastest)")
	_anti_aliasing_options.add_item("2x (Average)")
	_anti_aliasing_options.add_item("4x (Slow)")
	_anti_aliasing_options.add_item("8x (Slowest)")
	# initialize from current project settings (prefer 2D value)
	var current_aa: int = ProjectSettings.get_setting(ANTI_ALIASING_PATH_2D)
	if typeof(current_aa) == TYPE_INT and current_aa >= 0 and current_aa < _anti_aliasing_options.get_item_count():
		_anti_aliasing_options.select(current_aa)

	_feedback_timer = Timer.new()
	_feedback_timer.wait_time = FEEDBACK_DURATION
	_feedback_timer.one_shot = true

func _build_layout() -> void:
	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 10)
	content.size_flags_vertical = Control.SIZE_EXPAND_FILL

	var tabs: TabContainer = TabContainer.new()
	tabs.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	tabs.size_flags_vertical = Control.SIZE_EXPAND_FILL
	tabs.add_theme_constant_override("side_margin", 0)
	var no_margin_style: StyleBoxEmpty = StyleBoxEmpty.new()
	tabs.add_theme_stylebox_override("panel", no_margin_style)
	tabs.add_theme_stylebox_override("tabbar_background", no_margin_style)

	var dev_tab: VBoxContainer = VBoxContainer.new()
	dev_tab.name = "Dev"
	dev_tab.add_theme_constant_override("separation", 8)

	var copy_row: HBoxContainer = HBoxContainer.new()
	copy_row.add_child(_copy_debugger_errors_button)
	dev_tab.add_child(copy_row)

	var copy_options_row: HBoxContainer = HBoxContainer.new()
	copy_options_row.add_theme_constant_override("separation", 12)
	copy_options_row.add_child(_include_stack_trace_checkbox)
	copy_options_row.add_child(_use_short_type_names_checkbox)
	dev_tab.add_child(copy_options_row)

	var action_row: HBoxContainer = HBoxContainer.new()
	action_row.add_theme_constant_override("separation", 8)
	action_row.add_child(_cleanup_uids_button)
	action_row.add_child(_nullable_button)
	dev_tab.add_child(action_row)

	var scene_row: HBoxContainer = HBoxContainer.new()
	scene_row.add_theme_constant_override("separation", 8)
	scene_row.add_child(_close_all_scene_tabs_button)
	scene_row.add_child(_restart_editor_button)
	dev_tab.add_child(scene_row)
	dev_tab.add_child(_status_label)

	var hierarchy_label: Label = Label.new()
	hierarchy_label.text = "Hierarchy"

	var visual_tab: VBoxContainer = VBoxContainer.new()
	visual_tab.name = "Visual"
	visual_tab.add_theme_constant_override("separation", 8)
	_add_labeled_control("Clear Color", _clear_color_picker, visual_tab)
	_add_labeled_control("Anti Aliasing", _anti_aliasing_options, visual_tab)
	visual_tab.add_child(hierarchy_label)

	var hierarchy_level_row: HBoxContainer = HBoxContainer.new()
	hierarchy_level_row.add_theme_constant_override("separation", 8)
	hierarchy_level_row.add_child(_expand_to_level_button)
	hierarchy_level_row.add_child(_hierarchy_level_spinbox)
	hierarchy_level_row.add_child(_fully_expand_button)
	visual_tab.add_child(hierarchy_level_row)

	tabs.add_child(dev_tab)
	tabs.add_child(visual_tab)
	content.add_child(tabs)

	add_child(_feedback_timer)
	add_child(content)

func _register_events() -> void:
	if _events_registered:
		return

	_cleanup_uids_button.pressed.connect(_on_cleanup_uids_pressed)
	_nullable_button.pressed.connect(_on_nullable_pressed)
	_copy_debugger_errors_button.pressed.connect(_on_copy_debugger_errors_pressed)
	_close_all_scene_tabs_button.pressed.connect(_on_close_all_scene_tabs_pressed)
	_restart_editor_button.pressed.connect(_on_restart_editor_pressed)
	_expand_to_level_button.pressed.connect(_on_expand_to_level_pressed)
	_fully_expand_button.pressed.connect(_on_fully_expand_pressed)
	_clear_color_picker.color_changed.connect(_on_clear_color_changed)
	_anti_aliasing_options.item_selected.connect(_on_anti_aliasing_item_selected)
	_feedback_timer.timeout.connect(_on_feedback_timer_timeout)
	_events_registered = true

func _unregister_events() -> void:
	if not _events_registered:
		return

	_events_registered = false

	if _cleanup_uids_button != null and _cleanup_uids_button.is_connected("pressed", Callable(self, "_on_cleanup_uids_pressed")):
		_cleanup_uids_button.pressed.disconnect(Callable(self, "_on_cleanup_uids_pressed"))

	if _nullable_button != null and _nullable_button.is_connected("pressed", Callable(self, "_on_nullable_pressed")):
		_nullable_button.pressed.disconnect(Callable(self, "_on_nullable_pressed"))

	if _copy_debugger_errors_button != null and _copy_debugger_errors_button.is_connected("pressed", Callable(self, "_on_copy_debugger_errors_pressed")):
		_copy_debugger_errors_button.pressed.disconnect(Callable(self, "_on_copy_debugger_errors_pressed"))

	if _close_all_scene_tabs_button != null and _close_all_scene_tabs_button.is_connected("pressed", Callable(self, "_on_close_all_scene_tabs_pressed")):
		_close_all_scene_tabs_button.pressed.disconnect(Callable(self, "_on_close_all_scene_tabs_pressed"))

	if _restart_editor_button != null and _restart_editor_button.is_connected("pressed", Callable(self, "_on_restart_editor_pressed")):
		_restart_editor_button.pressed.disconnect(Callable(self, "_on_restart_editor_pressed"))

	if _expand_to_level_button != null and _expand_to_level_button.is_connected("pressed", Callable(self, "_on_expand_to_level_pressed")):
		_expand_to_level_button.pressed.disconnect(Callable(self, "_on_expand_to_level_pressed"))

	if _fully_expand_button != null and _fully_expand_button.is_connected("pressed", Callable(self, "_on_fully_expand_pressed")):
		_fully_expand_button.pressed.disconnect(Callable(self, "_on_fully_expand_pressed"))

	if _clear_color_picker != null and _clear_color_picker.is_connected("color_changed", Callable(self, "_on_clear_color_changed")):
		_clear_color_picker.color_changed.disconnect(Callable(self, "_on_clear_color_changed"))

	if _anti_aliasing_options != null and _anti_aliasing_options.is_connected("item_selected", Callable(self, "_on_anti_aliasing_item_selected")):
		_anti_aliasing_options.item_selected.disconnect(Callable(self, "_on_anti_aliasing_item_selected"))

	if _feedback_timer != null and _feedback_timer.is_connected("timeout", Callable(self, "_on_feedback_timer_timeout")):
		_feedback_timer.timeout.disconnect(Callable(self, "_on_feedback_timer_timeout"))

func _on_cleanup_uids_pressed() -> void:
	_cleanup_uids_button.disabled = true

	var project_root: String = ProjectSettings.globalize_path(PROJECT_ROOT_PATH)
	var removed_count: int = _cleanup_uid_files_recursive(project_root)

	if removed_count > 0:
		_set_status("Removed %d old uid files." % removed_count)
	else:
		_set_status("All uid files are good. No action needed.")

	_feedback_timer.start()
	_cleanup_uids_button.disabled = false

func _on_copy_debugger_errors_pressed() -> void:
	var include_stack_trace: bool = _include_stack_trace_checkbox.button_pressed
	var use_short_type_names: bool = _use_short_type_names_checkbox.button_pressed
	var errors: PackedStringArray = _debugger_error_clipboard.collect_errors(include_stack_trace, use_short_type_names)
	if errors.is_empty():
		_set_status("No errors to copy to clipboard")
		_feedback_timer.start()
		return

	DisplayServer.clipboard_set("\n\n".join(errors))
	_set_status("Copied %d errors to clipboard" % errors.size())
	_feedback_timer.start()

func _on_close_all_scene_tabs_pressed() -> void:
	var closed_count: int = _editor_scene_actions.close_all_open_scenes()
	if closed_count > 0:
		_set_status("Closed %d scene tabs" % closed_count)
	else:
		_set_status("No scene tabs to close")
	_feedback_timer.start()

func _on_restart_editor_pressed() -> void:
	_set_status("Restarting editor...")
	_feedback_timer.start()
	_editor_scene_actions.restart_editor(true)

func _on_expand_to_level_pressed() -> void:
	var level: int = int(_hierarchy_level_spinbox.value)
	var changed_count: int = _scene_hierarchy_actions.expand_to_level(level)
	if changed_count > 0:
		_set_status("Expanded hierarchy to level %d" % level)
	else:
		_set_status("No scene hierarchy available")
	_feedback_timer.start()

func _on_fully_expand_pressed() -> void:
	var changed_count: int = _scene_hierarchy_actions.fully_expand()
	if changed_count > 0:
		_set_status("Fully expanded hierarchy")
	else:
		_set_status("No scene hierarchy available")
	_feedback_timer.start()

func _cleanup_uid_files_recursive(directory: String) -> int:
	var dir: DirAccess = DirAccess.open(directory)
	if dir == null:
		return 0

	var removed_count: int = 0
	var subdirectories: Array[String] = []
	var files_in_dir: Array[String] = []

	dir.list_dir_begin()
	var file_name: String = dir.get_next()

	while file_name != "":
		if not file_name.begins_with("."):
			if dir.current_is_dir():
				subdirectories.append(directory.path_join(file_name))
			else:
				files_in_dir.append(file_name)

		file_name = dir.get_next()

	dir.list_dir_end()

	for uid_file in files_in_dir:
		if not uid_file.ends_with(".uid"):
			continue

		var expected_file: String = uid_file.trim_suffix(".uid")
		if files_in_dir.has(expected_file):
			continue

		DirAccess.remove_absolute(directory.path_join(uid_file))
		removed_count += 1

	for subdirectory in subdirectories:
		removed_count += _cleanup_uid_files_recursive(subdirectory)

	return removed_count

func _add_labeled_control(label_text: String, control: Control, container: VBoxContainer) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	var label: Label = Label.new()
	label.text = "%s:" % label_text
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	label.custom_minimum_size = Vector2(LABEL_PADDING, 0)
	row.add_child(label)
	row.add_child(control)
	container.add_child(row)

func _set_status(text: String) -> void:
	if text.is_empty():
		_status_label.text = " "
		_status_label.modulate = Color(0.75, 0.75, 0.75)
		return

	_status_label.text = text
	_status_label.modulate = Color(0.6, 0.95, 0.6)

func _on_feedback_timer_timeout() -> void:
	_set_status("")

# ── Clear color / anti aliasing controls ───────────────────────────────────────

func _on_clear_color_changed(color: Color) -> void:
	ProjectSettings.set_setting(DEFAULT_CLEAR_COLOR_PATH, color)
	ProjectSettings.save()

func _on_anti_aliasing_item_selected(index: int) -> void:
	# apply to both 2D and 3D settings; dev tools are not project-type specific
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_2D, index)
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_3D, index)

# ── Nullable toggle ──────────────────────────────────────────────────────────

func _update_nullable_button_text() -> void:
	var enabled: bool = _read_nullable_state()
	_nullable_button.text = "Disable Nullable" if enabled else "Enable Nullable"

func _read_nullable_state() -> bool:
	var path: String = ProjectSettings.globalize_path(CSPROJ_PATH)
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return false
	var content: String = file.get_as_text()
	file.close()
	return content.contains("<Nullable>enable</Nullable>")

func _on_nullable_pressed() -> void:
	var currently_enabled: bool = _read_nullable_state()
	var new_state: bool = not currently_enabled

	_set_csproj_nullable(new_state)
	_set_editorconfig_suppressions(not new_state)
	_update_nullable_button_text()

	var state_text: String = "enabled" if new_state else "disabled"
	_set_status("Nullable %s. Rebuild the project to apply." % state_text)
	_feedback_timer.start()

func _set_csproj_nullable(enable: bool) -> void:
	var path: String = ProjectSettings.globalize_path(CSPROJ_PATH)
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return
	var content: String = file.get_as_text()
	file.close()

	if enable:
		content = content.replace("<Nullable>disable</Nullable>", "<Nullable>enable</Nullable>")
	else:
		content = content.replace("<Nullable>enable</Nullable>", "<Nullable>disable</Nullable>")

	var write_file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if write_file == null:
		return
	write_file.store_string(content)
	write_file.close()

func _set_editorconfig_suppressions(suppress: bool) -> void:
	var path: String = ProjectSettings.globalize_path(EDITORCONFIG_PATH)
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return
	var content: String = file.get_as_text()
	file.close()

	if suppress:
		var lines: PackedStringArray = content.split("\n")
		var result: PackedStringArray = []
		for line in lines:
			result.append(line)
			if line.begins_with("dotnet_diagnostic.CA1816.severity = none"):
				if not content.contains(CS8632_SUPPRESSION):
					result.append(CS8632_SUPPRESSION)
				if not content.contains(IDE0370_SUPPRESSION):
					result.append(IDE0370_SUPPRESSION)
		content = "\n".join(result)
	else:
		var lines: PackedStringArray = content.split("\n")
		var result: PackedStringArray = []
		for line in lines:
			if not (line.begins_with("dotnet_diagnostic.CS8632.severity = none") or line.begins_with("dotnet_diagnostic.IDE0370.severity = none")):
				result.append(line)
		content = "\n".join(result)

	var write_file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if write_file == null:
		return
	write_file.store_string(content)
	write_file.close()
