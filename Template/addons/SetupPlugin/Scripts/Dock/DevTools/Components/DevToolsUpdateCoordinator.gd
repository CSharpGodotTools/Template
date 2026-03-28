@tool
class_name DevToolsUpdateCoordinator
extends RefCounted

const EMPTY_TEXT: String = ""
const STATUS_CHECKING_UPDATES: String = "Checking for template updates..."
const STATUS_UPDATE_AVAILABILITY_REFRESHED: String = "Update availability refreshed."
const STATUS_NO_UPDATE_NEEDED: String = "No update needed."
const STATUS_UPDATE_CACHE_RESET: String = "Update cache reset."
const STATUS_UPDATE_CACHE_RESET_FAILED: String = "Failed to reset update cache."
const STATUS_PROCEEDING_WITHOUT_METADATA_TEMPLATE: String = "Proceeding without metadata check: %s"
const FEEDBACK_METADATA_FAILED_TEMPLATE: String = "Could not refresh %s update metadata."
const FEEDBACK_METADATA_UNAVAILABLE_TEMPLATE: String = "Unable to refresh %s update metadata."
const FEEDBACK_METADATA_PROCEEDING: String = "Could not check latest metadata. Updating anyway."
const FEEDBACK_ALREADY_UP_TO_DATE_TEMPLATE: String = "Already up to date on %s"
const FEEDBACK_UPDATING_TO_TEMPLATE: String = "Updating to %s"
const FEEDBACK_UPDATING_LATEST: String = "Updating to latest available template"
const FEEDBACK_UPDATE_FAILED_TEMPLATE: String = "Update failed: %s"
const METADATA_MAIN_BRANCH: String = "main branch"
const METADATA_LATEST_RELEASE: String = "latest release"
const METADATA_JOIN_SEPARATOR: String = ", "
const REPO_URL: String = "https://github.com/CSharpGodotTools/Template"
const COMMITS_URL: String = "https://github.com/CSharpGodotTools/Template/commits/main/"
const RELEASES_URL: String = "https://github.com/CSharpGodotTools/Template/releases"
const URL_OPEN_FAILURE_TEMPLATE: String = "Failed to open URL: %s"
const ERROR_RESOURCE_FILESYSTEM_MISSING: String = "Resource filesystem is unavailable."

var _view: DevToolsUpdateView
var _runner: DevToolsUpdateRunner
var _state: DevToolsUpdateState
var _metadata_service: DevToolsUpdateMetadataService
var _update_check_in_progress: bool = false
var _update_in_progress: bool = false

func _init(view: DevToolsUpdateView, runner: DevToolsUpdateRunner, state: DevToolsUpdateState, metadata_service: DevToolsUpdateMetadataService) -> void:
	_view = view
	_runner = runner
	_state = state
	_metadata_service = metadata_service
func _run_update(from_release: bool) -> void:
	if _update_in_progress or _update_check_in_progress:
		return

	_update_in_progress = true
	_state.refresh_button_state(_view, _update_in_progress, _update_check_in_progress)

	var target_result: DevToolsUpdateResult = await _fetch_update_target(from_release)
	var target: String = EMPTY_TEXT
	if target_result.is_success():
		target = target_result.get_target()
	else:
		_view.show_feedback(FEEDBACK_METADATA_PROCEEDING)
		_view.set_status(STATUS_PROCEEDING_WITHOUT_METADATA_TEMPLATE % target_result.get_message())

	var tracked: String = _state.get_tracked_release_version() if from_release else _state.get_tracked_main_commit()
	if not target.is_empty() and tracked == target:
		_view.show_feedback(FEEDBACK_ALREADY_UP_TO_DATE_TEMPLATE % target)
		_view.set_status(STATUS_NO_UPDATE_NEEDED)
		_update_in_progress = false
		_state.refresh_button_state(_view, _update_in_progress, _update_check_in_progress)
		return

	if not target.is_empty():
		_view.show_feedback(FEEDBACK_UPDATING_TO_TEMPLATE % target)
	else:
		_view.show_feedback(FEEDBACK_UPDATING_LATEST)

	var result: DevToolsUpdateResult = await _runner.execute_update(from_release)
	_refresh_editor_filesystem()
	if result.is_success():
		if target.is_empty():
			var post_target: DevToolsUpdateResult = await _fetch_update_target(from_release)
			if post_target.is_success():
				target = post_target.get_target()
		if from_release:
			if not target.is_empty():
				_state.set_tracked_release_version(target)
				_state.set_latest_release_version(target)
		else:
			if not target.is_empty():
				_state.set_tracked_main_commit(target)
				_state.set_latest_main_commit(target)
		_state.save()
		_state.refresh_tracked_labels(_view)
		_state.refresh_latest_labels(_view)
		_view.set_status(result.get_message())
	else:
		_view.show_feedback(FEEDBACK_UPDATE_FAILED_TEMPLATE % result.get_message())
		_view.set_status(FEEDBACK_UPDATE_FAILED_TEMPLATE % result.get_message())

	_update_in_progress = false
	_state.refresh_button_state(_view, _update_in_progress, _update_check_in_progress)

func _fetch_update_target(from_release: bool) -> DevToolsUpdateResult:
	if from_release:
		var release_result: DevToolsUpdateResult = await _metadata_service.fetch_release_version()
		if release_result.is_success():
			_state.set_latest_release_version(release_result.get_target())
			_state.refresh_latest_labels(_view)
		return release_result

	var main_result: DevToolsUpdateResult = await _metadata_service.fetch_main_commit()
	if main_result.is_success():
		_state.set_latest_main_commit(main_result.get_target())
		_state.refresh_latest_labels(_view)
	return main_result

func _refresh_editor_filesystem() -> void:
	var resource_filesystem: EditorFileSystem = EditorInterface.get_resource_filesystem()
	if resource_filesystem == null:
		push_error(ERROR_RESOURCE_FILESYSTEM_MISSING)
		return
	resource_filesystem.scan()

func _open_url(url: String) -> void:
	var result: Error = OS.shell_open(url)
	if result != OK:
		push_error(URL_OPEN_FAILURE_TEMPLATE % url)
func initialize_state() -> void:
	_state.load()
	_view.set_check_updates_on_startup(_state.get_check_updates_on_startup())
	_state.refresh_tracked_labels(_view)
	_state.refresh_latest_labels(_view)
	_state.refresh_button_state(_view, _update_in_progress, _update_check_in_progress)

func should_check_updates_on_startup() -> bool:
	return _state.get_check_updates_on_startup()

func set_check_updates_on_startup(enabled: bool) -> void:
	_state.set_check_updates_on_startup(enabled)
	_state.save()

func check_for_updates_pressed() -> void:
	await check_for_updates(true)

func update_from_main() -> void:
	await _run_update(false)

func update_from_release() -> void:
	await _run_update(true)

func reset_update_cache() -> void:
	if _update_in_progress or _update_check_in_progress:
		return
	_state.clear_tracked()
	_state.refresh_tracked_labels(_view)
	_state.refresh_latest_labels(_view)
	var reset_ok: bool = _state.clear_cache()
	_view.show_feedback(STATUS_UPDATE_CACHE_RESET)
	_view.set_status(STATUS_UPDATE_CACHE_RESET if reset_ok else STATUS_UPDATE_CACHE_RESET_FAILED)
	await check_for_updates(false)

func open_template_repo() -> void:
	_open_url(REPO_URL)

func open_commits() -> void:
	_open_url(COMMITS_URL)

func open_release_notes() -> void:
	_open_url(RELEASES_URL)
func check_for_updates(show_success_status: bool) -> void:
	if _update_in_progress or _update_check_in_progress:
		return

	_update_check_in_progress = true
	_state.refresh_button_state(_view, _update_in_progress, _update_check_in_progress)
	_view.set_status(STATUS_CHECKING_UPDATES)

	var errors: Array[String] = []
	var main_result: DevToolsUpdateResult = await _metadata_service.fetch_main_commit()
	if main_result.is_success():
		_state.set_latest_main_commit(main_result.get_target())
	else:
		_state.set_latest_main_commit(EMPTY_TEXT)
		errors.append(METADATA_MAIN_BRANCH)

	var release_result: DevToolsUpdateResult = await _metadata_service.fetch_release_version()
	if release_result.is_success():
		_state.set_latest_release_version(release_result.get_target())
	else:
		_state.set_latest_release_version(EMPTY_TEXT)
		errors.append(METADATA_LATEST_RELEASE)

	_update_check_in_progress = false
	_state.refresh_latest_labels(_view)
	_state.refresh_button_state(_view, _update_in_progress, _update_check_in_progress)

	if not errors.is_empty():
		var error_text: String = METADATA_JOIN_SEPARATOR.join(errors)
		_view.show_feedback(FEEDBACK_METADATA_FAILED_TEMPLATE % error_text)
		_view.set_status(FEEDBACK_METADATA_UNAVAILABLE_TEMPLATE % error_text)
		return

	if show_success_status:
		_view.show_feedback(STATUS_UPDATE_AVAILABILITY_REFRESHED)
		_view.set_status(STATUS_UPDATE_AVAILABILITY_REFRESHED)
		return

	_view.set_status(EMPTY_TEXT)
