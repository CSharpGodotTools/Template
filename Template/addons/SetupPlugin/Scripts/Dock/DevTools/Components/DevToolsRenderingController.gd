@tool
class_name DevToolsRenderingController
extends RefCounted

const DEFAULT_CLEAR_COLOR_PATH: String = "rendering/environment/defaults/default_clear_color"
const ANTI_ALIASING_PATH_2D: String = "rendering/anti_aliasing/quality/msaa_2d"
const ANTI_ALIASING_PATH_3D: String = "rendering/anti_aliasing/quality/msaa_3d"
const STATUS_CLEAR_COLOR_UPDATED: String = "Updated clear color."
const STATUS_ANTI_ALIASING_UPDATED: String = "Anti-aliasing updated."

var _status_feedback: DevToolsStatusFeedback

func _init(status_feedback: DevToolsStatusFeedback) -> void:
	_status_feedback = status_feedback

func set_clear_color(color: Color) -> void:
	ProjectSettings.set_setting(DEFAULT_CLEAR_COLOR_PATH, color)
	ProjectSettings.save()
	_status_feedback.show(STATUS_CLEAR_COLOR_UPDATED)

func set_anti_aliasing(index: int) -> void:
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_2D, index)
	ProjectSettings.set_setting(ANTI_ALIASING_PATH_3D, index)
	_status_feedback.show(STATUS_ANTI_ALIASING_UPDATED)
