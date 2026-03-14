# Provides a recursive pass to remove empty directories from the project tree.
# Run at the start of setup to eliminate stale placeholder folders left over
# from a previous partial or cancelled setup run.
class_name SetupDirectoryMaintenance

# Entry point: starts a recursive scan from `root_directory`.
# The root directory itself is never deleted even if empty.
static func delete_empty_directories(root_directory: String) -> int:
	if not DirAccess.dir_exists_absolute(root_directory):
		return 0
	
	return delete_empty_directories_recursive(root_directory, true)

# Recursively checks each child directory; deletes it if it contains no files
# and no non-empty subdirectories.  Returns the count of directories deleted.
static func delete_empty_directories_recursive(directory: String, is_root_directory: bool) -> int:
	var dir: DirAccess = DirAccess.open(directory)
	if dir == null:
		return 0
	
	var child_directories: Array[String] = []
	var has_files: bool = false
	var has_non_empty_children: bool = false
	var removed_count: int = 0
	
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
		removed_count += delete_empty_directories_recursive(child_directory, false)
		if DirAccess.dir_exists_absolute(child_directory):
			has_non_empty_children = true
	
	var has_content: bool = has_files or has_non_empty_children
	if not has_content and not is_root_directory:
		if DirAccess.remove_absolute(directory) == OK:
			removed_count += 1
	
	return removed_count
