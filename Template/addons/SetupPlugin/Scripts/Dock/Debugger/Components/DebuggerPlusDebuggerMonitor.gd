@tool
extends RefCounted

signal debugger_tree_changed

var _scanner
var _editor_root: Control
var _watched_debugger_trees: Array[Tree] = []
var _debugger_tree_signatures: Dictionary = {}

func _init(scanner) -> void:
	_scanner = scanner

func bind() -> void:
	if _editor_root == null:
		_editor_root = EditorInterface.get_base_control()
	if _editor_root == null:
		return
	var callback: Callable = Callable(self, "_on_editor_tree_structure_changed")
	if not _editor_root.is_connected("child_entered_tree", callback):
		_editor_root.child_entered_tree.connect(_on_editor_tree_structure_changed)
	refresh_watchers()

func refresh_watchers() -> void:
	var root: Control = EditorInterface.get_base_control()
	if root == null:
		return
	var panel_roots: Array[Control] = _scanner.find_debugger_related_tab_controls(root)
	if panel_roots.is_empty():
		_unwatch_debugger_trees()
		return

	var seen_tree_ids: Dictionary = {}
	for panel_root in panel_roots:
		if panel_root == null:
			continue
		var pending: Array[Node] = [panel_root]
		while not pending.is_empty():
			var node: Node = pending.pop_back()
			for child in node.get_children():
				if child is Node:
					pending.append(child)
			if node is Tree:
				var tree: Tree = node as Tree
				seen_tree_ids[tree.get_instance_id()] = true
				_watch_debugger_tree(tree)

	_prune_watched_debugger_trees(seen_tree_ids)

func unbind() -> void:
	if _editor_root != null:
		var callback: Callable = Callable(self, "_on_editor_tree_structure_changed")
		if _editor_root.is_connected("child_entered_tree", callback):
			_editor_root.disconnect("child_entered_tree", callback)
	_editor_root = null
	_unwatch_debugger_trees()

func _watch_debugger_tree(tree: Tree) -> void:
	for existing in _watched_debugger_trees:
		if existing == tree:
			return
	_watched_debugger_trees.append(tree)
	_connect_tree_signal(tree, "item_selected", Callable(self, "_on_debugger_tree_changed"))
	_connect_tree_signal(tree, "item_activated", Callable(self, "_on_debugger_tree_changed"))
	_connect_tree_signal(tree, "nothing_selected", Callable(self, "_on_debugger_tree_changed"))
	_connect_tree_signal(tree, "minimum_size_changed", Callable(self, "_on_debugger_tree_changed"))
	_connect_tree_signal(tree, "draw", Callable(self, "_on_debugger_tree_draw"))
	_debugger_tree_signatures[tree.get_instance_id()] = _compute_tree_signature(tree)

func _prune_watched_debugger_trees(keep_tree_ids: Dictionary) -> void:
	var retained: Array[Tree] = []
	for tree in _watched_debugger_trees:
		if tree == null or not is_instance_valid(tree):
			continue
		var tree_id: int = tree.get_instance_id()
		if keep_tree_ids.has(tree_id):
			retained.append(tree)
			continue
		_disconnect_tree_signals(tree)
		_debugger_tree_signatures.erase(tree_id)
	_watched_debugger_trees = retained

func _unwatch_debugger_trees() -> void:
	for tree in _watched_debugger_trees:
		if tree == null or not is_instance_valid(tree):
			continue
		_disconnect_tree_signals(tree)
	_watched_debugger_trees.clear()
	_debugger_tree_signatures.clear()

func _connect_tree_signal(tree: Tree, signal_name: StringName, callback: Callable) -> void:
	if tree.is_connected(signal_name, callback):
		return
	tree.connect(signal_name, callback)

func _disconnect_tree_signals(tree: Tree) -> void:
	_disconnect_tree_signal(tree, "item_selected", Callable(self, "_on_debugger_tree_changed"))
	_disconnect_tree_signal(tree, "item_activated", Callable(self, "_on_debugger_tree_changed"))
	_disconnect_tree_signal(tree, "nothing_selected", Callable(self, "_on_debugger_tree_changed"))
	_disconnect_tree_signal(tree, "minimum_size_changed", Callable(self, "_on_debugger_tree_changed"))
	_disconnect_tree_signal(tree, "draw", Callable(self, "_on_debugger_tree_draw"))

func _disconnect_tree_signal(tree: Tree, signal_name: StringName, callback: Callable) -> void:
	if tree.is_connected(signal_name, callback):
		tree.disconnect(signal_name, callback)

func _on_editor_tree_structure_changed(_node: Node = null) -> void:
	refresh_watchers()
	debugger_tree_changed.emit()

func _on_debugger_tree_changed() -> void:
	_refresh_if_debugger_tree_changed()

func _on_debugger_tree_draw() -> void:
	_refresh_if_debugger_tree_changed()

func _refresh_if_debugger_tree_changed() -> void:
	var changed: bool = false
	for tree in _watched_debugger_trees:
		if tree == null or not is_instance_valid(tree):
			continue
		var key: int = tree.get_instance_id()
		var latest: int = _compute_tree_signature(tree)
		if int(_debugger_tree_signatures.get(key, -1)) != latest:
			_debugger_tree_signatures[key] = latest
			changed = true
	if changed:
		debugger_tree_changed.emit()

func _compute_tree_signature(tree: Tree) -> int:
	if tree == null:
		return 0
	var root: TreeItem = tree.get_root()
	if root == null:
		return 0
	return _accumulate_tree_signature(root.get_first_child(), 17, 0, tree.columns)

func _accumulate_tree_signature(item: TreeItem, signature: int, depth: int, column_count: int) -> int:
	var current: TreeItem = item
	var result: int = signature
	var index: int = 0
	while current != null:
		var text_a: String = current.get_text(0)
		var text_b: String = current.get_text(1) if column_count > 1 else ""
		result = result * 31 + (text_a.hash() ^ (text_b.hash() * 7) ^ (depth * 17) ^ (index * 13))
		var child: TreeItem = current.get_first_child()
		if child != null:
			result = _accumulate_tree_signature(child, result, depth + 1, column_count)
		index += 1
		current = current.get_next()
	return result
