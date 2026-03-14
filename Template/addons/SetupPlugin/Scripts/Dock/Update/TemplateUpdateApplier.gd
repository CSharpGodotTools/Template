@tool
class_name TemplateUpdateApplier
extends RefCounted

const UpdateFileOps = preload("res://addons/SetupPlugin/Scripts/Dock/Update/UpdateFileOps.gd")

func apply(template_root: String, project_root: String, status_callback: Callable) -> Dictionary:
	var result: Dictionary = UpdateFileOps.replace_directory(template_root.path_join("Framework"), project_root.path_join("Framework"))
	if not result.get("success", false):
		return result

	var example_mod_target: String = project_root.path_join("Mods/Example Mod")
	if DirAccess.dir_exists_absolute(example_mod_target):
		_notify(status_callback, "Updating Mods/Example Mod...")
		result = UpdateFileOps.replace_directory(template_root.path_join("Mods/Example Mod"), example_mod_target)
		if not result.get("success", false):
			return result

	_notify(status_callback, "Updating addons/SetupPlugin...")
	result = UpdateFileOps.replace_directory(template_root.path_join("addons/SetupPlugin"), project_root.path_join("addons/SetupPlugin"))
	if not result.get("success", false):
		return result

	_notify(status_callback, "Updating script_templates files...")
	result = UpdateFileOps.merge_directory(template_root.path_join("script_templates"), project_root.path_join("script_templates"))
	if not result.get("success", false):
		return result

	_notify(status_callback, "Updating NuGet.config...")
	result = UpdateFileOps.replace_file(template_root.path_join("NuGet.config"), project_root.path_join("NuGet.config"))
	if not result.get("success", false):
		return result

	_notify(status_callback, "Synchronizing .gdignore files...")
	result = _sync_gdignore_files(template_root, project_root)
	if not result.get("success", false):
		return result

	_notify(status_callback, "Synchronizing project .csproj...")
	var csproj_sync = load("res://addons/SetupPlugin/Scripts/Dock/Update/TemplateCsprojSynchronizer.gd").new()
	result = csproj_sync.sync_template_settings(template_root.path_join("Template.csproj"), project_root)
	if not result.get("success", false):
		return result

	return {
		"success": true,
		"message": "Update complete. Build project and restart editor if script reload warnings appear."
	}

func _sync_gdignore_files(template_root: String, project_root: String) -> Dictionary:
	for relative_path in ["script_templates/.gdignore", "Properties/.gdignore", "gdunit4_testadapter_v5/.gdignore"]:
		var source_file: String = template_root.path_join(relative_path)
		if not FileAccess.file_exists(source_file):
			continue
		var target_file: String = project_root.path_join(relative_path)
		if not UpdateFileOps.copy_file(source_file, target_file):
			return {"success": false, "message": "Failed to copy .gdignore file: %s" % relative_path}

	return {"success": true}

func _notify(status_callback: Callable, message: String) -> void:
	if status_callback.is_valid():
		status_callback.call(message)
