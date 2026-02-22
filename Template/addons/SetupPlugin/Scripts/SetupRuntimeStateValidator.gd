class_name SetupRuntimeStateValidator
extends Node

const TEMPLATE_PROJECT_NAME: String = "Template"

var _project_root: String
var _main_scenes_root: String

func _init(project_root: String, main_scenes_root: String) -> void:
	_project_root = project_root
	_main_scenes_root = main_scenes_root

func try_validate() -> Dictionary:
	if _project_root.is_empty() or not DirAccess.dir_exists_absolute(_project_root):
		return {"valid": false, "reason": "Project root could not be resolved."}

	if not FileAccess.file_exists(_project_root.path_join("project.godot")):
		return {"valid": false, "reason": "The project.godot file is missing."}

	if not FileAccess.file_exists(_project_root.path_join("%s.csproj" % TEMPLATE_PROJECT_NAME)):
		return {"valid": false, "reason": "%s.csproj was not found." % TEMPLATE_PROJECT_NAME}

	if not FileAccess.file_exists(_project_root.path_join("%s.sln" % TEMPLATE_PROJECT_NAME)):
		return {"valid": false, "reason": "%s.sln was not found." % TEMPLATE_PROJECT_NAME}

	if not DirAccess.dir_exists_absolute(_main_scenes_root):
		return {"valid": false, "reason": "Setup templates directory was not found: %s" % _main_scenes_root}

	var formatted_name: String = GameNameRules.format_game_name("Validation")
	var valid_characters: bool = GameNameRules.is_alpha_numeric_and_allow_spaces(formatted_name)

	if not valid_characters:
		return {"valid": false, "reason": "Internal setup validation failed for game-name rules."}

	return {"valid": true, "reason": ""}
