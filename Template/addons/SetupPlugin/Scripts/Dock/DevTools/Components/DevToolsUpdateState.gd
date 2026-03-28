@tool
class_name DevToolsUpdateState
extends RefCounted

const EMPTY_TEXT: String = ""
const DEFAULT_TRACKED_TEXT: String = "Not tracked"
const DEFAULT_LATEST_TEXT: String = "Unknown"
const DEFAULT_CHECK_ON_STARTUP: bool = true
const CACHE_KEY_MAIN_COMMIT: String = "main_commit"
const CACHE_KEY_RELEASE_VERSION: String = "release_version"
const CACHE_KEY_AUTO_CHECK: String = "auto_check_on_startup"

var _cache_script: Script
var _tracked_main_commit: String = EMPTY_TEXT
var _tracked_release_version: String = EMPTY_TEXT
var _latest_main_commit: String = EMPTY_TEXT
var _latest_release_version: String = EMPTY_TEXT
var _check_updates_on_startup: bool = DEFAULT_CHECK_ON_STARTUP

func _init(cache_script: Script) -> void:
	_cache_script = cache_script

func load() -> void:
	var cache_state: Dictionary = _cache_script.load_state()
	_tracked_main_commit = str(cache_state.get(CACHE_KEY_MAIN_COMMIT, EMPTY_TEXT)).strip_edges()
	_tracked_release_version = str(cache_state.get(CACHE_KEY_RELEASE_VERSION, EMPTY_TEXT)).strip_edges()
	_check_updates_on_startup = bool(cache_state.get(CACHE_KEY_AUTO_CHECK, DEFAULT_CHECK_ON_STARTUP))

func save() -> void:
	_cache_script.save_state(_tracked_main_commit, _tracked_release_version, _check_updates_on_startup)

func clear_cache() -> bool:
	return bool(_cache_script.clear_state(_check_updates_on_startup))

func clear_tracked() -> void:
	_tracked_main_commit = EMPTY_TEXT
	_tracked_release_version = EMPTY_TEXT

func set_check_updates_on_startup(enabled: bool) -> void:
	_check_updates_on_startup = enabled

func get_check_updates_on_startup() -> bool:
	return _check_updates_on_startup

func get_tracked_main_commit() -> String:
	return _tracked_main_commit

func get_tracked_release_version() -> String:
	return _tracked_release_version

func get_latest_main_commit() -> String:
	return _latest_main_commit

func get_latest_release_version() -> String:
	return _latest_release_version

func set_tracked_main_commit(value: String) -> void:
	_tracked_main_commit = value

func set_tracked_release_version(value: String) -> void:
	_tracked_release_version = value

func set_latest_main_commit(value: String) -> void:
	_latest_main_commit = value

func set_latest_release_version(value: String) -> void:
	_latest_release_version = value

func refresh_tracked_labels(view: DevToolsUpdateView) -> void:
	var main_text: String = _tracked_main_commit if not _tracked_main_commit.is_empty() else DEFAULT_TRACKED_TEXT
	var release_text: String = _tracked_release_version if not _tracked_release_version.is_empty() else DEFAULT_TRACKED_TEXT
	view.set_tracked_labels(main_text, release_text)

func refresh_latest_labels(view: DevToolsUpdateView) -> void:
	var main_text: String = _latest_main_commit if not _latest_main_commit.is_empty() else DEFAULT_LATEST_TEXT
	var release_text: String = _latest_release_version if not _latest_release_version.is_empty() else DEFAULT_LATEST_TEXT
	view.set_latest_labels(main_text, release_text)

func refresh_button_state(view: DevToolsUpdateView, update_in_progress: bool, update_check_in_progress: bool) -> void:
	var controls_locked: bool = update_in_progress or update_check_in_progress
	var disable_main_for_no_update: bool = not _latest_main_commit.is_empty() and _tracked_main_commit == _latest_main_commit
	var disable_release_for_no_update: bool = not _latest_release_version.is_empty() and _tracked_release_version == _latest_release_version
	view.set_buttons_disabled(controls_locked, disable_main_for_no_update, disable_release_for_no_update)
