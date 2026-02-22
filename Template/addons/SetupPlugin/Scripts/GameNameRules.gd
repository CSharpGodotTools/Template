class_name GameNameRules

const RESERVED_RAW_TEMPLATE_NAMESPACE: String = "__TEMPLATE__"

static func try_validate_for_setup(game_name: String, validation_error: String) -> bool:
	var formatted_game_name: String = format_game_name(game_name)
	
	if formatted_game_name.is_empty():
		validation_error = "The game name cannot be empty."
		return false
	
	if formatted_game_name.to_upper() == RESERVED_RAW_TEMPLATE_NAMESPACE.to_upper():
		validation_error = "%s is a reserved name." % RESERVED_RAW_TEMPLATE_NAMESPACE
		return false
	
	if equals_existing_class_name(formatted_game_name):
		validation_error = "Namespace %s is the same name as %s.cs" % [formatted_game_name, formatted_game_name]
		return false
	
	validation_error = ""
	return true

static func format_game_name(name: String) -> String:
	if name.is_empty():
		return ""
	
	var trimmed_name: String = name.strip_edges().replace(" ", "")
	if trimmed_name.is_empty():
		return ""
	
	var first_character: String = trimmed_name.substr(0, 1).to_upper()
	if trimmed_name.length() == 1:
		return first_character
	
	return first_character + trimmed_name.substr(1)

static func is_alpha_numeric_and_allow_spaces(value: String) -> bool:
	if value.is_empty():
		return false
	
	for character in value:
		if StringUtils.is_alphanumeric(character) or character == ' ':
			continue
		
		return false
	
	return true
	
func is_alphanumeric(text: String) -> bool:
	var regex = RegEx.new()
	regex.compile("^[a-zA-Z0-9]+$")
	return regex.search(text) != null

static func equals_existing_class_name(name: String) -> bool:
	var project_root: String = ProjectSettings.globalize_path("res://")
	var dir: DirAccess = DirAccess.open(project_root)
	
	if dir == null:
		return false
	
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".cs"):
			if file_name.to_upper() == (name + ".cs").to_upper():
				return true
		
		if dir.current_is_dir():
			var sub_dir: DirAccess = DirAccess.open(project_root.path_join(file_name))
			if sub_dir != null:
				if _check_directory_recursive(sub_dir, name, project_root.path_join(file_name)):
					return true
		
		file_name = dir.get_next()
	
	return false

static func _check_directory_recursive(dir: DirAccess, name: String, path: String) -> bool:
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".cs"):
			if file_name.to_upper() == (name + ".cs").to_upper():
				return true
		
		if dir.current_is_dir() and file_name != "." and file_name != "..":
			var sub_dir: DirAccess = DirAccess.open(path.path_join(file_name))
			if sub_dir != null:
				if _check_directory_recursive(sub_dir, name, path.path_join(file_name)):
					return true
		
		file_name = dir.get_next()
	
	return false
