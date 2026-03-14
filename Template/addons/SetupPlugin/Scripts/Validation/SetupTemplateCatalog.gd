# Represents the available setup template options discovered from MainScenes/.
# Provides a two-level catalogue: project types (e.g. "2D", "3D") mapped to
# their template variant names (e.g. "Empty", "Platformer").
class_name SetupTemplateCatalog

var _templates_by_project_type: Dictionary

# Stores the pre-built project-type → template-names mapping.
func _init(templates_by_project_type: Dictionary) -> void:
	_templates_by_project_type = templates_by_project_type

# Returns an Array of all available project type name strings.
var project_types: Array:
	get:
		return _templates_by_project_type.keys()

# Returns the list of template variant names for the given project type.
# Returns an empty array if the type is not in the catalogue.
func get_templates(project_type: String) -> Array:
	if _templates_by_project_type.has(project_type):
		return _templates_by_project_type[project_type]
	return []

# Returns a {"project_type": ..., "template_type": ...} dict for the first
# available option in the catalogue.  Returns an empty dict if none exist.
func get_first_selection() -> Dictionary:
	for entry_key in _templates_by_project_type.keys():
		var entry_value: Array = _templates_by_project_type[entry_key]
		if not entry_value.is_empty():
			return {
				"project_type": entry_key,
				"template_type": entry_value[0]
			}
	return {}

# Scans the MainScenes directory structure to build the catalogue.
# Each subdirectory of MainScenes is a project type; each of its subdirectories
# is a template variant.  Returns null and sets failure_reason on error.
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
