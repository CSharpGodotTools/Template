@tool
class_name DevToolsProjectToolsController
extends RefCounted

const EMPTY_COUNT: int = 0
const STATUS_UIDS_REMOVED_TEMPLATE: String = "Removed %d old uid files."
const STATUS_UIDS_OK: String = "All uid files are good. No action needed."
const STATUS_EMPTY_FOLDERS_REMOVED_TEMPLATE: String = "Removed %d empty folders."
const STATUS_EMPTY_FOLDERS_NONE: String = "No empty folders found."
const STATUS_NULLABLE_TEMPLATE: String = "Nullable %s. Updated .editorconfig and .csproj."
const NULLABLE_ENABLED: String = "enabled"
const NULLABLE_DISABLED: String = "disabled"
const BUTTON_ENABLE_NULLABLE: String = "Enable Nullable"
const BUTTON_DISABLE_NULLABLE: String = "Disable Nullable"
const ERROR_RESOURCE_FILESYSTEM_MISSING: String = "Resource filesystem is unavailable."

var _project_root: String
var _status_feedback: DevToolsStatusFeedback
var _cleanup_uids_button: Button
var _remove_empty_folders_button: Button
var _nullable_button: Button
var _uid_cleanup_script: Script
var _directory_maintenance_script: Script
var _nullable_settings_script: Script

func _init(project_root: String, status_feedback: DevToolsStatusFeedback, cleanup_uids_button: Button, remove_empty_folders_button: Button, nullable_button: Button, uid_cleanup_script: Script, directory_maintenance_script: Script, nullable_settings_script: Script) -> void:
	_project_root = project_root
	_status_feedback = status_feedback
	_cleanup_uids_button = cleanup_uids_button
	_remove_empty_folders_button = remove_empty_folders_button
	_nullable_button = nullable_button
	_uid_cleanup_script = uid_cleanup_script
	_directory_maintenance_script = directory_maintenance_script
	_nullable_settings_script = nullable_settings_script
	_refresh_nullable_button_text()

func _refresh_nullable_button_text() -> void:
	if _nullable_button == null:
		return
	_nullable_button.text = BUTTON_DISABLE_NULLABLE if _read_nullable_state() else BUTTON_ENABLE_NULLABLE

func _read_nullable_state() -> bool:
	return bool(_nullable_settings_script.read_state(_project_root))

func _refresh_editor_filesystem() -> void:
	var resource_filesystem: EditorFileSystem = EditorInterface.get_resource_filesystem()
	if resource_filesystem == null:
		push_error(ERROR_RESOURCE_FILESYSTEM_MISSING)
		return
	resource_filesystem.scan()

func cleanup_uids() -> void:
	if _cleanup_uids_button != null:
		_cleanup_uids_button.disabled = true
	var removed_count: int = int(_uid_cleanup_script.delete_orphan_uid_files(_project_root))
	var message: String = STATUS_UIDS_REMOVED_TEMPLATE % removed_count if removed_count > EMPTY_COUNT else STATUS_UIDS_OK
	_status_feedback.show(message)
	if _cleanup_uids_button != null:
		_cleanup_uids_button.disabled = false

func remove_empty_folders() -> void:
	if _remove_empty_folders_button != null:
		_remove_empty_folders_button.disabled = true
	var removed_count: int = int(_directory_maintenance_script.delete_empty_directories(_project_root))
	_refresh_editor_filesystem()
	var message: String = STATUS_EMPTY_FOLDERS_REMOVED_TEMPLATE % removed_count if removed_count > EMPTY_COUNT else STATUS_EMPTY_FOLDERS_NONE
	_status_feedback.show(message)
	if _remove_empty_folders_button != null:
		_remove_empty_folders_button.disabled = false

func toggle_nullable() -> void:
	var new_state: bool = not _read_nullable_state()
	_nullable_settings_script.set_state(_project_root, new_state)
	_refresh_nullable_button_text()
	var status: String = NULLABLE_ENABLED if new_state else NULLABLE_DISABLED
	_status_feedback.show(STATUS_NULLABLE_TEMPLATE % status)
