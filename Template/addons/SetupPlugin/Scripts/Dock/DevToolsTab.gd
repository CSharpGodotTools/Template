@tool
# Top-level logic for the Dev Tools dock tab.
# Instantiates editor service objects, connects all button signals, and
# implements every button handler (UID cleanup, nullable toggle, scene
# hierarchy controls, debugger clipboard, template update, and more).
class_name DevToolsTab
extends "res://addons/SetupPlugin/Scripts/Dock/DevToolsTabLayout.gd"

const PROJECT_ROOT_PATH: String = "res://"
const ANTI_ALIASING_PATH_3D: String = "rendering/anti_aliasing/quality/msaa_3d"
const DevToolsUidCleanupScript = preload("DevToolsUidCleanup.gd")
const EditorSceneActionsScript = preload("EditorSceneActions.gd")
const NullableProjectSettingsScript = preload("NullableProjectSettings.gd")
const SceneHierarchyActionsScript = preload("SceneHierarchyActions.gd")
const SetupDirectoryMaintenanceScript = preload("../Setup/SetupDirectoryMaintenance.gd")
const TemplateArchiveFetcherScript = preload("Update/TemplateArchiveFetcher.gd")
const TemplateArchiveExtractorScript = preload("Update/TemplateArchiveExtractor.gd")
const TemplateUpdateApplierScript = preload("Update/TemplateUpdateApplier.gd")
const TemplateUpdateCacheScript = preload("Update/TemplateUpdateCache.gd")
const UpdateFileOpsScript = preload("Update/UpdateFileOps.gd")

var _events_registered: bool
var _editor_scene_actions
var _scene_hierarchy_actions
var _tracked_main_commit: String = ""
var _tracked_release_version: String = ""
var _latest_main_commit: String = ""
var _latest_release_version: String = ""
var _update_check_in_progress: bool = false
var _update_in_progress: bool = false
var _check_updates_on_startup: bool = true

# Initialises service objects, builds the UI, then wires all button signals.
func _ready() -> void:
	_editor_scene_actions = EditorSceneActionsScript.new()
	_scene_hierarchy_actions = SceneHierarchyActionsScript.new()
	_create_controls()
	_update_nullable_button_text()
	_build_layout()
	_register_events()
	_load_update_cache_state()
	_refresh_tracked_update_labels()
	_refresh_latest_update_labels()
	_refresh_update_button_state()
	if _check_updates_on_startup:
		call_deferred("_check_for_updates", false)

# Disconnects all signals before the dock node is freed.
func prepare_for_disable() -> void:
	_unregister_events()

# Returns the absolute file-system path for the project root.
func _project_root() -> String:
	return ProjectSettings.globalize_path(PROJECT_ROOT_PATH)

# Connects every button pressed signal to its handler method.
# The guard prevents double-wiring on plugin hot-reload.
func _register_events() -> void:
	if _events_registered:
		return
	for pair in [[_cleanup_uids_button, _on_cleanup_uids_pressed], [_nullable_button, _on_nullable_pressed], [_remove_empty_folders_button, _on_remove_empty_folders_pressed], [_close_all_scene_tabs_button, _on_close_all_scene_tabs_pressed], [_restart_editor_button, _on_restart_editor_pressed], [_expand_to_level_button, _on_expand_to_level_pressed], [_fully_expand_button, _on_fully_expand_pressed], [_fully_collapse_button, _on_fully_collapse_pressed], [_update_from_main_button, _on_update_from_main_pressed], [_update_from_release_button, _on_update_from_release_pressed], [_check_updates_button, _on_check_updates_pressed], [_reset_update_cache_button, _on_reset_update_cache_pressed], [_view_template_repo_button, _on_view_template_repo_pressed], [_link_to_commits_button, _on_link_to_commits_pressed], [_link_to_release_notes_button, _on_link_to_release_notes_pressed]]:
		pair[0].pressed.connect(pair[1])
	_check_updates_on_startup_checkbox.toggled.connect(_on_check_updates_on_startup_toggled)
	_clear_color_picker.color_changed.connect(_on_clear_color_changed)
	_anti_aliasing_options.item_selected.connect(_on_anti_aliasing_item_selected)
	_feedback_timer.timeout.connect(_on_feedback_timer_timeout)
	_update_feedback_timer.timeout.connect(_on_update_feedback_timer_timeout)
	_events_registered = true

# Disconnects all signals. Called before the dock node is freed.
func _unregister_events() -> void:
	if not _events_registered:
		return
	_events_registered = false
	for pair in [[_cleanup_uids_button, "_on_cleanup_uids_pressed"], [_nullable_button, "_on_nullable_pressed"], [_remove_empty_folders_button, "_on_remove_empty_folders_pressed"], [_close_all_scene_tabs_button, "_on_close_all_scene_tabs_pressed"], [_restart_editor_button, "_on_restart_editor_pressed"], [_expand_to_level_button, "_on_expand_to_level_pressed"], [_fully_expand_button, "_on_fully_expand_pressed"], [_fully_collapse_button, "_on_fully_collapse_pressed"], [_update_from_main_button, "_on_update_from_main_pressed"], [_update_from_release_button, "_on_update_from_release_pressed"], [_check_updates_button, "_on_check_updates_pressed"], [_reset_update_cache_button, "_on_reset_update_cache_pressed"], [_view_template_repo_button, "_on_view_template_repo_pressed"], [_link_to_commits_button, "_on_link_to_commits_pressed"], [_link_to_release_notes_button, "_on_link_to_release_notes_pressed"]]:
		_disconnect_signal(pair[0], "pressed", pair[1])
	_disconnect_signal(_check_updates_on_startup_checkbox, "toggled", "_on_check_updates_on_startup_toggled")
	_disconnect_signal(_clear_color_picker, "color_changed", "_on_clear_color_changed")
	_disconnect_signal(_anti_aliasing_options, "item_selected", "_on_anti_aliasing_item_selected")
	_disconnect_signal(_feedback_timer, "timeout", "_on_feedback_timer_timeout")
	_disconnect_signal(_update_feedback_timer, "timeout", "_on_update_feedback_timer_timeout")

# Safely disconnects a single signal, silently skipping already-disconnected ones.
func _disconnect_signal(source: Object, signal_name: StringName, method_name: String) -> void:
	if source != null and source.is_connected(signal_name, Callable(self, method_name)):
		source.disconnect(signal_name, Callable(self, method_name))

# Updates the status label. An empty string resets it to the grey placeholder.
func _set_status(text: String) -> void:
	if text.is_empty():
		_status_label.text = " "
		_status_label.modulate = Color(0.75, 0.75, 0.75)
		return
	_status_label.text = text
	_status_label.modulate = Color(0.6, 0.95, 0.6)

# Updates the status label shown at the bottom of the Visual tab.
func _set_visual_status(text: String) -> void:
	if _visual_status_label == null:
		return
	if text.is_empty():
		_visual_status_label.text = " "
		_visual_status_label.modulate = Color(0.75, 0.75, 0.75)
		return
	_visual_status_label.text = text
	_visual_status_label.modulate = Color(0.6, 0.95, 0.6)

# Updates the status label in the Update tab only.
func _set_update_status(text: String) -> void:
	_set_update_feedback(text)

# Triggers a Godot filesystem rescan so the editor reflects any files that
# were created, modified, or deleted by a recent operation.
func _refresh_editor_filesystem() -> void:
	var resource_filesystem: EditorFileSystem = EditorInterface.get_resource_filesystem()
	if resource_filesystem != null:
		resource_filesystem.scan()

# Clears the status label once the feedback display duration elapses.
func _on_feedback_timer_timeout() -> void:
	_set_status("")
	_set_visual_status("")

# Updates the green update feedback label. An empty string hides its text.
func _set_update_feedback(text: String) -> void:
	if _update_warning_label == null:
		return
	if text.is_empty():
		_update_warning_label.text = UPDATE_WARNING_DEFAULT_TEXT
		return
	var safe_text: String = text.replace("[", "\\[").replace("]", "\\]")
	_update_warning_label.text = "[color=#99f299]%s[/color]" % safe_text

# Shows update feedback for a short duration under the update warning label.
func _show_update_feedback(text: String) -> void:
	print("[SetupPlugin][Update] %s" % text)
	_set_update_feedback(text)
	if _update_feedback_timer != null:
		_update_feedback_timer.start()

# Clears the update feedback label when its timer elapses.
func _on_update_feedback_timer_timeout() -> void:
	_set_update_feedback("")

# Recursively deletes .uid files whose corresponding source file no longer exists.
func _on_cleanup_uids_pressed() -> void:
	_cleanup_uids_button.disabled = true
	var removed_count: int = DevToolsUidCleanupScript.delete_orphan_uid_files(_project_root())
	_set_status("Removed %d old uid files." % removed_count if removed_count > 0 else "All uid files are good. No action needed.")
	_feedback_timer.start()
	_cleanup_uids_button.disabled = false

# Removes all empty directories from the project root, then rescans the filesystem.
func _on_remove_empty_folders_pressed() -> void:
	_remove_empty_folders_button.disabled = true
	var removed_count: int = SetupDirectoryMaintenanceScript.delete_empty_directories(_project_root())
	_refresh_editor_filesystem()
	_set_status("Removed %d empty folders." % removed_count if removed_count > 0 else "No empty folders found.")
	_feedback_timer.start()
	_remove_empty_folders_button.disabled = false

# Closes every open scene tab in the editor.
func _on_close_all_scene_tabs_pressed() -> void:
	var closed_count: int = _editor_scene_actions.close_all_open_scenes()
	_set_status("Closed %d scene tabs" % closed_count if closed_count > 0 else "No scene tabs to close")
	_feedback_timer.start()

# Saves the current scene and restarts the Godot editor process.
func _on_restart_editor_pressed() -> void:
	_set_status("Restarting editor...")
	_feedback_timer.start()
	_editor_scene_actions.restart_editor(true)

# Expands the Scene dock hierarchy to the depth set in the SpinBox.
func _on_expand_to_level_pressed() -> void:
	var level: int = int(_hierarchy_level_spinbox.value)
	var changed_count: int = _scene_hierarchy_actions.expand_to_level(level)
	_set_visual_status("Expanded hierarchy to level %d" % level if changed_count > 0 else "No scene hierarchy available")
	_feedback_timer.start()

# Fully expands every node in the Scene dock hierarchy.
func _on_fully_expand_pressed() -> void:
	_set_visual_status("Fully expanded hierarchy" if _scene_hierarchy_actions.fully_expand() > 0 else "No scene hierarchy available")
	_feedback_timer.start()

# Collapses the Scene dock hierarchy to just the root level.
func _on_fully_collapse_pressed() -> void:
	_set_visual_status("Fully collapsed hierarchy" if _scene_hierarchy_actions.fully_collapse() > 0 else "No scene hierarchy available")
	_feedback_timer.start()

# Persists the chosen viewport clear colour to project settings immediately.
func _on_clear_color_changed(color: Color) -> void:
	ProjectSettings.set_setting(DEFAULT_CLEAR_COLOR_PATH, color)
	ProjectSettings.save()
	_set_visual_status("Updated clear color.")
	_feedback_timer.start()

# Writes the chosen MSAA level to both 2D and 3D project settings.
func _on_anti_aliasing_item_selected(index: int) -> void:
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_2D, index)
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_3D, index)
	_set_visual_status("Anti-aliasing updated.")
	_feedback_timer.start()

# Reads the current nullable state from Template.csproj and sets the button
# label to "Enable Nullable" or "Disable Nullable" accordingly.
func _update_nullable_button_text() -> void:
	_nullable_button.text = "Disable Nullable" if NullableProjectSettingsScript.read_state(_project_root()) else "Enable Nullable"

# Toggles C# nullable reference types in Template.csproj and .editorconfig.
func _on_nullable_pressed() -> void:
	var new_state: bool = not NullableProjectSettingsScript.read_state(_project_root())
	NullableProjectSettingsScript.set_state(_project_root(), new_state)
	_update_nullable_button_text()
	_set_status("Nullable %s. Updated .editorconfig and .csproj." % ("enabled" if new_state else "disabled"))
	_feedback_timer.start()

# Loads previously tracked update identifiers from persistent cache.
func _load_update_cache_state() -> void:
	var cache_state: Dictionary = TemplateUpdateCacheScript.load_state()
	_tracked_main_commit = str(cache_state.get("main_commit", "")).strip_edges()
	_tracked_release_version = str(cache_state.get("release_version", "")).strip_edges()
	_check_updates_on_startup = bool(cache_state.get("auto_check_on_startup", true))
	if _check_updates_on_startup_checkbox != null:
		_check_updates_on_startup_checkbox.button_pressed = _check_updates_on_startup

# Saves tracked identifiers to persistent cache.
func _save_update_cache_state() -> void:
	TemplateUpdateCacheScript.save_state(_tracked_main_commit, _tracked_release_version, _check_updates_on_startup)

# Toggles startup update checks and persists the preference.
func _on_check_updates_on_startup_toggled(enabled: bool) -> void:
	_check_updates_on_startup = enabled
	_save_update_cache_state()

# Refreshes the two labels that expose currently tracked commit/version values.
func _refresh_tracked_update_labels() -> void:
	if _tracked_main_commit_label != null:
		_tracked_main_commit_label.text = _tracked_main_commit if not _tracked_main_commit.is_empty() else "Not tracked"
	if _tracked_release_version_label != null:
		_tracked_release_version_label.text = _tracked_release_version if not _tracked_release_version.is_empty() else "Not tracked"

# Refreshes labels that show the latest known upstream commit/version.
func _refresh_latest_update_labels() -> void:
	if _latest_main_commit_label != null:
		_latest_main_commit_label.text = _latest_main_commit if not _latest_main_commit.is_empty() else "Unknown"
	if _latest_release_version_label != null:
		_latest_release_version_label.text = _latest_release_version if not _latest_release_version.is_empty() else "Unknown"

# Applies enabled/disabled state to update controls based on availability checks
# and in-flight operations.
func _refresh_update_button_state() -> void:
	var controls_locked: bool = _update_in_progress or _update_check_in_progress
	if _check_updates_button != null:
		_check_updates_button.disabled = controls_locked
	if _reset_update_cache_button != null:
		_reset_update_cache_button.disabled = controls_locked

	var disable_main_for_no_update: bool = not _latest_main_commit.is_empty() and _tracked_main_commit == _latest_main_commit
	var disable_release_for_no_update: bool = not _latest_release_version.is_empty() and _tracked_release_version == _latest_release_version
	if _update_from_main_button != null:
		_update_from_main_button.disabled = controls_locked or disable_main_for_no_update
	if _update_from_release_button != null:
		_update_from_release_button.disabled = controls_locked or disable_release_for_no_update

# Refreshes latest remote commit/version metadata used for update availability.
func _check_for_updates(show_success_status: bool) -> void:
	if _update_in_progress or _update_check_in_progress:
		return

	_update_check_in_progress = true
	_refresh_update_button_state()
	_set_update_status("Checking for template updates...")

	var fetcher = TemplateArchiveFetcherScript.new()
	var main_result: Dictionary = await fetcher.fetch_latest_main_commit(self)
	var release_result: Dictionary = await fetcher.fetch_latest_release_version(self)

	var errors: Array[String] = []
	if main_result.get("success", false):
		_latest_main_commit = str(main_result.get("full_commit", main_result.get("commit", ""))).strip_edges()
	else:
		_latest_main_commit = ""
		errors.append("main branch")

	if release_result.get("success", false):
		_latest_release_version = str(release_result.get("version", "")).strip_edges()
	else:
		_latest_release_version = ""
		errors.append("latest release")

	_update_check_in_progress = false
	_refresh_latest_update_labels()
	_refresh_update_button_state()

	if not errors.is_empty():
		_show_update_feedback("Could not refresh %s update metadata." % ", ".join(errors))
		_set_update_status("Unable to refresh %s update metadata." % ", ".join(errors))
		_update_feedback_timer.start()
	elif show_success_status:
		_show_update_feedback("Update availability refreshed.")
		_set_update_status("Update availability refreshed.")
		_update_feedback_timer.start()
	else:
		_set_update_status("")

# Starts an update from the latest commit on the main branch.
func _on_update_from_main_pressed() -> void:
	await _run_template_update(false)

# Starts an update from the latest tagged GitHub release.
func _on_update_from_release_pressed() -> void:
	await _run_template_update(true)

# Manually refreshes remote update metadata and button availability.
func _on_check_updates_pressed() -> void:
	await _check_for_updates(true)

# Clears tracked commit/version so updates can be retried after manual changes.
func _on_reset_update_cache_pressed() -> void:
	if _update_in_progress or _update_check_in_progress:
		return
	_tracked_main_commit = ""
	_tracked_release_version = ""
	_refresh_tracked_update_labels()
	_refresh_latest_update_labels()
	var reset_ok: bool = TemplateUpdateCacheScript.clear_state(_check_updates_on_startup)
	_show_update_feedback("Update cache reset.")
	_set_update_status("Update cache reset." if reset_ok else "Failed to reset update cache.")
	_update_feedback_timer.start()
	await _check_for_updates(false)

# Opens the CSharpGodotTools/Template repository in the default browser.
func _on_view_template_repo_pressed() -> void:
	OS.shell_open("https://github.com/CSharpGodotTools/Template")

# Opens the commit history page for the template repository in the default browser.
func _on_link_to_commits_pressed() -> void:
	OS.shell_open("https://github.com/CSharpGodotTools/Template/commits/main/")

# Opens the release notes page for the template repository in the default browser.
func _on_link_to_release_notes_pressed() -> void:
	OS.shell_open("https://github.com/CSharpGodotTools/Template/releases")

# Runs one update flow (main or release) after validating whether a new
# commit/version is available for that source.
func _run_template_update(from_release: bool) -> void:
	if _update_in_progress or _update_check_in_progress:
		return

	_update_in_progress = true
	_refresh_update_button_state()

	var target_result: Dictionary = await _fetch_update_target(from_release)
	var target: String = ""
	if target_result.get("success", false):
		target = str(target_result.get("target", "")).strip_edges()
	else:
		_show_update_feedback("Could not check latest metadata. Updating anyway.")
		_set_update_status("Proceeding without metadata check: %s" % target_result.get("message", "Unknown metadata error."))
		_update_feedback_timer.start()

	var tracked: String = _tracked_release_version if from_release else _tracked_main_commit
	if not target.is_empty() and tracked == target:
		_show_update_feedback("Already up to date on %s" % target)
		_set_update_status("No update needed.")
		_update_feedback_timer.start()
		_update_in_progress = false
		_refresh_update_button_state()
		return

	_show_update_feedback("Updating to %s" % target if not target.is_empty() else "Updating to latest available template")
	var result: Dictionary = await _execute_template_update(from_release)

	_refresh_editor_filesystem()
	if result.get("success", false):
		if target.is_empty():
			var post_update_target_result: Dictionary = await _fetch_update_target(from_release)
			if post_update_target_result.get("success", false):
				target = str(post_update_target_result.get("target", "")).strip_edges()
		if from_release:
			if not target.is_empty():
				_tracked_release_version = target
				_latest_release_version = target
		else:
			if not target.is_empty():
				_tracked_main_commit = target
				_latest_main_commit = target
		_save_update_cache_state()
		_refresh_tracked_update_labels()
		_refresh_latest_update_labels()
		_set_update_status(result.get("message", "Update finished successfully."))
	else:
		var failure_message: String = str(result.get("message", "Unknown error."))
		_show_update_feedback("Update failed: %s" % failure_message)
		_set_update_status("Update failed: %s" % failure_message)
	_update_feedback_timer.start()
	_update_in_progress = false
	_refresh_update_button_state()

# Fetches the current remote update target identifier for main/release.
func _fetch_update_target(from_release: bool) -> Dictionary:
	var fetcher = TemplateArchiveFetcherScript.new()
	if from_release:
		var release_result: Dictionary = await fetcher.fetch_latest_release_version(self)
		if not release_result.get("success", false):
			return release_result
		_latest_release_version = str(release_result.get("version", "")).strip_edges()
		_refresh_latest_update_labels()
		return {"success": true, "target": _latest_release_version}

	var main_result: Dictionary = await fetcher.fetch_latest_main_commit(self)
	if not main_result.get("success", false):
		return main_result
	_latest_main_commit = str(main_result.get("full_commit", main_result.get("commit", ""))).strip_edges()
	_refresh_latest_update_labels()
	return {"success": true, "target": _latest_main_commit}

# Full async update pipeline: creates a timestamped temp directory, downloads
# the archive, extracts it, locates the template root inside the extraction,
# applies the update to the project, then removes the temp directory.
func _execute_template_update(from_release: bool) -> Dictionary:
	var project_root: String = _project_root()
	var temp_root: String = project_root.path_join(".godot/setup_plugin_update_%d" % Time.get_unix_time_from_system())
	var archive_path: String = temp_root.path_join("template_update.zip")
	var extract_root: String = temp_root.path_join("extracted")
	DirAccess.make_dir_recursive_absolute(temp_root)

	var fetcher = TemplateArchiveFetcherScript.new()
	var extractor = TemplateArchiveExtractorScript.new()
	var applier = TemplateUpdateApplierScript.new()

	_set_update_status("Downloading template update...")
	var download_result: Dictionary
	if from_release:
		download_result = await fetcher.download_release_archive(self, archive_path)
	else:
		download_result = await fetcher.download_main_archive(self, archive_path)
	if not download_result.get("success", false):
		UpdateFileOpsScript.delete_path_recursive(temp_root)
		return download_result

	_set_update_status("Extracting archive...")
	var extract_result: Dictionary = extractor.extract_zip(archive_path, extract_root)
	if not extract_result.get("success", false):
		UpdateFileOpsScript.delete_path_recursive(temp_root)
		return extract_result

	var template_root: String = UpdateFileOpsScript.find_template_directory(extract_root)
	if template_root.is_empty():
		UpdateFileOpsScript.delete_path_recursive(temp_root)
		return {"success": false, "message": "Template folder was not found in the downloaded archive."}

	_set_update_status("Applying update files...")
	var apply_result: Dictionary = applier.apply(template_root, project_root, Callable(self, "_set_update_status"))
	UpdateFileOpsScript.delete_path_recursive(temp_root)
	return apply_result
