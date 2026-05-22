@tool
extends RefCounted

const _ENTRY_TOGGLE_GUTTER_X: float = 22.0
const _CONTEXT_MENU_Y_OFFSET: int = 12
const _CONTEXT_COPY_ID: int = 0

var _view
var _tree_renderer
var _source_navigator
var _entry_filter
var _context_entry_to_copy: String = ""

func _init(view, tree_renderer, source_navigator, entry_filter) -> void:
	_view = view
	_tree_renderer = tree_renderer
	_source_navigator = source_navigator
	_entry_filter = entry_filter

func on_tree_item_activated() -> void:
	var selected: TreeItem = _view.get_error_tree().get_selected()
	var target: Dictionary = _tree_renderer.source_target_from_tree_item(selected)
	if target.is_empty():
		return
	_source_navigator.open_source_in_vscode(str(target.get("path", "")), int(target.get("line", 1)))

func on_tree_gui_input(event: InputEvent) -> void:
	if not (event is InputEventMouseButton):
		return
	var mouse_event: InputEventMouseButton = event as InputEventMouseButton
	if not mouse_event.pressed:
		return
	if mouse_event.button_index == MOUSE_BUTTON_RIGHT:
		_show_entry_context_menu(mouse_event.position)
		return
	if mouse_event.button_index != MOUSE_BUTTON_LEFT:
		return

	var item: TreeItem = _view.get_error_tree().get_item_at_position(mouse_event.position)
	if item == null or mouse_event.position.x <= _ENTRY_TOGGLE_GUTTER_X:
		return
	if _tree_renderer.entry_from_tree_item(item).is_empty():
		return

	item.collapsed = not item.collapsed
	_tree_renderer.remember_entry_state(item)

func on_entry_context_menu_id_pressed(id: int) -> void:
	if id != _CONTEXT_COPY_ID or _context_entry_to_copy.is_empty():
		return
	DisplayServer.clipboard_set(_context_entry_to_copy)

func on_copy_all_pressed() -> void:
	var visible_entries: PackedStringArray = _entry_filter.get_visible_entries()
	if visible_entries.is_empty():
		return
	DisplayServer.clipboard_set("\n\n".join(visible_entries))

func on_expand_all_pressed() -> void:
	_tree_renderer.set_all_entries_collapsed(false)

func on_collapse_all_pressed() -> void:
	_tree_renderer.set_all_entries_collapsed(true)

func _show_entry_context_menu(position: Vector2) -> void:
	var item: TreeItem = _view.get_error_tree().get_item_at_position(position)
	if item == null:
		return
	_context_entry_to_copy = _tree_renderer.entry_from_tree_item(item)
	if _context_entry_to_copy.is_empty():
		return

	var menu: PopupMenu = _view.get_entry_context_menu()
	menu.position = DisplayServer.mouse_get_position() + Vector2i(0, _CONTEXT_MENU_Y_OFFSET)
	menu.reset_size()
	menu.popup()
