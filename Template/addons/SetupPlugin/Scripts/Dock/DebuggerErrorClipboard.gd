@tool
class_name DebuggerErrorClipboard
extends RefCounted

func collect_errors(include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var errors: PackedStringArray = []
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return errors

	var debugger_root: Control = _find_debugger_tab_control(root)
	if debugger_root == null:
		return errors

	var pending: Array[Node] = [debugger_root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)

		if node is Tree:
			var tree: Tree = node as Tree
			errors.append_array(_collect_tree_error_rows(tree, include_stack_trace, use_short_type_names))

	return _dedupe_non_empty(errors)

func _collect_tree_error_rows(tree: Tree, include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var out: PackedStringArray = []
	var root_item: TreeItem = tree.get_root()
	if root_item == null:
		return out

	var item: TreeItem = root_item.get_first_child()
	while item != null:
		var entry: String = _format_debugger_error_item(item, include_stack_trace, use_short_type_names)
		if not entry.is_empty():
			out.append(entry)
		item = item.get_next()

	return out

func _format_debugger_error_item(item: TreeItem, include_stack_trace: bool, use_short_type_names: bool) -> String:
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

func _format_tree_item_source(item: TreeItem) -> String:
	var meta: Variant = item.get_metadata(0)
	if meta is Array:
		var arr: Array = meta
		if arr.size() >= 2:
			var file: String = str(arr[0])
			var line: String = str(arr[1])
			if not file.is_empty():
				return "%s:%s" % [file, line]
	return ""

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
				if use_short_type_names:
					details = _normalize_stack_frame_signature(details)
				result.append(details)
			child = child.get_next()
			continue

		if collecting and not details.is_empty():
			if _should_stop_stack_frame(details):
				break
			if use_short_type_names:
				details = _normalize_stack_frame_signature(details)
			result.append(details)

		child = child.get_next()

	return result

func _should_stop_stack_frame(frame: String) -> bool:
	var lower: String = frame.to_lower()
	if lower.contains("nativeinterop.nativevariantptrargs"):
		return true
	if lower.contains("godot.nativeinterop"):
		return true
	if lower.contains("godot.bridge.csharpinstancebridge"):
		return true
	return false

func _find_debugger_tab_control(root: Control) -> Control:
	var pending: Array[Node] = [root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)

		if node is TabContainer:
			var tabs: TabContainer = node as TabContainer
			for i in range(tabs.get_tab_count()):
				var title: String = tabs.get_tab_title(i)
				if title.begins_with("Debugger"):
					return tabs.get_tab_control(i)

	return null

func _dedupe_non_empty(lines: PackedStringArray) -> PackedStringArray:
	var result: PackedStringArray = []
	var seen: Dictionary = {}
	for line in lines:
		var trimmed: String = line.strip_edges()
		if trimmed.is_empty():
			continue
		if seen.has(trimmed):
			continue
		seen[trimmed] = true
		result.append(trimmed)
	return result

func _looks_like_error_line(text: String) -> bool:
	if text.is_empty():
		return false
	var t: String = text.to_lower()
	return t.contains("error") or t.contains("exception") or t.contains("failed")

func _normalize_stack_frame_signature(frame: String) -> String:
	if frame.contains(" @ "):
		var parts: PackedStringArray = frame.split(" @ ", false, 1)
		if parts.size() == 2:
			return "%s @ %s" % [parts[0], _strip_fully_qualified_names(parts[1])]
	return _strip_fully_qualified_names(frame)

func _strip_fully_qualified_names(text: String) -> String:
	var regex: RegEx = RegEx.new()
	var compiled: Error = regex.compile("\\b(?:[A-Za-z_][A-Za-z0-9_]*\\.)+([A-Za-z_][A-Za-z0-9_]*)")
	if compiled != OK:
		return text
	return regex.sub(text, "$1", true)
