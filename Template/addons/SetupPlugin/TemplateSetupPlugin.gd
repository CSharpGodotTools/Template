@tool
extends EditorPlugin

const TEMPLATE_PROJECT_NAME: String = "Template"
const PROJECT_ROOT_PATH: String = "res://"
const TemplateSetupDock = preload("res://addons/SetupPlugin/Scripts/TemplateSetupDock.gd")

var _dock: EditorDock
var _content: TemplateSetupDock

func _enter_tree() -> void:
	var project_root: String = ProjectSettings.globalize_path(PROJECT_ROOT_PATH)
	
	# If Template.csproj does not exist then assume the setup runner has finished and the Godot
	# editor has restarted. Developers should no longer be interacting with this addon so we return.
	if not FileAccess.file_exists(project_root.path_join("%s.csproj" % TEMPLATE_PROJECT_NAME)):
		return
	
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
