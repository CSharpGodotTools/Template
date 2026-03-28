@tool
class_name DevToolsUpdateRunner
extends RefCounted

const TEMP_DIRECTORY_TEMPLATE: String = ".godot/setup_plugin_update_%d"
const ARCHIVE_FILE_NAME: String = "template_update.zip"
const EXTRACT_DIR_NAME: String = "extracted"
const STATUS_DOWNLOADING: String = "Downloading template update..."
const STATUS_EXTRACTING: String = "Extracting archive..."
const STATUS_APPLYING: String = "Applying update files..."
const ERROR_TEMPLATE_MISSING: String = "Template folder was not found in the downloaded archive."
const ERROR_DOWNLOAD_FAILED: String = "Download failed."
const ERROR_EXTRACT_FAILED: String = "Extraction failed."
const ERROR_APPLY_FAILED: String = "Update apply failed."
const KEY_SUCCESS: String = "success"
const KEY_MESSAGE: String = "message"
const DEFAULT_SUCCESS: bool = false
const STATUS_CALLBACK_METHOD: StringName = &"set_status"

var _host: Node
var _project_root: String
var _status_view: DevToolsUpdateView
var _fetcher_script: Script
var _extractor_script: Script
var _applier_script: Script
var _file_ops_script: Script

func _init(host: Node, project_root: String, status_view: DevToolsUpdateView, fetcher_script: Script, extractor_script: Script, applier_script: Script, file_ops_script: Script) -> void:
	_host = host
	_project_root = project_root
	_status_view = status_view
	_fetcher_script = fetcher_script
	_extractor_script = extractor_script
	_applier_script = applier_script
	_file_ops_script = file_ops_script

func _build_temp_root() -> String:
	var timestamp: int = Time.get_unix_time_from_system()
	return _project_root.path_join(TEMP_DIRECTORY_TEMPLATE % timestamp)

func _download_archive(fetcher: Object, from_release: bool, archive_path: String) -> Dictionary:
	if from_release:
		return await fetcher.download_release_archive(_host, archive_path)
	return await fetcher.download_main_archive(_host, archive_path)

func _cleanup_temp(temp_root: String) -> void:
	_file_ops_script.delete_path_recursive(temp_root)

func _dictionary_success(result: Dictionary) -> bool:
	return bool(result.get(KEY_SUCCESS, DEFAULT_SUCCESS))

func _result_from_dictionary(result: Dictionary, fallback_message: String) -> DevToolsUpdateResult:
	var success: bool = _dictionary_success(result)
	var message: String = str(result.get(KEY_MESSAGE, fallback_message)).strip_edges()
	if message.is_empty():
		message = fallback_message
	return DevToolsUpdateResult.new(success, message)

func execute_update(from_release: bool) -> DevToolsUpdateResult:
	var temp_root: String = _build_temp_root()
	var archive_path: String = temp_root.path_join(ARCHIVE_FILE_NAME)
	var extract_root: String = temp_root.path_join(EXTRACT_DIR_NAME)
	DirAccess.make_dir_recursive_absolute(temp_root)

	var fetcher: Object = _fetcher_script.new()
	var extractor: Object = _extractor_script.new()
	var applier: Object = _applier_script.new()

	_status_view.set_status(STATUS_DOWNLOADING)
	var download_result: Dictionary = await _download_archive(fetcher, from_release, archive_path)
	if not _dictionary_success(download_result):
		_cleanup_temp(temp_root)
		return _result_from_dictionary(download_result, ERROR_DOWNLOAD_FAILED)

	_status_view.set_status(STATUS_EXTRACTING)
	var extract_result: Dictionary = extractor.extract_zip(archive_path, extract_root)
	if not _dictionary_success(extract_result):
		_cleanup_temp(temp_root)
		return _result_from_dictionary(extract_result, ERROR_EXTRACT_FAILED)

	var template_root: String = _file_ops_script.find_template_directory(extract_root)
	if template_root.is_empty():
		_cleanup_temp(temp_root)
		return DevToolsUpdateResult.new(false, ERROR_TEMPLATE_MISSING)

	_status_view.set_status(STATUS_APPLYING)
	var apply_result: Dictionary = applier.apply(template_root, _project_root, Callable(_status_view, STATUS_CALLBACK_METHOD))
	_cleanup_temp(temp_root)
	return _result_from_dictionary(apply_result, ERROR_APPLY_FAILED)
