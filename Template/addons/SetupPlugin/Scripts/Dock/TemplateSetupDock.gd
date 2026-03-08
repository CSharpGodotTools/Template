@tool
class_name TemplateSetupDock
extends VBoxContainer

var _setup_tab: SetupTab
var _dev_tools_tab: DevToolsTab
var _is_disposed: bool

func _ready() -> void:
	var tab_container: TabContainer = TabContainer.new()
	add_child(tab_container)

	_setup_tab = SetupTab.new()
	tab_container.add_child(_setup_tab)
	tab_container.set_tab_title(0, "Setup")

	_dev_tools_tab = DevToolsTab.new()
	tab_container.add_child(_dev_tools_tab)
	tab_container.set_tab_title(1, "Dev Tools")

func _exit_tree() -> void:
	prepare_for_plugin_disable()

func prepare_for_plugin_disable() -> void:
	if _is_disposed:
		return

	_is_disposed = true

	if _setup_tab != null:
		_setup_tab.prepare_for_disable()

	if _dev_tools_tab != null:
		_dev_tools_tab.prepare_for_disable()
