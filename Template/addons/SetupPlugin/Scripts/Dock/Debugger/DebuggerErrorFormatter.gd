# Formats a single TreeItem from Godot's Debugger panel into a human-readable
# error string.  Optionally includes the source file location and a cleaned-up
# stack trace.  Fully-qualified type names can be stripped to their short form.
extends RefCounted

# Formats one debugger TreeItem as a multi-line string.
# Returns an empty string if the item does not look like an error entry.
func format_item(item: TreeItem, include_stack_trace: bool, use_short_type_names: bool) -> String:
	var title: String = item.get_text(1).strip_edges()
	if use_short_type_names:
		title = _strip_fully_qualified_names(title)
	if not _looks_like_error_line(title):
		return ""

	var lines: PackedStringArray = [title]
	var source: String = _format_tree_item_source(item)
	if not source.is_empty():
		lines.append("Source: %s" % source)
	if include_stack_trace:
		var stack: PackedStringArray = _collect_stack_trace(item, use_short_type_names)
		if not stack.is_empty():
			lines.append("Stack Trace:")
			for frame in stack:
				lines.append("    %s" % frame)
	return "\n".join(lines)

# Extracts the source file path and line number from the item's metadata.
# Godot's debugger stores this as a two-element array: [file, line].
func _format_tree_item_source(item: TreeItem) -> String:
	var meta: Variant = item.get_metadata(0)
	if meta is Array:
		var parts: Array = meta
		if parts.size() >= 2 and not str(parts[0]).is_empty():
			return "%s:%s" % [str(parts[0]), str(parts[1])]
	return ""

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
			if not details.is_empty() and not _should_stop_stack_frame(details):
				result.append(_normalize_stack_frame_signature(details) if use_short_type_names else details)
			child = child.get_next()
			continue
		if collecting and not details.is_empty():
			if _should_stop_stack_frame(details):
				break
			result.append(_normalize_stack_frame_signature(details) if use_short_type_names else details)
		child = child.get_next()
	return result

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
	return lower.contains("error") or lower.contains("exception") or lower.contains("failed")

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