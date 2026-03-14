@tool
class_name DevToolsTab
extends "res://addons/SetupPlugin/Scripts/Dock/DevToolsTabLayout.gd"

const PROJECT_ROOT_PATH: String = "res://"
const ANTI_ALIASING_PATH_3D: String = "rendering/anti_aliasing/quality/msaa_3d"
const DebuggerErrorClipboardScript = preload("res://addons/SetupPlugin/Scripts/Dock/DebuggerErrorClipboard.gd")
const DevToolsUidCleanupScript = preload("res://addons/SetupPlugin/Scripts/Dock/DevToolsUidCleanup.gd")
const EditorSceneActionsScript = preload("res://addons/SetupPlugin/Scripts/Dock/EditorSceneActions.gd")
const NullableProjectSettingsScript = preload("res://addons/SetupPlugin/Scripts/Dock/NullableProjectSettings.gd")
const SceneHierarchyActionsScript = preload("res://addons/SetupPlugin/Scripts/Dock/SceneHierarchyActions.gd")
const SetupDirectoryMaintenanceScript = preload("res://addons/SetupPlugin/Scripts/Setup/SetupDirectoryMaintenance.gd")

var _events_registered: bool
var _debugger_error_clipboard
var _editor_scene_actions
var _scene_hierarchy_actions

func _ready() -> void:
	_debugger_error_clipboard = DebuggerErrorClipboardScript.new()
	_editor_scene_actions = EditorSceneActionsScript.new()
	_scene_hierarchy_actions = SceneHierarchyActionsScript.new()
	_create_controls()
	_update_nullable_button_text()
	_build_layout()
	_register_events()

func prepare_for_disable() -> void:
	_unregister_events()

func _project_root() -> String:
	return ProjectSettings.globalize_path(PROJECT_ROOT_PATH)

func _register_events() -> void:
	if _events_registered:
		return
	for pair in [[_cleanup_uids_button, _on_cleanup_uids_pressed], [_nullable_button, _on_nullable_pressed], [_remove_empty_folders_button, _on_remove_empty_folders_pressed], [_copy_debugger_errors_button, _on_copy_debugger_errors_pressed], [_close_all_scene_tabs_button, _on_close_all_scene_tabs_pressed], [_restart_editor_button, _on_restart_editor_pressed], [_expand_to_level_button, _on_expand_to_level_pressed], [_fully_expand_button, _on_fully_expand_pressed], [_fully_collapse_button, _on_fully_collapse_pressed], [_update_from_main_button, _on_update_from_main_pressed], [_update_from_release_button, _on_update_from_release_pressed]]:
		pair[0].pressed.connect(pair[1])
	_clear_color_picker.color_changed.connect(_on_clear_color_changed)
	_anti_aliasing_options.item_selected.connect(_on_anti_aliasing_item_selected)
	_feedback_timer.timeout.connect(_on_feedback_timer_timeout)
	_events_registered = true

func _unregister_events() -> void:
	if not _events_registered:
		return
	_events_registered = false
	for pair in [[_cleanup_uids_button, "_on_cleanup_uids_pressed"], [_nullable_button, "_on_nullable_pressed"], [_remove_empty_folders_button, "_on_remove_empty_folders_pressed"], [_copy_debugger_errors_button, "_on_copy_debugger_errors_pressed"], [_close_all_scene_tabs_button, "_on_close_all_scene_tabs_pressed"], [_restart_editor_button, "_on_restart_editor_pressed"], [_expand_to_level_button, "_on_expand_to_level_pressed"], [_fully_expand_button, "_on_fully_expand_pressed"], [_fully_collapse_button, "_on_fully_collapse_pressed"], [_update_from_main_button, "_on_update_from_main_pressed"], [_update_from_release_button, "_on_update_from_release_pressed"]]:
		_disconnect_signal(pair[0], "pressed", pair[1])
	_disconnect_signal(_clear_color_picker, "color_changed", "_on_clear_color_changed")
	_disconnect_signal(_anti_aliasing_options, "item_selected", "_on_anti_aliasing_item_selected")
	_disconnect_signal(_feedback_timer, "timeout", "_on_feedback_timer_timeout")

func _disconnect_signal(source: Object, signal_name: StringName, method_name: String) -> void:
	if source != null and source.is_connected(signal_name, Callable(self, method_name)):
		source.disconnect(signal_name, Callable(self, method_name))

func _set_status(text: String) -> void:
	if text.is_empty():
		_status_label.text = " "
		_status_label.modulate = Color(0.75, 0.75, 0.75)
		return
	_status_label.text = text
	_status_label.modulate = Color(0.6, 0.95, 0.6)

func _refresh_editor_filesystem() -> void:
	var resource_filesystem: EditorFileSystem = EditorInterface.get_resource_filesystem()
	if resource_filesystem != null:
		resource_filesystem.scan()

func _on_feedback_timer_timeout() -> void:
	_set_status("")

func _on_cleanup_uids_pressed() -> void:
	_cleanup_uids_button.disabled = true
	var removed_count: int = DevToolsUidCleanupScript.delete_orphan_uid_files(_project_root())
	_set_status("Removed %d old uid files." % removed_count if removed_count > 0 else "All uid files are good. No action needed.")
	_feedback_timer.start()
	_cleanup_uids_button.disabled = false

func _on_remove_empty_folders_pressed() -> void:
	_remove_empty_folders_button.disabled = true
	var removed_count: int = SetupDirectoryMaintenanceScript.delete_empty_directories(_project_root())
	_refresh_editor_filesystem()
	_set_status("Removed %d empty folders." % removed_count if removed_count > 0 else "No empty folders found.")
	_feedback_timer.start()
	_remove_empty_folders_button.disabled = false

func _on_copy_debugger_errors_pressed() -> void:
	var errors: PackedStringArray = _debugger_error_clipboard.collect_errors(_include_stack_trace_checkbox.button_pressed, _use_short_type_names_checkbox.button_pressed)
	if errors.is_empty():
		_set_status("No errors to copy to clipboard")
		_feedback_timer.start()
		return
	DisplayServer.clipboard_set("\n\n".join(errors))
	_set_status("Copied %d errors to clipboard" % errors.size())
	_feedback_timer.start()

func _on_close_all_scene_tabs_pressed() -> void:
	var closed_count: int = _editor_scene_actions.close_all_open_scenes()
	_set_status("Closed %d scene tabs" % closed_count if closed_count > 0 else "No scene tabs to close")
	_feedback_timer.start()

func _on_restart_editor_pressed() -> void:
	_set_status("Restarting editor...")
	_feedback_timer.start()
	_editor_scene_actions.restart_editor(true)

func _on_expand_to_level_pressed() -> void:
	var level: int = int(_hierarchy_level_spinbox.value)
	var changed_count: int = _scene_hierarchy_actions.expand_to_level(level)
	_set_status("Expanded hierarchy to level %d" % level if changed_count > 0 else "No scene hierarchy available")
	_feedback_timer.start()

func _on_fully_expand_pressed() -> void:
	_set_status("Fully expanded hierarchy" if _scene_hierarchy_actions.fully_expand() > 0 else "No scene hierarchy available")
	_feedback_timer.start()

func _on_fully_collapse_pressed() -> void:
	_set_status("Fully collapsed hierarchy" if _scene_hierarchy_actions.fully_collapse() > 0 else "No scene hierarchy available")
	_feedback_timer.start()

func _on_clear_color_changed(color: Color) -> void:
	ProjectSettings.set_setting(DEFAULT_CLEAR_COLOR_PATH, color)
	ProjectSettings.save()

func _on_anti_aliasing_item_selected(index: int) -> void:
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_2D, index)
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_3D, index)

func _update_nullable_button_text() -> void:
	_nullable_button.text = "Disable Nullable" if NullableProjectSettingsScript.read_state(_project_root()) else "Enable Nullable"

func _on_nullable_pressed() -> void:
	var new_state: bool = not NullableProjectSettingsScript.read_state(_project_root())
	NullableProjectSettingsScript.set_state(_project_root(), new_state)
	_update_nullable_button_text()
	_set_status("Nullable %s. Rebuild the project to apply." % ("enabled" if new_state else "disabled"))
	_feedback_timer.start()

func _set_update_buttons_disabled(disabled: bool) -> void:
	_update_from_main_button.disabled = disabled
	_update_from_release_button.disabled = disabled

func _on_update_from_main_pressed() -> void:
	await _run_template_update(false)

func _on_update_from_release_pressed() -> void:
	await _run_template_update(true)

func _run_template_update(from_release: bool) -> void:
	_set_update_buttons_disabled(true)
	var result: Dictionary = await _execute_template_update(from_release)

	_refresh_editor_filesystem()
	if result.get("success", false):
		_set_status(result.get("message", "Update finished successfully."))
	else:
		_set_status("Update failed: %s" % result.get("message", "Unknown error."))
	_feedback_timer.start()
	_set_update_buttons_disabled(false)

func _execute_template_update(from_release: bool) -> Dictionary:
	var project_root: String = _project_root()
	var temp_root: String = project_root.path_join(".godot/setup_plugin_update_%d" % Time.get_unix_time_from_system())
	var archive_path: String = temp_root.path_join("template_update.zip")
	var extract_root: String = temp_root.path_join("extracted")
	DirAccess.make_dir_recursive_absolute(temp_root)

	var fetcher = load("res://addons/SetupPlugin/Scripts/Dock/Update/TemplateArchiveFetcher.gd").new()
	var extractor = load("res://addons/SetupPlugin/Scripts/Dock/Update/TemplateArchiveExtractor.gd").new()
	var applier = load("res://addons/SetupPlugin/Scripts/Dock/Update/TemplateUpdateApplier.gd").new()
	var update_file_ops = load("res://addons/SetupPlugin/Scripts/Dock/Update/UpdateFileOps.gd")

	_set_status("Downloading template update...")
	var download_result: Dictionary
	if from_release:
		download_result = await fetcher.download_release_archive(self, archive_path)
	else:
		download_result = await fetcher.download_main_archive(self, archive_path)
	if not download_result.get("success", false):
		update_file_ops.delete_path_recursive(temp_root)
		return download_result

	_set_status("Extracting archive...")
	var extract_result: Dictionary = extractor.extract_zip(archive_path, extract_root)
	if not extract_result.get("success", false):
		update_file_ops.delete_path_recursive(temp_root)
		return extract_result

	var template_root: String = update_file_ops.find_template_directory(extract_root)
	if template_root.is_empty():
		update_file_ops.delete_path_recursive(temp_root)
		return {"success": false, "message": "Template folder was not found in the downloaded archive."}

	_set_status("Applying update files...")
	var apply_result: Dictionary = applier.apply(template_root, project_root, Callable(self, "_set_status"))
	update_file_ops.delete_path_recursive(temp_root)
	return apply_result
