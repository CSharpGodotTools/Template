@tool
extends RefCounted

const _REFRESH_BUTTON_WIDTH: int = 110
const _COPY_BUTTON_WIDTH: int = 120
const _TOGGLE_BUTTON_WIDTH: int = 130
const _TREE_MIN_HEIGHT: int = 200
const _TREE_FONT_SIZE: int = 15
const _TIMESTAMP_COLUMN_WIDTH: int = 110

var _host: VBoxContainer
var _buttons: Dictionary = {}
var _checkboxes: Dictionary = {}
var _filter_edit: LineEdit
var _error_tree: Tree
var _tree_scroll_container: ScrollContainer
var _tree_panel_style: StyleBoxFlat
var _entry_context_menu: PopupMenu
var _settings_popup: PopupPanel

func _init(host: VBoxContainer, settings_popup_script: Script, settings) -> void:
	_host = host
	_create_controls(settings_popup_script, settings)
	_build_layout()

func get_button(key: StringName) -> Button:
	if not _buttons.has(key):
		return null
	return _buttons[key] as Button

func get_checkbox(key: StringName) -> CheckButton:
	if not _checkboxes.has(key):
		return null
	return _checkboxes[key] as CheckButton

func get_filter_edit() -> LineEdit:
	return _filter_edit

func get_error_tree() -> Tree:
	return _error_tree

func get_entry_context_menu() -> PopupMenu:
	return _entry_context_menu

func get_settings_popup() -> PopupPanel:
	return _settings_popup

func get_option_checkboxes() -> Array[CheckButton]:
	return [
		get_checkbox(&"stack_trace"),
		get_checkbox(&"short_type_names"),
		get_checkbox(&"duplicates"),
		get_checkbox(&"timestamps"),
		get_checkbox(&"errors"),
		get_checkbox(&"warnings")
	]

func apply_timestamp_column_visibility(show_timestamps: bool) -> void:
	if show_timestamps:
		_error_tree.columns = 2
		_error_tree.set_column_expand(0, false)
		_error_tree.set_column_custom_minimum_width(0, _TIMESTAMP_COLUMN_WIDTH)
		_error_tree.set_column_expand(1, true)
		return
	_error_tree.columns = 1
	_error_tree.set_column_expand(0, true)
	_error_tree.set_column_custom_minimum_width(0, 0)

func apply_color_theme(settings) -> void:
	var tree_color: Color = settings.color_tree_font if settings.colors_enabled else settings.DEFAULT_COLOR_TREE_FONT
	var background: Color = settings.color_panel_background if settings.colors_enabled else settings.DEFAULT_COLOR_PANEL_BACKGROUND
	_error_tree.add_theme_color_override("font_color", tree_color)
	_tree_panel_style.bg_color = background

func update_dock_title(visible_count: int) -> void:
	var parent_node: Node = _host.get_parent()
	if parent_node == null:
		return
	var title_text: String = "Debugger+"
	if visible_count > 0:
		title_text = "Debugger+ (%d)" % visible_count
	parent_node.set("title", title_text)

func update_popup_prefix_cases(warning_case: int, error_case: int) -> void:
	if _settings_popup != null and _settings_popup.has_method("set_prefix_cases"):
		_settings_popup.call("set_prefix_cases", warning_case, error_case)

func _create_controls(settings_popup_script: Script, settings) -> void:
	_buttons[&"refresh"] = _new_button("Refresh", _REFRESH_BUTTON_WIDTH)
	_buttons[&"copy_all"] = _new_button("Copy All", _COPY_BUTTON_WIDTH)
	_buttons[&"expand_all"] = _new_button("Expand All", _TOGGLE_BUTTON_WIDTH)
	_buttons[&"collapse_all"] = _new_button("Collapse All", _TOGGLE_BUTTON_WIDTH)
	_buttons[&"settings"] = _new_button("Settings", _TOGGLE_BUTTON_WIDTH)
	get_button(&"settings").tooltip_text = "Open Debugger+ settings."

	_filter_edit = LineEdit.new()
	_filter_edit.placeholder_text = "Filter errors (message, source, stack)"
	_filter_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	_checkboxes[&"stack_trace"] = _new_checkbox("Stack Trace", "Include stack trace details for errors.", true)
	_checkboxes[&"short_type_names"] = _new_checkbox("Short Type Names", "Use compact type names in messages and stack frames.", true)
	_checkboxes[&"duplicates"] = _new_checkbox("Duplicates", "Show duplicate entries instead of collapsing identical ones.", false)
	_checkboxes[&"timestamps"] = _new_checkbox("Timestamps", "Show elapsed timestamps for entries.", true)
	_checkboxes[&"errors"] = _new_checkbox("Errors", "Toggle visibility of errors.", true)
	_checkboxes[&"warnings"] = _new_checkbox("Warnings", "Toggle visibility of warnings.", true)
	_checkboxes[&"dev"] = _new_checkbox("Dev", "Enables additional debug info meant for project maintainers.", false)

	_error_tree = Tree.new()
	_error_tree.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_error_tree.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_error_tree.custom_minimum_size = Vector2(0, _TREE_MIN_HEIGHT)
	_error_tree.select_mode = Tree.SelectMode.SELECT_ROW
	_error_tree.columns = 2
	_error_tree.set_column_expand(0, false)
	_error_tree.set_column_custom_minimum_width(0, _TIMESTAMP_COLUMN_WIDTH)
	_error_tree.set_column_expand(1, true)
	_error_tree.hide_root = true
	_error_tree.auto_tooltip = false
	_error_tree.add_theme_font_size_override("font_size", _TREE_FONT_SIZE)
	_error_tree.add_theme_color_override("font_color", settings.color_tree_font)

	_tree_scroll_container = ScrollContainer.new()
	_tree_scroll_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_tree_scroll_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_tree_scroll_container.custom_minimum_size = Vector2(0, _TREE_MIN_HEIGHT)
	_tree_panel_style = StyleBoxFlat.new()
	_tree_panel_style.bg_color = settings.color_panel_background
	_tree_scroll_container.add_theme_stylebox_override("panel", _tree_panel_style)
	_tree_scroll_container.add_child(_error_tree)

	_entry_context_menu = PopupMenu.new()
	_entry_context_menu.add_item("Copy Error", 0)

	_settings_popup = settings_popup_script.new()
	if _settings_popup.has_method("set_option_controls"):
		_settings_popup.call("set_option_controls", get_option_checkboxes())
	if _settings_popup.has_method("set_dev_control"):
		_settings_popup.call("set_dev_control", get_checkbox(&"dev"))
	if _settings_popup.has_method("set_prefix_cases"):
		_settings_popup.call("set_prefix_cases", settings.warning_prefix_case, settings.error_prefix_case)

func _build_layout() -> void:
	_host.add_theme_constant_override("separation", 8)

	var toolbar: HBoxContainer = HBoxContainer.new()
	toolbar.add_theme_constant_override("separation", 8)
	toolbar.add_child(get_button(&"refresh"))
	toolbar.add_child(get_button(&"copy_all"))
	toolbar.add_child(get_button(&"expand_all"))
	toolbar.add_child(get_button(&"collapse_all"))
	toolbar.add_child(get_button(&"settings"))

	var sections: VBoxContainer = VBoxContainer.new()
	sections.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	sections.size_flags_vertical = Control.SIZE_EXPAND_FILL
	sections.add_theme_constant_override("separation", 6)
	sections.add_child(_tree_scroll_container)

	_host.add_child(toolbar)
	_host.add_child(_filter_edit)
	_host.add_child(sections)
	_host.add_child(_entry_context_menu)
	_host.add_child(_settings_popup)

func _new_button(text: String, min_width: int) -> Button:
	var button: Button = Button.new()
	button.text = text
	button.custom_minimum_size = Vector2(min_width, 0)
	return button

func _new_checkbox(text: String, tooltip_text: String, pressed: bool) -> CheckButton:
	var checkbox: CheckButton = CheckButton.new()
	checkbox.text = text
	checkbox.tooltip_text = tooltip_text
	checkbox.button_pressed = pressed
	return checkbox
