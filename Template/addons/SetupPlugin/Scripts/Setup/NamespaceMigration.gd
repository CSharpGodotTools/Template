# Performs a text-substitution pass over all .cs files in the project to
# replace the placeholder namespace "__TEMPLATE__" with the chosen game name.
# By default skips addons/, Framework/, Mods/, and .godot/.
# Pass include_all_scripts = true to process every .cs file except .godot/.
class_name NamespaceMigration

# Entry point: opens the project root directory and starts the recursive walk.
static func rename_template_namespaces(project_root: String, new_namespace_name: String, include_all_scripts: bool = false) -> void:
	var dir: DirAccess = DirAccess.open(project_root)
	if dir == null:
		return
	
	_process_directory_recursive(dir, project_root, new_namespace_name, include_all_scripts)

# Walks the directory tree and calls process_script_file for each .cs file
# that should not be skipped according to should_skip_file.
static func _process_directory_recursive(dir: DirAccess, current_path: String, new_namespace_name: String, include_all_scripts: bool) -> void:
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name == "." or file_name == "..":
			file_name = dir.get_next()
			continue
		
		var full_path: String = current_path.path_join(file_name)
		
		if dir.current_is_dir():
			var sub_dir: DirAccess = DirAccess.open(full_path)
			if sub_dir != null:
				_process_directory_recursive(sub_dir, full_path, new_namespace_name, include_all_scripts)
		elif file_name.ends_with(".cs"):
			if not should_skip_file(full_path, include_all_scripts):
				process_script_file(full_path, new_namespace_name)
		
		file_name = dir.get_next()

# Replaces all occurrences of the __TEMPLATE__ namespace placeholder with
# new_namespace_name in a single .cs file (namespace, using, and qualified-name forms).
static func process_script_file(script_file: String, new_namespace_name: String) -> void:
	var script_text: String = FileAccess.get_file_as_string(script_file)
	
	script_text = script_text.replace(
		"namespace %s" % GameNameRules.RESERVED_RAW_TEMPLATE_NAMESPACE,
		"namespace %s" % new_namespace_name)
	
	script_text = script_text.replace(
		"using %s" % GameNameRules.RESERVED_RAW_TEMPLATE_NAMESPACE,
		"using %s" % new_namespace_name)
	
	script_text = script_text.replace(
		"%s." % GameNameRules.RESERVED_RAW_TEMPLATE_NAMESPACE,
		"%s." % new_namespace_name)
	
	var file: FileAccess = FileAccess.open(script_file, FileAccess.WRITE)
	if file != null:
		file.store_string(script_text)

# Returns true if the file should be excluded from namespace migration.
# Always excludes .godot/ internals.  Also excludes addons/, Framework/, and
# Mods/ unless include_all_scripts is true.
static func should_skip_file(file_path: String, include_all_scripts: bool = false) -> bool:
	var normalized_path: String = file_path.replace("\\", "/")
	var lower_path: String = normalized_path.to_lower()

	if include_all_scripts:
		return lower_path.contains("/.godot/")
	
	return lower_path.contains("/.godot/") \
		or lower_path.contains("/addons/") \
		or lower_path.contains("/godotutils/") \
		or lower_path.contains("/mods/") \
		or lower_path.contains("/framework/")
