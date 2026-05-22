# Scans the Godot editor's control tree to find the Debugger panel and collect
# error rows from its Tree widgets.
extends RefCounted

# Recursively searches the editor base control for TabContainer tabs whose
# titles start with one of the requested prefixes.
func find_tab_controls_by_prefixes(root: Control, tab_title_prefixes: PackedStringArray) -> Array[Control]:
	var results: Array[Control] = []
	if root == null:
		return results

	var pending: Array[Node] = [root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is TabContainer:
			var tabs: TabContainer = node as TabContainer
			for index in range(tabs.get_tab_count()):
				var tab_title: String = tabs.get_tab_title(index)
				for prefix in tab_title_prefixes:
					if _tab_title_matches_prefix(tab_title, prefix):
						var tab_control: Control = tabs.get_tab_control(index)
						if tab_control != null:
							results.append(tab_control)
						break
	return results

func _tab_title_matches_prefix(tab_title: String, prefix: String) -> bool:
	if not tab_title.begins_with(prefix):
		return false
	# Exclude custom tabs like "Debugger+" when searching for built-in Debugger.
	if prefix == "Debugger" and tab_title.begins_with("Debugger+"):
		return false
	return true

# Returns controls for tabs we mirror in Debugger+.
func find_debugger_related_tab_controls(root: Control) -> Array[Control]:
	return find_tab_controls_by_prefixes(root, ["Debugger", "MSBuild"])

# Backward-compatible helper for existing callers that only need Debugger.
func find_debugger_tab_control(root: Control) -> Control:
	var controls: Array[Control] = find_tab_controls_by_prefixes(root, ["Debugger"])
	if controls.is_empty():
		return null
	return controls[0]

# Iterates the root-level items in a Tree and returns a formatted string for
# each item that looks like an error, using the provided formatter.
func collect_tree_error_rows(tree: Tree, formatter: DebuggerErrorFormatter, include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var rows: PackedStringArray = []
	var root_item: TreeItem = tree.get_root()
	if root_item == null:
		return rows
	var first_child: TreeItem = root_item.get_first_child()
	if first_child == null:
		return rows

	if _looks_like_msbuild_problems_tree(first_child):
		_collect_tree_item_rows_recursive(first_child, formatter, include_stack_trace, use_short_type_names, rows)
		return rows

	# Keep debugger parsing at top-level rows only to avoid collecting stack
	# frames and detail children as duplicate standalone entries.
	var item: TreeItem = first_child
	while item != null:
		var entry: String = formatter.format_item(item, include_stack_trace, use_short_type_names)
		if not entry.is_empty():
			rows.append(entry)
		item = item.get_next()
	return rows

func _looks_like_msbuild_problems_tree(first_item: TreeItem) -> bool:
	var pending: Array[TreeItem] = [first_item]
	var inspected: int = 0
	while not pending.is_empty() and inspected < 64:
		var item: TreeItem = pending.pop_back()
		inspected += 1
		if typeof(item.get_metadata(0)) == TYPE_INT:
			return true
		var tooltip: String = item.get_tooltip_text(0)
		if tooltip.contains("Type: warning") or tooltip.contains("Type: error"):
			return true
		var child: TreeItem = item.get_first_child()
		while child != null:
			pending.append(child)
			child = child.get_next()
	return false

func _collect_tree_item_rows_recursive(item: TreeItem, formatter: DebuggerErrorFormatter, include_stack_trace: bool, use_short_type_names: bool, rows: PackedStringArray) -> void:
	var current: TreeItem = item
	while current != null:
		var entry: String = formatter.format_item(current, include_stack_trace, use_short_type_names)
		if not entry.is_empty():
			rows.append(entry)
		var child: TreeItem = current.get_first_child()
		if child != null:
			_collect_tree_item_rows_recursive(child, formatter, include_stack_trace, use_short_type_names, rows)
		current = current.get_next()

# Removes duplicate and empty strings from the array, preserving insertion order.
func dedupe_non_empty(lines: PackedStringArray) -> PackedStringArray:
	var result: PackedStringArray = []
	var seen: Dictionary = {}
	for line in lines:
		var trimmed: String = line.strip_edges()
		if not trimmed.is_empty() and not seen.has(trimmed):
			seen[trimmed] = true
			result.append(trimmed)
	return result