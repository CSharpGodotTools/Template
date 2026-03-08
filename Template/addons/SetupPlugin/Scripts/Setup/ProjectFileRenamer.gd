class_name ProjectFileRenamer

const TEMPLATE_PROJECT_NAME: String = "Template"

static func rename_template_project_files(project_root: String, project_name: String) -> void:
	rename_csproj_file(project_root, project_name)
	rename_solution_file(project_root, project_name)
	rename_project_godot_file(project_root, project_name)

static func rename_project_godot_file(project_root: String, project_name: String) -> void:
	var project_file_path: String = project_root.path_join("project.godot")
	require_file(project_file_path)
	
	var project_text: String = FileAccess.get_file_as_string(project_file_path)
	
	project_text = project_text.replace(
		"project/assembly_name=\"%s\"" % TEMPLATE_PROJECT_NAME,
		"project/assembly_name=\"%s\"" % project_name)
	
	project_text = project_text.replace(
		"config/name=\"%s\"" % TEMPLATE_PROJECT_NAME,
		"config/name=\"%s\"" % project_name)
	
	var file: FileAccess = FileAccess.open(project_file_path, FileAccess.WRITE)
	if file != null:
		file.store_string(project_text)

static func rename_solution_file(project_root: String, project_name: String) -> void:
	var solution_file_path: String = project_root.path_join("%s.sln" % TEMPLATE_PROJECT_NAME)
	require_file(solution_file_path)
	
	var solution_text: String = FileAccess.get_file_as_string(solution_file_path)
	solution_text = solution_text.replace(TEMPLATE_PROJECT_NAME, project_name)
	
	DirAccess.remove_absolute(solution_file_path)
	var new_file: FileAccess = FileAccess.open(project_root.path_join(project_name + ".sln"), FileAccess.WRITE)
	if new_file != null:
		new_file.store_string(solution_text)

static func rename_csproj_file(project_root: String, project_name: String) -> void:
	var csproj_file_path: String = project_root.path_join("%s.csproj" % TEMPLATE_PROJECT_NAME)
	require_file(csproj_file_path)
	
	var csproj_text: String = FileAccess.get_file_as_string(csproj_file_path)
	csproj_text = csproj_text.replace(
		"<RootNamespace>%s</RootNamespace>" % TEMPLATE_PROJECT_NAME,
		"<RootNamespace>%s</RootNamespace>" % project_name)
	
	DirAccess.remove_absolute(csproj_file_path)
	var new_file: FileAccess = FileAccess.open(project_root.path_join(project_name + ".csproj"), FileAccess.WRITE)
	if new_file != null:
		new_file.store_string(csproj_text)

static func require_file(path: String) -> void:
	if FileAccess.file_exists(path):
		return
	
	push_error("Missing setup artifact: %s" % path)
