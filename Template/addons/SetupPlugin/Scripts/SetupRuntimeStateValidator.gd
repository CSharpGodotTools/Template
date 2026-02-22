class_name SetupRuntimeStateValidator
extends Node

const TEMPLATE_PROJECT_NAME: String = "Template"

var _project_root: String
var _main_scenes_root: String

func _init(project_root: String, main_scenes_root: String) -> void:
	_project_root = project_root
	_main_scenes_root = main_scenes_root

func try_validate(failure_reason: String) -> bool:
	if _project_root.is_empty() or not DirAccess.dir_exists_absolute(_project_root):
		failure_reason = "Project root could not be resolved."
		return false
	
	if not FileAccess.file_exists(_project_root.path_join("project.godot")):
		failure_reason = "The project.godot file is missing."
		return false
	
	if not FileAccess.file_exists(_project_root.path_join("%s.csproj" % TEMPLATE_PROJECT_NAME)):
		failure_reason = "%s.csproj was not found." % TEMPLATE_PROJECT_NAME
		return false
	
	if not FileAccess.file_exists(_project_root.path_join("%s.sln" % TEMPLATE_PROJECT_NAME)):
		failure_reason = "%s.sln was not found." % TEMPLATE_PROJECT_NAME
		return false
	
	if not DirAccess.dir_exists_absolute(_main_scenes_root):
		failure_reason = "Setup templates directory was not found: %s" % _main_scenes_root
		return false
	
	var formatted_name: String = GameNameRules.format_game_name("Validation")
	var valid_characters: bool = GameNameRules.is_alpha_numeric_and_allow_spaces(formatted_name)
	
	if not valid_characters:
		failure_reason = "Internal setup validation failed for game-name rules."
		return false
	
	failure_reason = ""
	return true
