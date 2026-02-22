class_name SetupDirectoryMaintenance

static func delete_empty_directories(root_directory: String) -> void:
	if not DirAccess.dir_exists_absolute(root_directory):
		return
	
	delete_empty_directories_recursive(root_directory, true)

static func delete_empty_directories_recursive(directory: String, is_root_directory: bool) -> bool:
	var dir: DirAccess = DirAccess.open(directory)
	if dir == null:
		return false
	
	var child_directories: Array[String] = []
	var has_files: bool = false
	var has_non_empty_children: bool = false
	
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name == "." or file_name == "..":
			file_name = dir.get_next()
			continue
		
		var full_path: String = directory.path_join(file_name)
		
		if dir.current_is_dir():
			child_directories.append(full_path)
		else:
			has_files = true
		
		file_name = dir.get_next()
	
	for child_directory in child_directories:
		var child_has_content: bool = delete_empty_directories_recursive(child_directory, false)
		if child_has_content:
			has_non_empty_children = true
	
	var has_content: bool = has_files or has_non_empty_children
	if not has_content and not is_root_directory:
		DirAccess.remove_absolute(directory)
	
	return has_content
