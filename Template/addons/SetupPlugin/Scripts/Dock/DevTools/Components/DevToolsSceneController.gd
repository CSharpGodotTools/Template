@tool
class_name DevToolsSceneController
extends RefCounted

const EMPTY_COUNT: int = 0
const STATUS_SCENE_TABS_TEMPLATE: String = "Closed %d scene tabs"
const STATUS_NO_SCENE_TABS: String = "No scene tabs to close"
const STATUS_RESTARTING_EDITOR: String = "Restarting editor..."
const STATUS_EXPANDED_TEMPLATE: String = "Expanded hierarchy to level %d"
const STATUS_NO_HIERARCHY: String = "No scene hierarchy available"
const STATUS_EXPANDED_FULL: String = "Fully expanded hierarchy"
const STATUS_COLLAPSED_FULL: String = "Fully collapsed hierarchy"

var _editor_scene_actions: EditorSceneActions
var _scene_hierarchy_actions: SceneHierarchyActions
var _status_feedback: DevToolsStatusFeedback

func _init(editor_scene_actions: EditorSceneActions, scene_hierarchy_actions: SceneHierarchyActions, status_feedback: DevToolsStatusFeedback) -> void:
	_editor_scene_actions = editor_scene_actions
	_scene_hierarchy_actions = scene_hierarchy_actions
	_status_feedback = status_feedback

func close_all_scene_tabs() -> void:
	var closed_count: int = _editor_scene_actions.close_all_open_scenes()
	var message: String = STATUS_SCENE_TABS_TEMPLATE % closed_count if closed_count > EMPTY_COUNT else STATUS_NO_SCENE_TABS
	_status_feedback.show(message)

func restart_editor() -> void:
	_status_feedback.show(STATUS_RESTARTING_EDITOR)
	_editor_scene_actions.restart_editor(true)

func expand_to_level(level: int) -> void:
	var changed_count: int = _scene_hierarchy_actions.expand_to_level(level)
	var message: String = STATUS_EXPANDED_TEMPLATE % level if changed_count > EMPTY_COUNT else STATUS_NO_HIERARCHY
	_status_feedback.show(message)

func fully_expand() -> void:
	var message: String = STATUS_EXPANDED_FULL if _scene_hierarchy_actions.fully_expand() > EMPTY_COUNT else STATUS_NO_HIERARCHY
	_status_feedback.show(message)

func fully_collapse() -> void:
	var message: String = STATUS_COLLAPSED_FULL if _scene_hierarchy_actions.fully_collapse() > EMPTY_COUNT else STATUS_NO_HIERARCHY
	_status_feedback.show(message)
