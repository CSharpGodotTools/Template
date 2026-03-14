@tool
# Expands and collapses nodes in the Scene dock tree.
# Prefers operating directly on the Tree widget for immediate visual feedback.
# Falls back to the live scene node graph when no scene is open or the Tree
# widget cannot be located.
extends RefCounted

# Expands the scene hierarchy so all nodes up to (but not past) `level` depth
# are visible.  Returns the number of tree items touched.
func expand_to_level(level: int) -> int:
	var normalized_level: int = maxi(level, 0)
	var scene_tree: Tree = _find_scene_dock_tree()
	if scene_tree != null:
		return _apply_level_to_tree(scene_tree, normalized_level)
	return _apply_level_to_nodes(normalized_level)

# Collapses the scene hierarchy so only nodes at or above `level` remain
# expanded.  Returns the number of tree items touched.
func collapse_to_level(level: int) -> int:
	var normalized_level: int = maxi(level, 0)
	var scene_tree: Tree = _find_scene_dock_tree()
	if scene_tree != null:
		return _apply_level_to_tree(scene_tree, normalized_level)
	return _apply_level_to_nodes(normalized_level)

# Expands every node in the hierarchy.
# Returns the number of items touched, or 0 if no scene is open.
func fully_expand() -> int:
	var scene_tree: Tree = _find_scene_dock_tree()
	if scene_tree != null:
		var root_item: TreeItem = scene_tree.get_root()
		if root_item == null:
			return 0
		var touched: int = 0
		var pending: Array = [root_item]
		while not pending.is_empty():
			var item: TreeItem = pending.pop_back()
			item.collapsed = false
			touched += 1
			var child: TreeItem = item.get_first_child()
			while child != null:
				pending.append(child)
				child = child.get_next()
		scene_tree.queue_redraw()
		return touched
	return _apply_level_to_nodes(9999)

# Collapses everything to just the root level (equivalent to level = 1).
func fully_collapse() -> int:
	return collapse_to_level(1)

# Locates the Tree widget inside the Scene dock that shows the current scene's
# node tree.  Identifies the correct Tree by checking whether its root item
# name matches the edited scene root.  Returns null if no scene is open.
func _find_scene_dock_tree() -> Tree:
	var edited_root: Node = EditorInterface.get_edited_scene_root()
	if edited_root == null:
		return null
	var base_control: Control = EditorInterface.get_base_control()
	if base_control == null:
		return null
	var trees: Array[Node] = base_control.find_children("*", "Tree", true, false)
	for candidate in trees:
		if candidate is Tree:
			var tree: Tree = candidate as Tree
			var root_item: TreeItem = tree.get_root()
			if root_item != null and root_item.get_text(0).strip_edges() == edited_root.name:
				return tree
	return null

# Iterates every TreeItem and sets collapsed = (depth >= level), then redraws.
func _apply_level_to_tree(scene_tree: Tree, level: int) -> int:
	var root_item: TreeItem = scene_tree.get_root()
	if root_item == null:
		return 0
	var touched: int = 0
	var pending: Array = [[root_item, 0]]
	while not pending.is_empty():
		var pair: Array = pending.pop_back()
		var item: TreeItem = pair[0]
		var depth: int = pair[1]
		item.collapsed = depth >= level
		touched += 1
		var child: TreeItem = item.get_first_child()
		while child != null:
			pending.append([child, depth + 1])
			child = child.get_next()
	scene_tree.queue_redraw()
	return touched

# Fallback path used when _find_scene_dock_tree returns null.
# Applies expand/collapse by walking the live scene node graph.
func _apply_level_to_nodes(level: int) -> int:
	var root: Node = EditorInterface.get_edited_scene_root()
	if root == null:
		return 0
	var touched: int = 0
	var pending: Array = [[root, 0]]
	while not pending.is_empty():
		var pair: Array = pending.pop_back()
		var node: Node = pair[0]
		var depth: int = pair[1]
		if node.has_method("set_display_folded"):
			node.set_display_folded(depth >= level)
			touched += 1
		for child in node.get_children():
			if child is Node:
				pending.append([child, depth + 1])
	return touched