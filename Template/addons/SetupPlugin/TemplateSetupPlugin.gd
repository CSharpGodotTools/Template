@tool
extends EditorPlugin

const TEMPLATE_PROJECT_NAME: String = "Template"
const PROJECT_ROOT_PATH: String = "res://"
const MAIN_SCENE_SETTING_PATH: String = "application/run/main_scene"
const ROOT_LEVEL_SCENE_PATH: String = "res://Level.tscn"
const DebuggerEventBridgeScript = preload("Scripts/Dock/Debugger/DebuggerEventBridge.gd")
const DebuggerPlusTabScript = preload("Scripts/Dock/Debugger/DebuggerPlusTab.gd")
const DevToolsTabScript = preload("Scripts/Dock/DevToolsTab.gd")
const TemplateSetupDock = preload("Scripts/Dock/TemplateSetupDock.gd")

var _dock: EditorDock
var _content: TemplateSetupDock
var _debugger_event_bridge
var _debugger_plus_dock: EditorDock
var _debugger_plus_content: VBoxContainer
var _dev_dock: EditorDock
# Entry point for the SetupPlugin editor plugin.
# Registers two docks:
#   - Dev Tools dock (always visible whilst the plugin is enabled)
#   - Setup dock     (only shown while Template.csproj still exists, meaning
#                     the one-time project setup has not yet been completed)

func _enter_tree() -> void:
	var project_root: String = ProjectSettings.globalize_path(PROJECT_ROOT_PATH)

	# ensure any previous Debugger+ dock is removed (happens on plugin re-enable)
	if _debugger_plus_dock != null and is_instance_valid(_debugger_plus_dock):
		if _debugger_plus_dock.get_parent() != null:
			remove_dock(_debugger_plus_dock)
		_debugger_plus_dock.queue_free()

	# ensure any previous dev dock is removed (happens on plugin re‑enable)
	if _dev_dock != null and is_instance_valid(_dev_dock):
		if _dev_dock.get_parent() != null:
			remove_dock(_dev_dock)
		_dev_dock.queue_free()

	_debugger_plus_dock = EditorDock.new()
	_debugger_plus_dock.title = "Debugger+"
	_debugger_plus_dock.default_slot = EditorDock.DockSlot.DOCK_SLOT_BOTTOM
	_debugger_plus_dock.available_layouts = EditorDock.DockLayout.DOCK_LAYOUT_VERTICAL
	_debugger_plus_content = DebuggerPlusTabScript.new()
	if _debugger_event_bridge == null:
		_debugger_event_bridge = DebuggerEventBridgeScript.new()
		add_debugger_plugin(_debugger_event_bridge)
	if _debugger_plus_content.has_method("attach_debugger_event_bridge"):
		_debugger_plus_content.call("attach_debugger_event_bridge", _debugger_event_bridge)
	_debugger_plus_dock.add_child(_debugger_plus_content)
	add_dock(_debugger_plus_dock)
	
	_dev_dock = EditorDock.new()
	# Resolve the absolute project root so we can probe for Template.csproj.
	_dev_dock.title = "Dev Tools"
	_dev_dock.default_slot = EditorDock.DockSlot.DOCK_SLOT_BOTTOM
	_dev_dock.available_layouts = EditorDock.DockLayout.DOCK_LAYOUT_VERTICAL
	var dev_content = DevToolsTabScript.new()
	_dev_dock.add_child(dev_content)
	add_dock(_dev_dock)

	# If Template.csproj does not exist then assume the setup runner has finished and the Godot
	# editor has restarted. Developers should no longer be interacting with this addon so we return.
	if not FileAccess.file_exists(project_root.path_join("%s.csproj" % TEMPLATE_PROJECT_NAME)):
		return

	# Before setup completion, prevent a stale Level.tscn main-scene value from
	# being persisted when editor/project settings are saved.
	_clear_setup_main_scene_if_needed()

	_dock = EditorDock.new()
	_dock.title = "Setup"
	_dock.default_slot = EditorDock.DockSlot.DOCK_SLOT_RIGHT_BL
	_dock.available_layouts = EditorDock.DockLayout.DOCK_LAYOUT_VERTICAL
	# If Template.csproj no longer exists the developer has already renamed the
	# project.  The Setup dock is not needed, so we exit early.
	
	_content = TemplateSetupDock.new()
	_dock.add_child(_content)
	
	add_dock(_dock)

func _clear_setup_main_scene_if_needed() -> void:
	var main_scene_value: String = str(ProjectSettings.get_setting(MAIN_SCENE_SETTING_PATH, "")).strip_edges()
	if main_scene_value != ROOT_LEVEL_SCENE_PATH:
		return

	ProjectSettings.set_setting(MAIN_SCENE_SETTING_PATH, "")
	ProjectSettings.save()

func _exit_tree() -> void:
	if _debugger_plus_content != null and is_instance_valid(_debugger_plus_content):
		_debugger_plus_content.prepare_for_disable()

	if _debugger_plus_dock != null and is_instance_valid(_debugger_plus_dock):
		if _debugger_plus_dock.get_parent() != null:
			remove_dock(_debugger_plus_dock)
		_debugger_plus_dock.queue_free()
	_debugger_plus_content = null
	_debugger_plus_dock = null

	if _debugger_event_bridge != null:
		remove_debugger_plugin(_debugger_event_bridge)
		_debugger_event_bridge = null

	# remove dev tools dock when plugin disabled
	if _dev_dock != null and is_instance_valid(_dev_dock):
		if _dev_dock.get_parent() != null:
			remove_dock(_dev_dock)
		_dev_dock.queue_free()
		_dev_dock = null
	
	# Remove and free the Dev Tools dock first.
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
