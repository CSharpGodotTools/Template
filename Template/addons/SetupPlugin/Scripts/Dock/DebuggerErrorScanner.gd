class_name DebuggerErrorScanner
extends RefCounted

func find_debugger_tab_control(root: Control) -> Control:
	var pending: Array[Node] = [root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is TabContainer:
			var tabs: TabContainer = node as TabContainer
			for index in range(tabs.get_tab_count()):
				if tabs.get_tab_title(index).begins_with("Debugger"):
					return tabs.get_tab_control(index)
	return null

func collect_tree_error_rows(tree: Tree, formatter: DebuggerErrorFormatter, include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var rows: PackedStringArray = []
	var root_item: TreeItem = tree.get_root()
	if root_item == null:
		return rows
	var item: TreeItem = root_item.get_first_child()
	while item != null:
		var entry: String = formatter.format_item(item, include_stack_trace, use_short_type_names)
		if not entry.is_empty():
			rows.append(entry)
		item = item.get_next()
	return rows

func dedupe_non_empty(lines: PackedStringArray) -> PackedStringArray:
	var result: PackedStringArray = []
	var seen: Dictionary = {}
	for line in lines:
		var trimmed: String = line.strip_edges()
		if not trimmed.is_empty() and not seen.has(trimmed):
			seen[trimmed] = true
			result.append(trimmed)
	return result