@tool
class_name DevToolsTab
extends VBoxContainer

const PROJECT_ROOT_PATH: String = "res://"
const CSPROJ_PATH: String = "res://Template.csproj"
const EDITORCONFIG_PATH: String = "res://.editorconfig"
const LABEL_PADDING: int = 120
const MARGIN_PADDING: int = 30
const FEEDBACK_DURATION: float = 5.0
const CS8632_SUPPRESSION: String = "dotnet_diagnostic.CS8632.severity = none # The annotation for nullable reference types should only be used in code within a '#nullable' annotations context."
const IDE0370_SUPPRESSION: String = "dotnet_diagnostic.IDE0370.severity = none # Disable the IDE suggestion to enable nullable reference types."

const ANTI_ALIASING_PATH_2D: String = "rendering/anti_aliasing/quality/msaa_2d"
const ANTI_ALIASING_PATH_3D: String = "rendering/anti_aliasing/quality/msaa_3d"
const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"

var _status_label: Label
var _cleanup_uids_button: Button
var _nullable_button: Button
var _anti_aliasing_options: OptionButton
var _clear_color_picker: ColorPickerButton
var _feedback_timer: Timer
var _events_registered: bool

func _ready() -> void:
	_create_controls()
	_build_layout()
	_register_events()

func prepare_for_disable() -> void:
	_unregister_events()

func _create_controls() -> void:
	_status_label = Label.new()
	_status_label.autowrap_mode = TextServer.AutowrapMode.AUTOWRAP_WORD_SMART
	_status_label.visible = false

	_cleanup_uids_button = Button.new()
	_cleanup_uids_button.text = "Cleanup uids"
	_cleanup_uids_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_cleanup_uids_button.custom_minimum_size = Vector2(150, 0)

	_nullable_button = Button.new()
	_nullable_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_nullable_button.custom_minimum_size = Vector2(150, 0)
	_update_nullable_button_text()

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
	content.add_child(_status_label)

	var left_col: VBoxContainer = VBoxContainer.new()
	_add_labeled_control("Clear Color", _clear_color_picker, left_col)
	_add_labeled_control("Anti Aliasing", _anti_aliasing_options, left_col)

	var right_col: VBoxContainer = VBoxContainer.new()
	right_col.add_child(_cleanup_uids_button)
	right_col.add_child(_nullable_button)

	var row: HBoxContainer = HBoxContainer.new()
	row.add_theme_constant_override("separation", 100)
	row.add_child(left_col)
	row.add_child(right_col)

	content.add_child(row)

	var margin: MarginContainer = MarginContainer.new()
	margin.add_theme_constant_override("margin_left", MARGIN_PADDING)
	margin.add_theme_constant_override("margin_top", MARGIN_PADDING)
	margin.add_theme_constant_override("margin_right", MARGIN_PADDING)
	margin.add_theme_constant_override("margin_bottom", MARGIN_PADDING)
	margin.add_child(content)

	add_child(_feedback_timer)
	add_child(margin)

func _register_events() -> void:
	if _events_registered:
		return

	_cleanup_uids_button.pressed.connect(_on_cleanup_uids_pressed)
	_nullable_button.pressed.connect(_on_nullable_pressed)
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
	_status_label.text = text
	_status_label.visible = not text.is_empty()
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
