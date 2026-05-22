@tool
extends EditorPlugin

const DebuggerEventBridgeScript = preload("Scripts/Dock/Debugger/DebuggerEventBridge.gd")
const DebuggerPlusTabScript = preload("Scripts/Dock/DebuggerPlusTab.gd")

var _debugger_event_bridge
var _debugger_plus_dock: EditorDock
var _debugger_plus_content: VBoxContainer

func _enter_tree() -> void:
	# ensure any previous Debugger+ dock is removed (happens on plugin re-enable)
	if _debugger_plus_dock != null and is_instance_valid(_debugger_plus_dock):
		if _debugger_plus_dock.get_parent() != null:
			remove_dock(_debugger_plus_dock)
		_debugger_plus_dock.queue_free()

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
