@tool
class_name DevToolsUpdateResult
extends RefCounted

const EMPTY_TEXT: String = ""

var _success: bool
var _message: String
var _target: String

func _init(success: bool, message: String, target: String = EMPTY_TEXT) -> void:
	_success = success
	_message = message
	_target = target

func is_success() -> bool:
	return _success

func get_message() -> String:
	return _message

func get_target() -> String:
	return _target
