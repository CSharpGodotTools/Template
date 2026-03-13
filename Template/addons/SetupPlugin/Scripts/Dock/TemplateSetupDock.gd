@tool
class_name TemplateSetupDock
extends VBoxContainer

var _setup_tab: SetupTab
var _is_disposed: bool

func _ready() -> void:
	_setup_tab = SetupTab.new()
	add_child(_setup_tab)

func _exit_tree() -> void:
	prepare_for_plugin_disable()

func prepare_for_plugin_disable() -> void:
	if _is_disposed:
		return

	_is_disposed = true

	if _setup_tab != null:
		_setup_tab.prepare_for_disable()
