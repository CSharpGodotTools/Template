@tool
extends EditorPlugin

const TEMPLATE_PROJECT_NAME: String = "Template"
const PROJECT_ROOT_PATH: String = "res://"
const DevToolsTab = preload("res://addons/SetupPlugin/Scripts/Dock/DevToolsTab.gd")
const TemplateSetupDock = preload("res://addons/SetupPlugin/Scripts/Dock/TemplateSetupDock.gd")

var _dock: EditorDock
var _content: TemplateSetupDock
var _dev_dock: EditorDock

func _enter_tree() -> void:
	var project_root: String = ProjectSettings.globalize_path(PROJECT_ROOT_PATH)

	# ensure any previous dev dock is removed (happens on plugin re‑enable)
	if _dev_dock != null and is_instance_valid(_dev_dock):
		if _dev_dock.get_parent() != null:
			remove_dock(_dev_dock)
		_dev_dock.queue_free()
	
	_dev_dock = EditorDock.new()
	_dev_dock.title = "Dev Tools"
	_dev_dock.default_slot = EditorDock.DockSlot.DOCK_SLOT_BOTTOM
	_dev_dock.available_layouts = EditorDock.DockLayout.DOCK_LAYOUT_VERTICAL
	var dev_content: DevToolsTab = DevToolsTab.new()
	_dev_dock.add_child(dev_content)
	add_dock(_dev_dock)

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
	# remove dev tools dock when plugin disabled
	if _dev_dock != null and is_instance_valid(_dev_dock):
		if _dev_dock.get_parent() != null:
			remove_dock(_dev_dock)
		_dev_dock.queue_free()
		_dev_dock = null
	
	if _dock == null:
		return
	
	if not is_instance_valid(_dock):
		_content = null
		_dock = null
		return
	
	if _content != null and is_instance_valid(_content):
		_content.prepare_for_plugin_disable()
	
	if _dock.get_parent() != null:
		remove_dock(_dock)
	_dock.queue_free()
	_content = null
	_dock = null
