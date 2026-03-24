# Formats a single TreeItem from Godot's Debugger panel into a human-readable
# error string.  Optionally includes the source file location and a cleaned-up
# stack trace.  Fully-qualified type names can be stripped to their short form.
extends RefCounted

# Formats one debugger TreeItem as a multi-line string.
# Returns an empty string if the item does not look like an error entry.
func format_item(item: TreeItem, include_stack_trace: bool, use_short_type_names: bool) -> String:
	var is_msbuild_problem: bool = _is_msbuild_problem_item(item)
	var msbuild_details: Dictionary = _extract_msbuild_details(item) if is_msbuild_problem else {}
	var title: String = _extract_item_title(item)
	if use_short_type_names:
		title = _strip_fully_qualified_names(title)
	if is_msbuild_problem:
		title = _normalize_msbuild_title(title, msbuild_details)
	if _is_non_problem_group_row(item, title):
		return ""
	if not is_msbuild_problem and not _looks_like_error_line(title):
		return ""
	var row_timestamp: String = _extract_row_timestamp(item)
	if not row_timestamp.is_empty():
		title = "%s %s" % [row_timestamp, title]

	var lines: PackedStringArray = [title]
	var source: String = _format_tree_item_source(item)
	if source.is_empty() and is_msbuild_problem:
		source = _source_from_msbuild_details(msbuild_details)
	if source.is_empty():
		source = _extract_source_from_text_or_context(title, item)
	if source.is_empty() and _contains_compiler_code(title.to_lower()):
		return ""
	if is_msbuild_problem and source.is_empty():
		return ""
	if _is_helper_source(source) or _is_non_actionable_source(source):
		var stack_source: String = _find_first_user_source_from_stack(item)
		if not stack_source.is_empty():
			source = stack_source
	if not source.is_empty():
		lines.append("Source: %s" % source)
	if include_stack_trace:
		var stack: PackedStringArray = _collect_stack_trace(item, use_short_type_names)
		if not stack.is_empty():
			lines.append("Stack Trace:")
			for frame in stack:
				lines.append("    %s" % frame)
	if is_msbuild_problem:
		var diagnostic_type: String = str(msbuild_details.get("type", "")).strip_edges().to_lower()
		if diagnostic_type == "warning" or diagnostic_type == "error":
			lines.append("__meta_type:%s" % diagnostic_type)
	return "\n".join(lines)

func _is_msbuild_problem_item(item: TreeItem) -> bool:
	return typeof(item.get_metadata(0)) == TYPE_INT

func _extract_msbuild_details(item: TreeItem) -> Dictionary:
	var tooltip: String = item.get_tooltip_text(0).strip_edges()
	if tooltip.is_empty():
		var tree: Tree = item.get_tree()
		if tree != null and tree.columns > 1:
			tooltip = item.get_tooltip_text(1).strip_edges()
	if tooltip.is_empty():
		return {}

	var details: Dictionary = {}
	for raw_line in tooltip.split("\n", false):
		var line: String = raw_line.strip_edges()
		if line.begins_with("Code:"):
			details["code"] = line.trim_prefix("Code:").strip_edges()
		elif line.begins_with("Type:"):
			details["type"] = line.trim_prefix("Type:").strip_edges().to_lower()
		elif line.begins_with("File:"):
			details["file"] = line.trim_prefix("File:").strip_edges()
		elif line.begins_with("Line:"):
			var line_text: String = line.trim_prefix("Line:").strip_edges()
			if line_text.is_valid_int():
				details["line"] = int(line_text)
	return details

func _normalize_msbuild_title(title: String, details: Dictionary) -> String:
	var result: String = title.strip_edges()
	var code: String = str(details.get("code", "")).strip_edges()
	if not code.is_empty() and not result.to_lower().begins_with(code.to_lower() + ":"):
		result = "%s: %s" % [code, result]
	return result

func _source_from_msbuild_details(details: Dictionary) -> String:
	var file_path: String = str(details.get("file", "")).strip_edges()
	if file_path.is_empty():
		return ""
	var line_number: int = int(details.get("line", 1))
	return "%s:%d" % [file_path, maxi(1, line_number)]

func _extract_item_title(item: TreeItem) -> String:
	var tree: Tree = item.get_tree()
	if tree != null and tree.columns > 1:
		var secondary: String = item.get_text(1).strip_edges()
		if not secondary.is_empty():
			return secondary
	return item.get_text(0).strip_edges()

func _is_non_problem_group_row(item: TreeItem, title: String) -> bool:
	var meta: Variant = item.get_metadata(0)
	if typeof(meta) == TYPE_INT:
		return false
	if meta is Array:
		# Debugger entries store source metadata arrays.
		return false
	if title.is_empty():
		return true

	var lower: String = title.to_lower()
	if lower.ends_with(" issues)"):
		return true
	if lower.ends_with(".csproj"):
		return true
	if not lower.contains("error") and not lower.contains("warning") and not _contains_compiler_code(lower):
		return true
	return false

# Extracts the source file path and line number from the item's metadata.
# Godot's debugger stores this as a two-element array: [file, line].
func _format_tree_item_source(item: TreeItem) -> String:
	var meta: Variant = item.get_metadata(0)
	if meta is Array:
		var parts: Array = meta
		if parts.size() >= 2 and not str(parts[0]).is_empty():
			return "%s:%s" % [str(parts[0]), str(parts[1])]
	return ""

func _extract_row_timestamp(item: TreeItem) -> String:
	var text_timestamp: String = _normalize_elapsed_timestamp_from_text(item.get_text(0).strip_edges())
	if not text_timestamp.is_empty():
		return text_timestamp

	var meta: Variant = item.get_metadata(0)
	if meta is Array:
		var parts: Array = meta
		if parts.size() >= 3:
			var maybe_elapsed = parts[2]
			if typeof(maybe_elapsed) == TYPE_INT:
				return _format_elapsed_from_msec(int(maybe_elapsed))
			if typeof(maybe_elapsed) == TYPE_FLOAT:
				return _format_elapsed_from_msec(int(float(maybe_elapsed) * 1000.0))
	return ""

func _normalize_elapsed_timestamp_from_text(text: String) -> String:
	if text.is_empty():
		return ""
	var regex: RegEx = RegEx.new()
	if regex.compile("(\\d+):(\\d{2}):(\\d{2})[\\.:](\\d{1,3})") != OK:
		return ""
	var match: RegExMatch = regex.search(text)
	if match == null:
		return ""
	var hours: int = int(match.get_string(1))
	var minutes: int = int(match.get_string(2))
	var seconds: int = int(match.get_string(3))
	var millis_text: String = match.get_string(4)
	var millis: int = int(millis_text)
	if millis_text.length() == 1:
		millis *= 100
	elif millis_text.length() == 2:
		millis *= 10
	return "%d:%02d:%02d:%03d" % [hours, minutes, seconds, millis]

func _format_elapsed_from_msec(elapsed_msec: int) -> String:
	var safe_elapsed: int = maxi(0, elapsed_msec)
	var total_seconds: int = safe_elapsed / 1000
	var hours: int = total_seconds / 3600
	var minutes: int = (total_seconds % 3600) / 60
	var seconds: int = total_seconds % 60
	var millis: int = safe_elapsed % 1000
	return "%d:%02d:%02d:%03d" % [hours, minutes, seconds, millis]

func _is_helper_source(source: String) -> bool:
	if source.is_empty():
		return false
	var lower: String = source.to_lower()
	return lower.contains("debug.cs:") or lower.ends_with("debug.cs")

func _is_non_actionable_source(source: String) -> bool:
	if source.is_empty():
		return true
	var trimmed: String = source.strip_edges()
	if trimmed == "res://" or trimmed == "res://:0":
		return true

	var separator: int = trimmed.rfind(":")
	if separator <= 0:
		return false
	var path: String = trimmed.substr(0, separator).strip_edges()
	var line_text: String = trimmed.substr(separator + 1).strip_edges()
	if path.is_empty() or path == "res://":
		return true
	if line_text.is_valid_int() and int(line_text) <= 0:
		return true
	return false

func _find_first_user_source_from_stack(item: TreeItem) -> String:
	var child: TreeItem = item.get_first_child()
	var collecting: bool = false
	while child != null:
		var label: String = child.get_text(0)
		var details: String = child.get_text(1).strip_edges()
		if label.contains("Stack Trace"):
			collecting = true
			child = child.get_next()
			continue
		if collecting and not details.is_empty():
			if _should_stop_stack_frame(details):
				break
			var source: String = _extract_source_from_stack_frame(details)
			if not source.is_empty() and not _is_helper_source(source):
				return source
		child = child.get_next()
	return ""

func _extract_source_from_stack_frame(frame: String) -> String:
	var at_split: PackedStringArray = frame.split(" @ ", false, 1)
	if at_split.size() == 2:
		var left: String = at_split[0].strip_edges()
		var right: String = at_split[1].strip_edges()
		if _looks_like_source_segment(left):
			return left
		if _looks_like_source_segment(right):
			return right

	var line_matcher: RegEx = RegEx.new()
	if line_matcher.compile("(?:in\\s+)?(.+\\.(?:cs|gd))(?::line\\s*|:)(\\d+)") == OK:
		var match: RegExMatch = line_matcher.search(frame)
		if match != null:
			return "%s:%s" % [match.get_string(1).strip_edges(), match.get_string(2).strip_edges()]

	return ""

func _looks_like_source_segment(text: String) -> bool:
	var lower: String = text.to_lower()
	return lower.contains(".cs:") or lower.contains(".gd:") or lower.contains(":line")

# Walks the TreeItem's children to collect every stack frame that appears
# after the row labelled "Stack Trace", stopping at internal .NET frames.
func _collect_stack_trace(item: TreeItem, use_short_type_names: bool) -> PackedStringArray:
	var result: PackedStringArray = []
	var child: TreeItem = item.get_first_child()
	var collecting: bool = false
	while child != null:
		var label: String = child.get_text(0)
		var details: String = child.get_text(1).strip_edges()
		if label.contains("Stack Trace"):
			collecting = true
			if not details.is_empty() and not _should_stop_stack_frame(details) and not _is_non_actionable_stack_frame(details):
				result.append(_normalize_stack_frame_signature(details) if use_short_type_names else details)
			child = child.get_next()
			continue
		if collecting and not details.is_empty():
			if _should_stop_stack_frame(details):
				break
			if _is_non_actionable_stack_frame(details):
				child = child.get_next()
				continue
			result.append(_normalize_stack_frame_signature(details) if use_short_type_names else details)
		child = child.get_next()
	return result

func _is_non_actionable_stack_frame(frame: String) -> bool:
	var trimmed: String = frame.strip_edges()
	if trimmed.is_empty():
		return true
	if trimmed == ":0" or trimmed.begins_with(":0 @"):
		return true

	var extracted_source: String = _extract_source_from_stack_frame(trimmed)
	if not extracted_source.is_empty() and _is_non_actionable_source(extracted_source):
		return true
	return false

# Returns true for internal .NET/GDNative frames that offer no value to the
# developer and should be excluded from the output.
func _should_stop_stack_frame(frame: String) -> bool:
	var lower: String = frame.to_lower()
	return lower.contains("nativeinterop.nativevariantptrargs") \
		or lower.contains("godot.nativeinterop") \
		or lower.contains("godot.bridge.csharpinstancebridge")

# Heuristic check: returns true when the text appears to be an error line
# (contains "error", "exception", or "failed", case-insensitive).
func _looks_like_error_line(text: String) -> bool:
	if text.is_empty():
		return false
	var lower: String = text.to_lower()
	if lower.contains("error") or lower.contains("exception") or lower.contains("failed") or lower.contains("warning"):
		return true
	if _contains_compiler_code(lower):
		return true
	return false

func _contains_compiler_code(text: String) -> bool:
	var regex: RegEx = RegEx.new()
	if regex.compile("\\b(?:cs|nu|msb|ca)\\d{3,5}\\b") != OK:
		return false
	return regex.search(text) != null

func _extract_source_from_text_or_context(text: String, item: TreeItem) -> String:
	var from_text: String = _extract_source_from_msbuild_text(text)
	if not from_text.is_empty():
		return from_text

	# In MSBuild tree layout, file path may be on any ancestor row.
	var parent_path: String = _find_path_in_ancestors(item)
	if parent_path.is_empty():
		return ""
	var line_number: int = _extract_line_number_from_text(text)
	if line_number <= 0:
		line_number = 1
	return "%s:%d" % [parent_path, line_number]

func _find_path_in_ancestors(item: TreeItem) -> String:
	var current: TreeItem = item.get_parent()
	while current != null:
		var from_col0: String = _extract_file_path_from_group_row(current.get_text(0).strip_edges())
		if not from_col0.is_empty():
			return from_col0
		var tree: Tree = current.get_tree()
		if tree != null and tree.columns > 1:
			var from_col1: String = _extract_file_path_from_group_row(current.get_text(1).strip_edges())
			if not from_col1.is_empty():
				return from_col1
		current = current.get_parent()
	return ""

func _extract_source_from_msbuild_text(text: String) -> String:
	var regex: RegEx = RegEx.new()
	if regex.compile("([^\\s]+\\.(?:cs|gd))\\((\\d+)(?:,\\d+)?\\)") != OK:
		return ""
	var match: RegExMatch = regex.search(text)
	if match == null:
		return ""
	var path: String = match.get_string(1).strip_edges()
	var line_text: String = match.get_string(2).strip_edges()
	if path.is_empty() or not line_text.is_valid_int():
		return ""
	return "%s:%d" % [path, int(line_text)]

func _extract_file_path_from_group_row(text: String) -> String:
	if text.is_empty():
		return ""
	var regex: RegEx = RegEx.new()
	if regex.compile("(.+\\.(?:cs|gd))(?:\\s+\\(\\d+\\s+issues\\))?$") != OK:
		return ""
	var match: RegExMatch = regex.search(text)
	if match == null:
		return ""
	return match.get_string(1).strip_edges()

func _extract_line_number_from_text(text: String) -> int:
	var regex: RegEx = RegEx.new()
	if regex.compile("\\((\\d+)(?:,\\d+)?\\)") != OK:
		return -1
	var match: RegExMatch = regex.search(text)
	if match == null:
		return -1
	var line_text: String = match.get_string(1)
	if not line_text.is_valid_int():
		return -1
	return int(line_text)

# If the frame contains " @ ", strips fully-qualified type names from the
# method-signature portion while leaving the file/line portion intact.
func _normalize_stack_frame_signature(frame: String) -> String:
	if frame.contains(" @ "):
		var parts: PackedStringArray = frame.split(" @ ", false, 1)
		if parts.size() == 2:
			return "%s @ %s" % [parts[0], _strip_fully_qualified_names(parts[1])]
	return _strip_fully_qualified_names(frame)

# Uses a regex to replace "Namespace.ClassName" style prefixes with just
# "ClassName", making stack traces shorter and easier to read.
func _strip_fully_qualified_names(text: String) -> String:
	var regex: RegEx = RegEx.new()
	if regex.compile("\\b(?:[A-Za-z_][A-Za-z0-9_]*\\.)+([A-Za-z_][A-Za-z0-9_]*)") != OK:
		return text
	return regex.sub(text, "$1", true)
