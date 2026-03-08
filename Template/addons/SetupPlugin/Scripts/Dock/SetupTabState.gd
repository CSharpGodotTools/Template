@tool
class_name SetupTabState
extends SetupTabLayout

const ANTI_ALIASING_PATH_2D: String = "rendering/anti_aliasing/quality/msaa_2d"
const ANTI_ALIASING_PATH_3D: String = "rendering/anti_aliasing/quality/msaa_3d"
const MAIN_SCENES_PATH: String = "res://MainScenes"
const PROJECT_ROOT_PATH: String = "res://"
const REBUILD_INSTRUCTION: String = "Rebuild the project, then disable and re-enable the Setup Plugin."

var _project_setup_runner: ProjectSetupRunner
var _runtime_state_validator: SetupRuntimeStateValidator
var _template_catalog: SetupTemplateCatalog

var _selected_project_type: String = ""
var _selected_template_type: String = ""
var _selected_anti_aliasing_type: int = 0
var _runtime_state_error: String = ""
var _is_runtime_state_valid: bool

func _initialize_services() -> void:
	var project_root: String = ProjectSettings.globalize_path(PROJECT_ROOT_PATH)
	var main_scenes_root: String = ProjectSettings.globalize_path(MAIN_SCENES_PATH)

	_runtime_state_validator = SetupRuntimeStateValidator.new(project_root, main_scenes_root)
	_project_setup_runner = ProjectSetupRunner.new(project_root, main_scenes_root)

func _release_restart_dialog() -> void:
	if _confirm_restart_dialog == null:
		return

	if not is_instance_valid(_confirm_restart_dialog):
		_confirm_restart_dialog = null
		return

	var parent: Node = _confirm_restart_dialog.get_parent()
	if parent != null:
		parent.remove_child(_confirm_restart_dialog)

	_confirm_restart_dialog.queue_free()
	_confirm_restart_dialog = null

func _validate_and_initialize_state() -> void:
	var validation := _runtime_state_validator.try_validate()
	if not validation["valid"]:
		_mark_runtime_state_invalid(validation["reason"], null)
		return

	var main_scenes_root: String = ProjectSettings.globalize_path(MAIN_SCENES_PATH)
	var catalog_failure: String
	var template_catalog := SetupTemplateCatalog.try_load(main_scenes_root, catalog_failure)
	if template_catalog == null:
		_mark_runtime_state_invalid(catalog_failure, null)
		return

	_template_catalog = template_catalog

	var selection := _template_catalog.get_first_selection()
	if selection.is_empty():
		_mark_runtime_state_invalid("No setup template options are available.", null)
		return

	_selected_project_type = selection["project_type"]
	_selected_template_type = selection["template_type"]

	_populate_project_type_options()
	_populate_template_type_options(_selected_project_type)

	_project_type.select(0)
	_template_type.select(0)

	_is_runtime_state_valid = true
	_runtime_state_error = ""
	_set_controls_enabled(true)
	_set_status("Setup plugin is ready.", false)

func _populate_project_type_options() -> void:
	_project_type.clear()

	var project_types: Array = _template_catalog.project_types
	for project_type in project_types:
		_project_type.add_item(project_type)

func _populate_template_type_options(project_type: String) -> void:
	_template_type.clear()

	var template_types: Array = _template_catalog.get_templates(project_type)
	if template_types.is_empty():
		_selected_template_type = ""
		return

	for template_type in template_types:
		_template_type.add_item(template_type)

	_template_type.select(0)
	_selected_template_type = _template_type.get_item_text(0)

func _set_anti_aliasing_type(type: int) -> void:
	if _selected_project_type == "2D":
		_selected_anti_aliasing_type = type
		ProjectSettings.set_setting(ANTI_ALIASING_PATH_2D, type)
		return

	if _selected_project_type == "3D":
		_selected_anti_aliasing_type = type
		ProjectSettings.set_setting(ANTI_ALIASING_PATH_3D, type)
		return

	printerr("Cannot set anti aliasing setting: Unknown project type: ", _selected_project_type)

func _ensure_runtime_state_valid(operation_name: String) -> bool:
	if _is_runtime_state_valid:
		return true

	var message: String = _runtime_state_error
	if message.is_empty():
		message = REBUILD_INSTRUCTION

	_set_status(message, true)
	printerr("Setup operation blocked (%s). %s" % [operation_name, message])
	return false

func _mark_runtime_state_invalid(reason: String, exception: Variant = null) -> void:
	_is_runtime_state_valid = false
	_runtime_state_error = "%s %s" % [reason, REBUILD_INSTRUCTION]

	_set_controls_enabled(false)
	_set_status(_runtime_state_error, true)

	printerr(_runtime_state_error)

	if exception == null:
		return

	printerr(str(exception))

func _report_user_error(message: String) -> void:
	_game_name_preview.text = message
	_set_status(message, true)
	printerr(message)
