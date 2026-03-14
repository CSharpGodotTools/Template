# Filesystem utilities used during the initial project setup:
#   - Creates .gdignore files in GdUnit4 test output folders so Godot does not
#     try to import test results
#   - Reads UID strings from .tscn scene files
#   - Verifies / fixes the namespace placeholder in the new-script template
class_name SetupFileSystem

const NEW_SCRIPT_TEMPLATE_PATH: String = "script_templates/Node/NewScript.cs"

# Creates "TestResults/" and "gdunit4_testadapter_v5/" directories if they do
# not exist, and places an empty .gdignore in each one.
static func ensure_gdignore_files_in_gdunit_test_folders(project_root: String) -> void:
	var folders: Array[String] = [
		"TestResults",
		"gdunit4_testadapter_v5"
	]
	
	for folder in folders:
		var folder_path: String = project_root.path_join(folder)
		DirAccess.make_dir_absolute(folder_path)
		
		var gdignore_path: String = folder_path.path_join(".gdignore")
		if FileAccess.file_exists(gdignore_path):
			continue
		
		var file: FileAccess = FileAccess.open(gdignore_path, FileAccess.WRITE)
		if file != null:
			file.store_string("")

# Parses the first line of a .tscn file to extract its "uid=..." value.
# Returns an empty string if the file cannot be read or has no uid field.
static func get_uid_from_scene_file(scene_file_path: String) -> String:
	if not FileAccess.file_exists(scene_file_path):
		return ""
	
	var file: FileAccess = FileAccess.open(scene_file_path, FileAccess.READ)
	if file == null:
		return ""
	
	var first_line: String = file.get_line()
	if first_line.is_empty() or not first_line.contains("gd_scene"):
		return ""
	
	var uid_part: PackedStringArray = first_line.split("uid=")
	if uid_part.size() < 2:
		return ""
	
	var uid_value: PackedStringArray = uid_part[1].split("\"")
	if uid_value.size() < 2:
		return ""
	
	return uid_value[1]

# Verifies that the new-script template no longer contains the __TEMPLATE__
# namespace placeholder.  Replaces it if still present.  Returns false and
# logs an error if it cannot be confirmed clean after the replacement.
static func ensure_script_template_namespace_replaced(project_root: String, namespace_name: String) -> bool:
	var script_template_path: String = project_root.path_join(NEW_SCRIPT_TEMPLATE_PATH)
	if not FileAccess.file_exists(script_template_path):
		push_error("Missing script template after setup: %s" % script_template_path)
		return false

	var script_text: String = FileAccess.get_file_as_string(script_template_path)
	if script_text.contains(GameNameRules.RESERVED_RAW_TEMPLATE_NAMESPACE):
		script_text = script_text.replace(GameNameRules.RESERVED_RAW_TEMPLATE_NAMESPACE, namespace_name)

		var file: FileAccess = FileAccess.open(script_template_path, FileAccess.WRITE)
		if file == null:
			push_error("Unable to update script template namespace: %s" % script_template_path)
			return false

		file.store_string(script_text)
		file.close()

	var updated_script_text: String = FileAccess.get_file_as_string(script_template_path)
	if updated_script_text.contains(GameNameRules.RESERVED_RAW_TEMPLATE_NAMESPACE):
		push_error("Script template namespace placeholder was not replaced: %s" % script_template_path)
		return false

	return true
