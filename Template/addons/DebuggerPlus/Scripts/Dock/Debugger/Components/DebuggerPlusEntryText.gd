@tool
extends RefCounted

func single_line_summary(entry: String) -> String:
	var lines: PackedStringArray = entry.split("\n", false)
	for line in lines:
		var trimmed: String = line.strip_edges()
		if not trimmed.is_empty():
			return trimmed
	var fallback: String = entry.strip_edges()
	return fallback if not fallback.is_empty() else "(Error entry)"

func entry_meta_type(entry: String) -> String:
	for line in entry.split("\n", false):
		var trimmed: String = line.strip_edges().to_lower()
		if trimmed.begins_with("__meta_type:"):
			return trimmed.trim_prefix("__meta_type:").strip_edges()
	return ""

func entry_is_warning(entry: String) -> bool:
	if entry_meta_type(entry) == "warning":
		return true
	return single_line_summary(entry).to_lower().contains("warning")

func entry_is_error(entry: String) -> bool:
	if entry_meta_type(entry) == "error":
		return true
	var summary: String = single_line_summary(entry).to_lower()
	return summary.contains("error") or summary.contains("exception")

func split_timestamp_prefix(line: String) -> Dictionary:
	var regex: RegEx = RegEx.new()
	if regex.compile("^(\\d+:\\d{2}:\\d{2}:\\d{3})\\s+(.*)$") != OK:
		return {"timestamp": "", "message": line}
	var match: RegExMatch = regex.search(line)
	if match == null:
		return {"timestamp": "", "message": line}
	return {"timestamp": match.get_string(1), "message": match.get_string(2)}

func extract_detail_lines(entry: String) -> PackedStringArray:
	var summary: String = single_line_summary(entry)
	var details: PackedStringArray = []
	for line in entry.split("\n", false):
		var trimmed: String = line.strip_edges()
		if trimmed.is_empty() or trimmed.begins_with("__meta_type:"):
			continue
		if trimmed == summary and details.is_empty():
			continue
		details.append(trimmed)
	return details

func apply_prefix_to_message(message: String, entry: String, warning_case: int, error_case: int) -> String:
	var trimmed: String = message.strip_edges()
	var lowered: String = trimmed.to_lower()
	if lowered.begins_with("warning:") or lowered.begins_with("error:"):
		return message

	var meta_type: String = entry_meta_type(entry)
	if meta_type == "warning":
		return _with_prefix(_prefix_text("warning", warning_case), message)
	if meta_type == "error":
		return _with_prefix(_prefix_text("error", error_case), message)
	if lowered.contains("warning"):
		return _with_prefix(_prefix_text("warning", warning_case), message)
	if lowered.contains("exception") or lowered.contains("error"):
		return _with_prefix(_prefix_text("error", error_case), message)
	return message

func entry_color_for_message(message: String, entry: String, settings) -> Color:
	if not settings.colors_enabled:
		return settings.DEFAULT_COLOR_ENTRY_DEFAULT
	var meta_type: String = entry_meta_type(entry)
	if meta_type == "warning":
		return settings.color_entry_warning
	if meta_type == "error":
		return settings.color_entry_error

	var lowered: String = message.to_lower()
	if lowered.contains("warning"):
		return settings.color_entry_warning
	if lowered.contains("nullreferenceexception") or lowered.contains("argumentnullexception"):
		return settings.color_entry_error
	if lowered.contains("exception") or lowered.contains("error"):
		return settings.color_entry_error
	return settings.color_entry_default

func detail_color_for_line(detail_line: String, settings) -> Color:
	if not settings.colors_enabled:
		return settings.DEFAULT_COLOR_DETAIL_DEFAULT
	var lowered: String = detail_line.to_lower()
	if lowered.begins_with("stack trace"):
		return settings.color_stack_header
	return settings.color_detail_default

func _with_prefix(prefix: String, message: String) -> String:
	if prefix.is_empty():
		return message
	return "%s: %s" % [prefix, message]

func _prefix_text(base: String, mode: int) -> String:
	if mode == 0:
		return ""
	if mode == 1:
		return "%s%s" % [base.substr(0, 1).to_upper(), base.substr(1, base.length() - 1).to_lower()]
	if mode == 2:
		return base.to_upper()
	return base
