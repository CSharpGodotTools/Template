class_name ProjectSetupRunner

const SETUP_PLUGIN_NAME: String = "SetupPlugin"
const MAIN_SCENE_PATH: String = "application/run/main_scene"

var _project_root: String
var _main_scenes_root: String

func _init(project_root: String, main_scenes_root: String) -> void:
	_project_root = project_root
	_main_scenes_root = main_scenes_root

func run(formatted_game_name: String, project_type: String, template_type: String) -> void:
	SetupDirectoryMaintenance.delete_empty_directories(_project_root)
	
	var template_folder: String = _main_scenes_root.path_join(project_type).path_join(template_type)
	copy_template_to_project_root(template_folder)
	
	ProjectSettings.set_setting(MAIN_SCENE_PATH, "res://Level.tscn")
	ProjectSettings.save()
	
	ProjectFileRenamer.rename_template_project_files(_project_root, formatted_game_name)
	NamespaceMigration.rename_template_namespaces(_project_root, formatted_game_name)
	SetupFileSystem.ensure_gdignore_files_in_gdunit_test_folders(_project_root)
	
	EditorInterface.save_scene()
	disable_and_delete_setup_plugin()
	EditorInterface.restart_editor(false)

func copy_template_to_project_root(template_folder: String) -> void:
	if not DirAccess.dir_exists_absolute(template_folder):
		push_error("Template folder does not exist: %s" % template_folder)
		return
	
	var dir: DirAccess = DirAccess.open(template_folder)
	if dir == null:
		return
	
	_copy_directory_recursive(dir, template_folder, _project_root)

func _copy_directory_recursive(dir: DirAccess, source_path: String, dest_path: String) -> void:
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name == "." or file_name == "..":
			file_name = dir.get_next()
			continue
		
		var source_file: String = source_path.path_join(file_name)
		var dest_file: String = dest_path.path_join(file_name)
		
		if dir.current_is_dir():
			DirAccess.make_dir_absolute(dest_file)
			var sub_dir: DirAccess = DirAccess.open(source_file)
			if sub_dir != null:
				_copy_directory_recursive(sub_dir, source_file, dest_file)
		else:
			var dest_dir: String = dest_file.get_basename()
			DirAccess.make_dir_absolute(dest_dir)
			if FileAccess.file_exists(source_file):
				var source_content: String = FileAccess.get_file_as_string(source_file)
				var file: FileAccess = FileAccess.open(dest_file, FileAccess.WRITE)
				if file != null:
					file.store_string(source_content)
		
		file_name = dir.get_next()

func disable_and_delete_setup_plugin() -> void:
	EditorInterface.set_plugin_enabled(SETUP_PLUGIN_NAME, false)
	
	var setup_plugin_path: String = _project_root.path_join("addons").path_join(SETUP_PLUGIN_NAME)
	if not DirAccess.dir_exists_absolute(setup_plugin_path):
		return
	
	DirAccess.remove_absolute(setup_plugin_path)
