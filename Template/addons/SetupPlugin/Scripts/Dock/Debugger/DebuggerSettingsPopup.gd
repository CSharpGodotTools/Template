@tool
extends PopupPanel

signal color_changed(color_key: String, color: Color)
signal reset_defaults_requested
signal colors_enabled_toggled(enabled: bool)
signal warning_prefix_case_changed(mode: int)
signal error_prefix_case_changed(mode: int)

const POPUP_MIN_SIZE := Vector2(500, 0)
const PICKER_MIN_SIZE := Vector2(88, 34)
const TAB_CONTENT_MARGIN := 10
const ACTIONS_SIDE_MARGIN := 10
const ACTIONS_BOTTOM_MARGIN := 10

var _pickers_by_key: Dictionary = {}
var _options_host: VBoxContainer
var _dev_host: VBoxContainer
var _warning_case_option: OptionButton
var _error_case_option: OptionButton

func _ready() -> void:
	if get_child_count() > 0:
		return
	_build_ui()

func popup_centered_with_state(colors_by_key: Dictionary, colors_enabled: bool, warning_case: int, error_case: int) -> void:
	_apply_colors(colors_by_key)
	set_colors_enabled(colors_enabled)
	set_prefix_cases(warning_case, error_case)
	popup_centered()

func set_option_controls(controls: Array) -> void:
	if _options_host == null:
		return
	for child in _options_host.get_children():
		_options_host.remove_child(child)
	for control in controls:
		if not (control is Control):
			continue
		var node: Control = control as Control
		if node.get_parent() != null:
			node.get_parent().remove_child(node)
		_options_host.add_child(node)

func set_dev_control(control: Control) -> void:
	if _dev_host == null or control == null:
		return
	for child in _dev_host.get_children():
		_dev_host.remove_child(child)
	if control.get_parent() != null:
		control.get_parent().remove_child(control)
	_dev_host.add_child(control)

func set_colors_enabled(enabled: bool) -> void:
	var toggle: CheckButton = _find_enabled_toggle()
	if toggle != null:
		toggle.button_pressed = enabled

func set_prefix_cases(warning_case: int, error_case: int) -> void:
	if _warning_case_option != null:
		_warning_case_option.select(clampi(warning_case, 0, 2))
	if _error_case_option != null:
		_error_case_option.select(clampi(error_case, 0, 2))

func _build_ui() -> void:
	var root: VBoxContainer = VBoxContainer.new()
	root.custom_minimum_size = POPUP_MIN_SIZE
	root.add_theme_constant_override("separation", 8)
	root.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	root.size_flags_vertical = Control.SIZE_EXPAND_FILL
	add_child(root)

	var tabs: TabContainer = TabContainer.new()
	tabs.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	tabs.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(tabs)
	var tab_panel: StyleBox = tabs.get_theme_stylebox("panel", "TabContainer")
	if tab_panel != null:
		add_theme_stylebox_override("panel", tab_panel.duplicate())

	var options_tab: MarginContainer = MarginContainer.new()
	options_tab.name = "Options"
	options_tab.add_theme_constant_override("margin_left", TAB_CONTENT_MARGIN)
	options_tab.add_theme_constant_override("margin_top", TAB_CONTENT_MARGIN)
	options_tab.add_theme_constant_override("margin_right", TAB_CONTENT_MARGIN)
	options_tab.add_theme_constant_override("margin_bottom", TAB_CONTENT_MARGIN)
	tabs.add_child(options_tab)
	var options_content: VBoxContainer = VBoxContainer.new()
	options_content.add_theme_constant_override("separation", 8)
	options_tab.add_child(options_content)

	_options_host = VBoxContainer.new()
	_options_host.add_theme_constant_override("separation", 8)
	options_content.add_child(_options_host)

	_warning_case_option = OptionButton.new()
	_warning_case_option.add_item("None", 0)
	_warning_case_option.add_item("Lowercase", 1)
	_warning_case_option.add_item("UPPERCASE", 2)
	_warning_case_option.tooltip_text = "Choose how warning prefixes are displayed before warning entries."
	_warning_case_option.item_selected.connect(_on_warning_case_selected)
	options_content.add_child(_labeled_option_row("Warnings Prefix", _warning_case_option))

	_error_case_option = OptionButton.new()
	_error_case_option.add_item("None", 0)
	_error_case_option.add_item("Lowercase", 1)
	_error_case_option.add_item("UPPERCASE", 2)
	_error_case_option.tooltip_text = "Choose how error prefixes are displayed before error entries."
	_error_case_option.item_selected.connect(_on_error_case_selected)
	options_content.add_child(_labeled_option_row("Errors Prefix", _error_case_option))

	var colors_tab: MarginContainer = MarginContainer.new()
	colors_tab.name = "Colors"
	colors_tab.add_theme_constant_override("margin_left", TAB_CONTENT_MARGIN)
	colors_tab.add_theme_constant_override("margin_top", TAB_CONTENT_MARGIN)
	colors_tab.add_theme_constant_override("margin_right", TAB_CONTENT_MARGIN)
	colors_tab.add_theme_constant_override("margin_bottom", TAB_CONTENT_MARGIN)
	tabs.add_child(colors_tab)
	var colors_content: VBoxContainer = VBoxContainer.new()
	colors_content.add_theme_constant_override("separation", 8)
	colors_tab.add_child(colors_content)

	var enabled_toggle: CheckButton = CheckButton.new()
	enabled_toggle.name = "EnabledToggle"
	enabled_toggle.text = "Enable Colors"
	enabled_toggle.tooltip_text = "Enable or disable custom colors while keeping your saved color choices."
	enabled_toggle.button_pressed = true
	enabled_toggle.toggled.connect(_on_enabled_toggled)
	colors_content.add_child(enabled_toggle)

	_add_picker_row(colors_content, "Timestamp", "timestamp")
	_add_picker_row(colors_content, "Source", "source")
	_add_picker_row(colors_content, "Error", "entry_error")
	_add_picker_row(colors_content, "Warning", "entry_warning")
	_add_picker_row(colors_content, "Stack Header", "stack_header")
	_add_picker_row(colors_content, "Stack Frame", "stack_frame")

	var dev_tab: MarginContainer = MarginContainer.new()
	dev_tab.name = "Dev"
	dev_tab.add_theme_constant_override("margin_left", TAB_CONTENT_MARGIN)
	dev_tab.add_theme_constant_override("margin_top", TAB_CONTENT_MARGIN)
	dev_tab.add_theme_constant_override("margin_right", TAB_CONTENT_MARGIN)
	dev_tab.add_theme_constant_override("margin_bottom", TAB_CONTENT_MARGIN)
	tabs.add_child(dev_tab)
	_dev_host = VBoxContainer.new()
	_dev_host.add_theme_constant_override("separation", 8)
	dev_tab.add_child(_dev_host)

	var actions_margin: MarginContainer = MarginContainer.new()
	actions_margin.add_theme_constant_override("margin_left", ACTIONS_SIDE_MARGIN)
	actions_margin.add_theme_constant_override("margin_right", ACTIONS_SIDE_MARGIN)
	actions_margin.add_theme_constant_override("margin_bottom", ACTIONS_BOTTOM_MARGIN)
	root.add_child(actions_margin)

	var actions: HBoxContainer = HBoxContainer.new()
	actions.alignment = BoxContainer.ALIGNMENT_END
	actions.add_theme_constant_override("separation", 8)
	actions_margin.add_child(actions)

	var reset_button: Button = Button.new()
	reset_button.text = "Reset to Defaults"
	reset_button.tooltip_text = "Reset all settings in this popup to their default values."
	reset_button.pressed.connect(_on_reset_pressed)
	actions.add_child(reset_button)

	var close_button: Button = Button.new()
	close_button.text = "Close"
	close_button.tooltip_text = "Close the settings popup."
	close_button.pressed.connect(_on_close_pressed)
	actions.add_child(close_button)

func _labeled_option_row(label_text: String, option: OptionButton) -> HBoxContainer:
	var row: HBoxContainer = HBoxContainer.new()
	row.add_theme_constant_override("separation", 10)
	var label: Label = Label.new()
	label.text = label_text
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(label)
	row.add_child(option)
	return row

func _add_picker_row(parent: VBoxContainer, label_text: String, color_key: String) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	row.add_theme_constant_override("separation", 10)
	parent.add_child(row)

	var label: Label = Label.new()
	label.text = label_text
	label.tooltip_text = "Adjust the %s color used in Debugger+." % label_text
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(label)

	var picker: ColorPickerButton = ColorPickerButton.new()
	picker.custom_minimum_size = PICKER_MIN_SIZE
	picker.tooltip_text = "Pick a color for %s." % label_text
	picker.color_changed.connect(_on_picker_color_changed.bind(color_key))
	row.add_child(picker)

	_pickers_by_key[color_key] = picker

func _apply_colors(colors_by_key: Dictionary) -> void:
	for color_key in _pickers_by_key.keys():
		var picker: ColorPickerButton = _pickers_by_key[color_key] as ColorPickerButton
		if picker == null:
			continue
		if colors_by_key.has(color_key):
			picker.color = colors_by_key[color_key]

func _on_picker_color_changed(color: Color, color_key: String) -> void:
	color_changed.emit(color_key, color)

func _on_enabled_toggled(enabled: bool) -> void:
	for picker_key in _pickers_by_key.keys():
		var picker: ColorPickerButton = _pickers_by_key[picker_key] as ColorPickerButton
		if picker != null:
			picker.disabled = not enabled
	colors_enabled_toggled.emit(enabled)

func _on_warning_case_selected(index: int) -> void:
	warning_prefix_case_changed.emit(index)

func _on_error_case_selected(index: int) -> void:
	error_prefix_case_changed.emit(index)

func _on_reset_pressed() -> void:
	reset_defaults_requested.emit()

func _on_close_pressed() -> void:
	hide()

func _find_enabled_toggle() -> CheckButton:
	var root: VBoxContainer = get_child(0) as VBoxContainer
	if root == null or root.get_child_count() == 0:
		return null
	var tabs: TabContainer = root.get_child(0) as TabContainer
	if tabs == null:
		return null
	var colors_tab: MarginContainer = tabs.get_node_or_null("Colors") as MarginContainer
	if colors_tab == null:
		return null
	if colors_tab.get_child_count() == 0:
		return null
	var colors_content: VBoxContainer = colors_tab.get_child(0) as VBoxContainer
	if colors_content == null:
		return null
	return colors_content.get_node_or_null("EnabledToggle") as CheckButton
