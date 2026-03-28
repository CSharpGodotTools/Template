@tool
extends RefCounted

var _source_path_cache: Dictionary = {}

func parse_source_line(line: String) -> Dictionary:
	if not line.begins_with("Source:"):
		return {}
	var payload: String = line.trim_prefix("Source:").strip_edges()
	var separator: int = payload.rfind(":")
	if separator <= 0:
		return {}

	var path: String = payload.substr(0, separator).strip_edges()
	var line_text: String = payload.substr(separator + 1).strip_edges()
	if path.is_empty() or not line_text.is_valid_int():
		return {}
	return {"path": _resolve_source_path(path), "line": int(line_text)}

func parse_stack_trace_line(line: String) -> Dictionary:
	var trimmed: String = line.strip_edges()
	if not trimmed.contains(":"):
		return {}

	var regex: RegEx = RegEx.new()
	if regex.compile("(?:^|\\s)([^\\s:]+\\.(?:cs|gd)):(\\d+)") != OK:
		return {}
	var match: RegExMatch = regex.search(trimmed)
	if match == null:
		return {}

	var raw_path: String = match.get_string(1).strip_edges()
	var line_text: String = match.get_string(2).strip_edges()
	if raw_path.is_empty() or not line_text.is_valid_int():
		return {}
	return {"path": _resolve_source_path(raw_path), "line": int(line_text)}

func open_source_in_vscode(path: String, line: int) -> void:
	if path.is_empty():
		return

	var absolute_path: String = ProjectSettings.globalize_path(path)
	var target: String = "%s:%d" % [absolute_path, maxi(1, line)]
	var process_id: int = OS.create_process("code", ["-g", target], false)
	if process_id != -1:
		return

	var uri_path: String = absolute_path.replace(" ", "%20")
	OS.shell_open("vscode://file/%s:%d" % [uri_path, maxi(1, line)])

func _resolve_source_path(path: String) -> String:
	if path.is_empty() or path.begins_with("res://"):
		return path
	if _source_path_cache.has(path):
		return str(_source_path_cache[path])

	if path.begins_with("/") and FileAccess.file_exists(path):
		var localized: String = ProjectSettings.localize_path(path)
		_source_path_cache[path] = localized
		return localized

	var file_name: String = path.get_file()
	var matches: PackedStringArray = []
	_collect_matching_files("res://", file_name, matches)
	if matches.is_empty():
		_source_path_cache[path] = path
		return path
	if matches.size() == 1:
		_source_path_cache[path] = matches[0]
		return matches[0]

	for candidate in matches:
		if candidate.ends_with(path):
			_source_path_cache[path] = candidate
			return candidate

	_source_path_cache[path] = matches[0]
	return matches[0]

func _collect_matching_files(directory_path: String, file_name: String, matches: PackedStringArray) -> void:
	var directory: DirAccess = DirAccess.open(directory_path)
	if directory == null:
		return

	directory.list_dir_begin()
	var entry_name: String = directory.get_next()
	while entry_name != "":
		if entry_name == "." or entry_name == "..":
			entry_name = directory.get_next()
			continue
		var full_path: String = directory_path.path_join(entry_name)
		if directory.current_is_dir():
			_collect_matching_files(full_path, file_name, matches)
		elif entry_name == file_name:
			matches.append(full_path)
		entry_name = directory.get_next()
	directory.list_dir_end()
