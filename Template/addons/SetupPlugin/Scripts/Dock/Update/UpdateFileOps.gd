@tool
class_name UpdateFileOps
extends RefCounted

static func replace_directory(source_path: String, target_path: String) -> Dictionary:
	if not DirAccess.dir_exists_absolute(source_path):
		return _error_result("Update source directory is missing: %s" % source_path)

	if DirAccess.dir_exists_absolute(target_path) and not delete_path_recursive(target_path):
		return _error_result("Failed to delete directory: %s" % target_path)

	if DirAccess.make_dir_recursive_absolute(target_path) != OK and not DirAccess.dir_exists_absolute(target_path):
		return _error_result("Failed to create directory: %s" % target_path)

	if not copy_directory_contents(source_path, target_path):
		return _error_result("Failed to copy directory: %s" % target_path)

	return {"success": true}

static func merge_directory(source_path: String, target_path: String) -> Dictionary:
	if not DirAccess.dir_exists_absolute(source_path):
		return _error_result("Update source directory is missing: %s" % source_path)

	if DirAccess.make_dir_recursive_absolute(target_path) != OK and not DirAccess.dir_exists_absolute(target_path):
		return _error_result("Failed to create directory: %s" % target_path)

	if not copy_directory_contents(source_path, target_path):
		return _error_result("Failed to merge directory: %s" % target_path)

	return {"success": true}

static func replace_file(source_path: String, target_path: String) -> Dictionary:
	if not FileAccess.file_exists(source_path):
		return _error_result("Update source file is missing: %s" % source_path)

	if FileAccess.file_exists(target_path):
		DirAccess.remove_absolute(target_path)

	DirAccess.make_dir_recursive_absolute(target_path.get_base_dir())
	if not copy_file(source_path, target_path):
		return _error_result("Failed to copy file: %s" % target_path)

	return {"success": true}

static func copy_directory_contents(source_path: String, target_path: String) -> bool:
	var source_dir: DirAccess = DirAccess.open(source_path)
	if source_dir == null:
		return false

	source_dir.list_dir_begin()
	var name: String = source_dir.get_next()
	while name != "":
		if name != "." and name != "..":
			var source_child: String = source_path.path_join(name)
			var target_child: String = target_path.path_join(name)
			if source_dir.current_is_dir():
				if DirAccess.make_dir_recursive_absolute(target_child) != OK and not DirAccess.dir_exists_absolute(target_child):
					source_dir.list_dir_end()
					return false
				if not copy_directory_contents(source_child, target_child):
					source_dir.list_dir_end()
					return false
			else:
				if not copy_file(source_child, target_child):
					source_dir.list_dir_end()
					return false
		name = source_dir.get_next()
	source_dir.list_dir_end()
	return true

static func copy_file(source_path: String, target_path: String) -> bool:
	var input_file: FileAccess = FileAccess.open(source_path, FileAccess.READ)
	if input_file == null:
		return false

	var data: PackedByteArray = input_file.get_buffer(input_file.get_length())
	input_file.close()

	DirAccess.make_dir_recursive_absolute(target_path.get_base_dir())
	var output_file: FileAccess = FileAccess.open(target_path, FileAccess.WRITE)
	if output_file == null:
		return false

	output_file.store_buffer(data)
	output_file.close()
	return true

static func delete_path_recursive(path: String) -> bool:
	if FileAccess.file_exists(path):
		return DirAccess.remove_absolute(path) == OK
	if not DirAccess.dir_exists_absolute(path):
		return true

	var dir: DirAccess = DirAccess.open(path)
	if dir == null:
		return false

	dir.list_dir_begin()
	var name: String = dir.get_next()
	while name != "":
		if name != "." and name != "..":
			var child_path: String = path.path_join(name)
			if dir.current_is_dir():
				if not delete_path_recursive(child_path):
					dir.list_dir_end()
					return false
			elif DirAccess.remove_absolute(child_path) != OK:
				dir.list_dir_end()
				return false
		name = dir.get_next()
	dir.list_dir_end()
	return DirAccess.remove_absolute(path) == OK

static func find_template_directory(search_root: String) -> String:
	var pending: Array[String] = [search_root]
	while not pending.is_empty():
		var current: String = pending.pop_back()
		if _looks_like_template_root(current):
			return current

		var dir: DirAccess = DirAccess.open(current)
		if dir == null:
			continue

		dir.list_dir_begin()
		var name: String = dir.get_next()
		while name != "":
			if name != "." and name != ".." and dir.current_is_dir():
				pending.append(current.path_join(name))
			name = dir.get_next()
		dir.list_dir_end()
	return ""

static func _looks_like_template_root(path: String) -> bool:
	return DirAccess.dir_exists_absolute(path.path_join("Framework")) \
		and DirAccess.dir_exists_absolute(path.path_join("addons/SetupPlugin")) \
		and FileAccess.file_exists(path.path_join("Template.csproj"))

static func _error_result(message: String) -> Dictionary:
	return {"success": false, "message": message}
