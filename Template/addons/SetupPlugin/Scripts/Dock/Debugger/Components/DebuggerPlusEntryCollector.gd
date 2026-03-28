@tool
extends RefCounted

var _scanner
var _formatter
var _timestamp_service

func _init(scanner, formatter, timestamp_service) -> void:
	_scanner = scanner
	_formatter = formatter
	_timestamp_service = timestamp_service

func collect_entries(include_stack_trace: bool, use_short_type_names: bool, include_duplicates: bool, dev_mode: bool) -> PackedStringArray:
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return []

	var panel_roots: Array[Control] = _scanner.find_debugger_related_tab_controls(root)
	if panel_roots.is_empty():
		return []

	var rows: PackedStringArray = []
	for panel_root in panel_roots:
		if panel_root == null:
			continue
		var targeted_rows: PackedStringArray = _collect_errors_from_known_panel(panel_root, include_stack_trace, use_short_type_names)
		if not targeted_rows.is_empty():
			rows.append_array(targeted_rows)
			continue
		var fallback_rows: PackedStringArray = _collect_errors_with_recursive_fallback(panel_root, include_stack_trace, use_short_type_names)
		if dev_mode and not fallback_rows.is_empty():
			push_warning("Debugger+ fallback scan used for panel: %s (recovered %d entries)" % [str(panel_root.get_path()), fallback_rows.size()])
		rows.append_array(fallback_rows)

	if include_duplicates:
		return _non_empty_rows(rows)
	return _dedupe_non_empty_rows_ignoring_timestamps(rows)

func find_debugger_related_trees() -> Array[Tree]:
	var trees: Array[Tree] = []
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return trees

	var panel_roots: Array[Control] = _scanner.find_debugger_related_tab_controls(root)
	for panel_root in panel_roots:
		if panel_root == null:
			continue
		trees.append_array(_find_descendant_trees(panel_root))
	return trees

func _collect_errors_from_known_panel(panel_root: Control, include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var panel_rows: PackedStringArray = []
	var panel_path: String = str(panel_root.get_path())
	var trees: Array[Tree] = _find_descendant_trees(panel_root)

	if panel_path.ends_with("/Debugger"):
		for tree in trees:
			var tree_path: String = str(tree.get_path())
			if tree.columns == 2 and tree_path.contains("/Errors"):
				panel_rows.append_array(_scanner.collect_tree_error_rows(tree, _formatter, include_stack_trace, use_short_type_names))
		return panel_rows

	if panel_path.ends_with("/MSBuild"):
		for tree in trees:
			var tree_path: String = str(tree.get_path())
			if tree.columns == 1 and tree_path.contains("/Problems"):
				panel_rows.append_array(_scanner.collect_tree_error_rows(tree, _formatter, include_stack_trace, use_short_type_names))
		return panel_rows

	return panel_rows

func _collect_errors_with_recursive_fallback(panel_root: Control, include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var rows: PackedStringArray = []
	var pending: Array[Node] = [panel_root]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is Tree:
			rows.append_array(_scanner.collect_tree_error_rows(node as Tree, _formatter, include_stack_trace, use_short_type_names))
	return rows

func _find_descendant_trees(root_node: Node) -> Array[Tree]:
	var found: Array[Tree] = []
	var pending: Array[Node] = [root_node]
	while not pending.is_empty():
		var node: Node = pending.pop_back()
		for child in node.get_children():
			if child is Node:
				pending.append(child)
		if node is Tree:
			found.append(node as Tree)
	return found

func _non_empty_rows(rows: PackedStringArray) -> PackedStringArray:
	var filtered: PackedStringArray = []
	for row in rows:
		if not row.strip_edges().is_empty():
			filtered.append(row)
	return filtered

func _dedupe_non_empty_rows_ignoring_timestamps(rows: PackedStringArray) -> PackedStringArray:
	var deduped: PackedStringArray = []
	var seen_keys: Dictionary = {}
	for row in rows:
		var trimmed_row: String = row.strip_edges()
		if trimmed_row.is_empty():
			continue
		var normalized_key: String = _timestamp_service.strip_timestamp_from_entry(trimmed_row).strip_edges()
		if normalized_key.is_empty() or seen_keys.has(normalized_key):
			continue
		seen_keys[normalized_key] = true
		deduped.append(row)
	return deduped
