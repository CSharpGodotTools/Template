@tool
# Root container node for the Setup dock panel.
# Owns and manages the lifetime of SetupTab.
class_name TemplateSetupDock
extends VBoxContainer

var _setup_tab: SetupTab
var _is_disposed: bool

func _ready() -> void:
	# Instantiate SetupTab and attach it as a child so it becomes part of the scene tree.
	_setup_tab = SetupTab.new()
	add_child(_setup_tab)

func _exit_tree() -> void:
	# Triggered when the dock is removed from the scene tree (e.g. plugin disabled).
	# Delegates to prepare_for_plugin_disable to avoid duplicated teardown.
	prepare_for_plugin_disable()

func prepare_for_plugin_disable() -> void:
	# Guards against being called more than once (e.g. _exit_tree + explicit call).
	if _is_disposed:
		return

	_is_disposed = true

	if _setup_tab != null:
		# Tell SetupTab to disconnect its signals before the node is freed.
		_setup_tab.prepare_for_disable()
