class_name SetupFileSystem

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
