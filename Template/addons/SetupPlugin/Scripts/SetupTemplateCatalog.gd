class_name SetupTemplateCatalog

var _templates_by_project_type: Dictionary

func _init(templates_by_project_type: Dictionary) -> void:
	_templates_by_project_type = templates_by_project_type

var project_types: Array:
	get:
		return _templates_by_project_type.keys()

func get_templates(project_type: String) -> Array:
	if _templates_by_project_type.has(project_type):
		return _templates_by_project_type[project_type]
	return []

func get_first_selection() -> Dictionary:
	for entry_key in _templates_by_project_type.keys():
		var entry_value: Array = _templates_by_project_type[entry_key]
		if not entry_value.is_empty():
			return {
				"project_type": entry_key,
				"template_type": entry_value[0]
			}
	return {}

static func try_load(main_scenes_root: String, failure_reason: String) -> SetupTemplateCatalog:
	if not DirAccess.dir_exists_absolute(main_scenes_root):
		failure_reason = "Missing setup templates directory: %s" % main_scenes_root
		return null
	
	var templates_by_project_type: Dictionary = {}
	
	var dir: DirAccess = DirAccess.open(main_scenes_root)
	if dir == null:
		failure_reason = "Failed to open main scenes directory"
		return null
	
	var project_type_directories: Array = []
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name != "." and file_name != ".." and dir.current_is_dir():
			project_type_directories.append(main_scenes_root.path_join(file_name))
		file_name = dir.get_next()
	
	project_type_directories.sort()
	
	for project_type_directory in project_type_directories:
		var project_type_name: String = project_type_directory.get_file()
		if project_type_name.is_empty():
			continue
		
		var template_dir: DirAccess = DirAccess.open(project_type_directory)
		if template_dir == null:
			continue
		
		var template_directories: Array = []
		template_dir.list_dir_begin()
		var template_file: String = template_dir.get_next()
		
		while template_file != "":
			if template_file != "." and template_file != ".." and template_dir.current_is_dir():
				template_directories.append(project_type_directory.path_join(template_file))
			template_file = template_dir.get_next()
		
		template_directories.sort()
		
		var template_types: Array = []
		for template_directory in template_directories:
			var template_name: String = template_directory.get_file()
			if not template_name.is_empty():
				template_types.append(template_name)
		
		if not template_types.is_empty():
			templates_by_project_type[project_type_name] = template_types
	
	if templates_by_project_type.is_empty():
		failure_reason = "No setup templates were discovered."
		return null
	
	failure_reason = ""
	return SetupTemplateCatalog.new(templates_by_project_type)
