@tool
class_name DevToolsStatusFeedback
extends RefCounted

const PLACEHOLDER_TEXT: String = " "
const STATUS_IDLE_COLOR: Color = Color(0.75, 0.75, 0.75)
const STATUS_OK_COLOR: Color = Color(0.6, 0.95, 0.6)

var _label: Label
var _timer: Timer

func _init(label: Label, timer: Timer) -> void:
	_label = label
	_timer = timer
	_apply_placeholder()

func _apply_placeholder() -> void:
	if _label == null:
		return
	_label.text = PLACEHOLDER_TEXT
	_label.modulate = STATUS_IDLE_COLOR

func show(message: String) -> void:
	if message.is_empty():
		_apply_placeholder()
		return
	if _label == null:
		return
	_label.text = message
	_label.modulate = STATUS_OK_COLOR
	if _timer != null:
		_timer.start()

func clear() -> void:
	_apply_placeholder()

func on_timer_timeout() -> void:
	_apply_placeholder()
