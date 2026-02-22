@tool
extends EditorPlugin

const TemplateSetupDock = preload("res://addons/SetupPlugin/Scripts/TemplateSetupDock.gd")

var _dock: EditorDock
var _content: TemplateSetupDock

func _enter_tree() -> void:
	_dock = EditorDock.new()
	_dock.title = "Setup"
	_dock.default_slot = EditorDock.DockSlot.DOCK_SLOT_RIGHT_BL
	_dock.available_layouts = EditorDock.DockLayout.DOCK_LAYOUT_VERTICAL
	
	_content = TemplateSetupDock.new()
	_dock.add_child(_content)
	
	add_dock(_dock)

func _exit_tree() -> void:
	if _dock == null:
		return
	
	if not is_instance_valid(_dock):
		_content = null
		_dock = null
		return
	
	if _content != null and is_instance_valid(_content):
		_content.prepare_for_plugin_disable()
	
	remove_dock(_dock)
	_dock.queue_free()
	_content = null
	_dock = null
