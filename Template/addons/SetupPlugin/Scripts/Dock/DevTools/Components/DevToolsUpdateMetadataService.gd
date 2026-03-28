@tool
class_name DevToolsUpdateMetadataService
extends RefCounted

const EMPTY_TEXT: String = ""
const FETCHER_KEY_SUCCESS: String = "success"
const FETCHER_KEY_FULL_COMMIT: String = "full_commit"
const FETCHER_KEY_COMMIT: String = "commit"
const FETCHER_KEY_VERSION: String = "version"
const FETCHER_KEY_MESSAGE: String = "message"
const DEFAULT_FETCH_SUCCESS: bool = false

var _host: Node
var _fetcher_script: Script

func _init(host: Node, fetcher_script: Script) -> void:
	_host = host
	_fetcher_script = fetcher_script

func _dictionary_success(result: Dictionary) -> bool:
	return bool(result.get(FETCHER_KEY_SUCCESS, DEFAULT_FETCH_SUCCESS))

func _extract_main_commit(result: Dictionary) -> String:
	var full_commit: String = str(result.get(FETCHER_KEY_FULL_COMMIT, EMPTY_TEXT)).strip_edges()
	if not full_commit.is_empty():
		return full_commit
	return str(result.get(FETCHER_KEY_COMMIT, EMPTY_TEXT)).strip_edges()

func _extract_release_version(result: Dictionary) -> String:
	return str(result.get(FETCHER_KEY_VERSION, EMPTY_TEXT)).strip_edges()

func _result_from_dictionary(result: Dictionary) -> DevToolsUpdateResult:
	var message: String = str(result.get(FETCHER_KEY_MESSAGE, EMPTY_TEXT)).strip_edges()
	return DevToolsUpdateResult.new(false, message)

func fetch_main_commit() -> DevToolsUpdateResult:
	var fetcher: Object = _fetcher_script.new()
	var result: Dictionary = await fetcher.fetch_latest_main_commit(_host)
	if not _dictionary_success(result):
		return _result_from_dictionary(result)
	var commit: String = _extract_main_commit(result)
	return DevToolsUpdateResult.new(true, EMPTY_TEXT, commit)

func fetch_release_version() -> DevToolsUpdateResult:
	var fetcher: Object = _fetcher_script.new()
	var result: Dictionary = await fetcher.fetch_latest_release_version(_host)
	if not _dictionary_success(result):
		return _result_from_dictionary(result)
	var version: String = _extract_release_version(result)
	return DevToolsUpdateResult.new(true, EMPTY_TEXT, version)
