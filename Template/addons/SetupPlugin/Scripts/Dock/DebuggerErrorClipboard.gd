@tool
class_name DebuggerErrorClipboard
extends RefCounted

const DebuggerErrorFormatter = preload("res://addons/SetupPlugin/Scripts/Dock/DebuggerErrorFormatter.gd")
const DebuggerErrorScanner = preload("res://addons/SetupPlugin/Scripts/Dock/DebuggerErrorScanner.gd")

var _formatter: DebuggerErrorFormatter = DebuggerErrorFormatter.new()
var _scanner: DebuggerErrorScanner = DebuggerErrorScanner.new()

func collect_errors(include_stack_trace: bool, use_short_type_names: bool) -> PackedStringArray:
	var errors: PackedStringArray = []
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return errors

	var debugger_root: Control = _scanner.find_debugger_tab_control(root)
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
			errors.append_array(_scanner.collect_tree_error_rows(tree, _formatter, include_stack_trace, use_short_type_names))

	return _scanner.dedupe_non_empty(errors)
