@tool
extends PopupPanel

signal color_changed(color_key: String, color: Color)
signal reset_defaults_requested
signal colors_enabled_toggled(enabled: bool)

const POPUP_MIN_SIZE := Vector2(460, 0)
const PICKER_MIN_SIZE := Vector2(88, 34)
const POPUP_MARGIN := 14

var _pickers_by_key: Dictionary = {}

func _ready() -> void:
	if get_child_count() > 0:
		return
	_build_ui()

func popup_centered_with_colors(colors_by_key: Dictionary) -> void:
	_apply_colors(colors_by_key)
	popup_centered()

func set_colors_enabled(enabled: bool) -> void:
	var toggle: CheckButton = _find_enabled_toggle()
	if toggle != null:
		toggle.button_pressed = enabled

func _build_ui() -> void:
	var margin: MarginContainer = MarginContainer.new()
	margin.add_theme_constant_override("margin_left", POPUP_MARGIN)
	margin.add_theme_constant_override("margin_top", POPUP_MARGIN)
	margin.add_theme_constant_override("margin_right", POPUP_MARGIN)
	margin.add_theme_constant_override("margin_bottom", POPUP_MARGIN)
	add_child(margin)

	var root: VBoxContainer = VBoxContainer.new()
	root.custom_minimum_size = POPUP_MIN_SIZE
	root.add_theme_constant_override("separation", 8)
	margin.add_child(root)

	var enabled_toggle: CheckButton = CheckButton.new()
	enabled_toggle.name = "EnabledToggle"
	enabled_toggle.text = "Enable Colors"
	enabled_toggle.button_pressed = true
	enabled_toggle.toggled.connect(_on_enabled_toggled)
	root.add_child(enabled_toggle)

	_add_picker_row(root, "Timestamp", "timestamp")
	_add_picker_row(root, "Source", "source")
	_add_picker_row(root, "Error", "entry_error")
	_add_picker_row(root, "Warning", "entry_warning")
	_add_picker_row(root, "Stack Header", "stack_header")
	_add_picker_row(root, "Stack Frame", "stack_frame")

	var actions: HBoxContainer = HBoxContainer.new()
	actions.alignment = BoxContainer.ALIGNMENT_END
	actions.add_theme_constant_override("separation", 8)
	root.add_child(actions)

	var reset_button: Button = Button.new()
	reset_button.text = "Reset to Defaults"
	reset_button.pressed.connect(_on_reset_pressed)
	actions.add_child(reset_button)

	var close_button: Button = Button.new()
	close_button.text = "Close"
	close_button.pressed.connect(_on_close_pressed)
	actions.add_child(close_button)

func _add_picker_row(parent: VBoxContainer, label_text: String, color_key: String) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	row.add_theme_constant_override("separation", 10)
	parent.add_child(row)

	var label: Label = Label.new()
	label.text = label_text
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(label)

	var picker: ColorPickerButton = ColorPickerButton.new()
	picker.custom_minimum_size = PICKER_MIN_SIZE
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
	var enabled_toggle: CheckButton = _find_enabled_toggle()
	if enabled_toggle != null and colors_by_key.has("enabled"):
		enabled_toggle.button_pressed = bool(colors_by_key["enabled"])

func _on_picker_color_changed(color: Color, color_key: String) -> void:
	color_changed.emit(color_key, color)

func _on_enabled_toggled(enabled: bool) -> void:
	for picker_key in _pickers_by_key.keys():
		var picker: ColorPickerButton = _pickers_by_key[picker_key] as ColorPickerButton
		if picker != null:
			picker.disabled = not enabled
	colors_enabled_toggled.emit(enabled)

func _on_reset_pressed() -> void:
	reset_defaults_requested.emit()

func _on_close_pressed() -> void:
	hide()

func _find_enabled_toggle() -> CheckButton:
	var margin: MarginContainer = get_child(0) as MarginContainer
	if margin == null or margin.get_child_count() == 0:
		return null
	var root: VBoxContainer = margin.get_child(0) as VBoxContainer
	if root == null:
		return null
	return root.get_node_or_null("EnabledToggle") as CheckButton
