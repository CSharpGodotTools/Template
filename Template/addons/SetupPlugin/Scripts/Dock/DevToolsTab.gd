@tool
class_name DevToolsTab
extends VBoxContainer

const PROJECT_ROOT_PATH: String = "res://"
const CSPROJ_PATH: String = "res://Template.csproj"
const EDITORCONFIG_PATH: String = "res://.editorconfig"
const MARGIN_PADDING: int = 30
const FEEDBACK_DURATION: float = 5.0
const CS8632_SUPPRESSION: String = "dotnet_diagnostic.CS8632.severity = none # The annotation for nullable reference types should only be used in code within a '#nullable' annotations context."

var _status_label: Label
var _cleanup_uids_button: Button
var _nullable_button: Button
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
	_cleanup_uids_button.custom_minimum_size = Vector2(200, 0)

	_nullable_button = Button.new()
	_nullable_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_nullable_button.custom_minimum_size = Vector2(200, 0)
	_update_nullable_button_text()

	_feedback_timer = Timer.new()
	_feedback_timer.wait_time = FEEDBACK_DURATION
	_feedback_timer.one_shot = true

func _build_layout() -> void:
	var content: VBoxContainer = VBoxContainer.new()
	content.add_child(_status_label)

	var margin: MarginContainer = MarginContainer.new()
	margin.add_theme_constant_override("margin_left", MARGIN_PADDING)
	margin.add_theme_constant_override("margin_top", MARGIN_PADDING)
	margin.add_theme_constant_override("margin_right", MARGIN_PADDING)
	margin.add_theme_constant_override("margin_bottom", MARGIN_PADDING)
	margin.add_child(content)

	add_child(_feedback_timer)
	add_child(margin)
	add_child(_cleanup_uids_button)
	add_child(_nullable_button)

func _register_events() -> void:
	if _events_registered:
		return

	_cleanup_uids_button.pressed.connect(_on_cleanup_uids_pressed)
	_nullable_button.pressed.connect(_on_nullable_pressed)
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

func _set_status(text: String) -> void:
	_status_label.text = text
	_status_label.visible = not text.is_empty()
	_status_label.modulate = Color(0.6, 0.95, 0.6)

func _on_feedback_timer_timeout() -> void:
	_set_status("")

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
	_set_editorconfig_cs8632(not new_state)
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

func _set_editorconfig_cs8632(suppress: bool) -> void:
	var path: String = ProjectSettings.globalize_path(EDITORCONFIG_PATH)
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return
	var content: String = file.get_as_text()
	file.close()

	if suppress:
		if content.contains(CS8632_SUPPRESSION):
			return
		# Insert after the CA1816 line in the "Suppressed suggestions" section
		var lines: PackedStringArray = content.split("\n")
		var result: PackedStringArray = []
		for line in lines:
			result.append(line)
			if line.begins_with("dotnet_diagnostic.CA1816.severity = none"):
				result.append(CS8632_SUPPRESSION)
		content = "\n".join(result)
	else:
		# Remove the CS8632 suppression line
		var lines: PackedStringArray = content.split("\n")
		var result: PackedStringArray = []
		for line in lines:
			if not line.begins_with("dotnet_diagnostic.CS8632.severity = none"):
				result.append(line)
		content = "\n".join(result)

	var write_file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if write_file == null:
		return
	write_file.store_string(content)
	write_file.close()
