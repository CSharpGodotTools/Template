# Provides a recursive directory scanner that deletes orphaned .uid files.
# A .uid file is considered orphaned when its corresponding source file
# (same name without the ".uid" suffix) no longer exists in the same directory.
extends RefCounted

# Recursively scans `directory` and deletes any .uid file that has no matching
# sibling.  Returns the total number of files deleted.
static func delete_orphan_uid_files(directory: String) -> int:
	var dir: DirAccess = DirAccess.open(directory)
	if dir == null:
		return 0

	var removed_count: int = 0
	var subdirectories: Array[String] = []
	var files_in_dir: Array[String] = []
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	while file_name != "":
		if not file_name.begins_with("."):
			if dir.current_is_dir():
				subdirectories.append(directory.path_join(file_name))
			else:
				files_in_dir.append(file_name)
		file_name = dir.get_next()
	dir.list_dir_end()

	for uid_file in files_in_dir:
		if uid_file.ends_with(".uid") and not files_in_dir.has(uid_file.trim_suffix(".uid")):
			DirAccess.remove_absolute(directory.path_join(uid_file))
			removed_count += 1

	for subdirectory in subdirectories:
		removed_count += delete_orphan_uid_files(subdirectory)
	return removed_count