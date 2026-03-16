# Executes the one-time initial project setup when the user clicks "Run Setup".
# Copies the selected template scene to the project root, renames project files
# and .cs namespaces from "Template" to the chosen game name, configures the
# main scene, and restarts the Godot editor.
class_name ProjectSetupRunner

const SETUP_PLUGIN_NAME: String = "SetupPlugin"
const MAIN_SCENE_PATH: String = "application/run/main_scene"
const ROOT_LEVEL_SCENE_PATH: String = "res://Level.tscn"

var _project_root: String
var _main_scenes_root: String

# Stores the absolute file-system paths needed throughout the setup sequence.
func _init(project_root: String, main_scenes_root: String) -> void:
	_project_root = project_root
	_main_scenes_root = main_scenes_root

# Full setup sequence:
#   1. Remove stale empty directories
#   2. Copy the chosen template scene files to the project root
#   3. Set the main scene project setting
#   4. Rename .csproj / .sln / project.godot from "Template" to the game name
#   5. Replace the __TEMPLATE__ namespace placeholder in all .cs files
#   6. Verify the script template has the new namespace
#   7. Place .gdignore files in GdUnit4 test output folders
#   8. Save, delete the MainScenes directory, and restart the editor
func run(formatted_game_name: String, project_type: String, template_type: String) -> void:
	SetupDirectoryMaintenance.delete_empty_directories(_project_root)
	
	var template_folder: String = _main_scenes_root.path_join(project_type).path_join(template_type)
	copy_template_to_project_root(template_folder)
	
	ProjectSettings.set_setting(MAIN_SCENE_PATH, ROOT_LEVEL_SCENE_PATH)
	ProjectSettings.save()
	
	ProjectFileRenamer.rename_template_project_files(_project_root, formatted_game_name)
	NamespaceMigration.rename_template_namespaces(_project_root, formatted_game_name)
	if not SetupFileSystem.ensure_script_template_namespace_replaced(_project_root, formatted_game_name):
		push_error("Setup aborted because the script template namespace could not be verified.")
		return
	SetupFileSystem.ensure_gdignore_files_in_gdunit_test_folders(_project_root)
	
	open_root_level_scene_if_present()
	EditorInterface.save_scene()
	delete_directory_recursive(_main_scenes_root)
	EditorInterface.restart_editor(false)

# Ensures the editor is focused on the copied root Level.tscn so restart state
# does not reference the removed MainScenes template path.
func open_root_level_scene_if_present() -> void:
	if not ResourceLoader.exists(ROOT_LEVEL_SCENE_PATH):
		return

	EditorInterface.open_scene_from_path(ROOT_LEVEL_SCENE_PATH)

# Recursively copies all files from `template_folder` into the project root.
func copy_template_to_project_root(template_folder: String) -> void:
	if not DirAccess.dir_exists_absolute(template_folder):
		push_error("Template folder does not exist: %s" % template_folder)
		return
	
	var dir: DirAccess = DirAccess.open(template_folder)
	if dir == null:
		return
	
	_copy_directory_recursive(dir, template_folder, _project_root)
	
# Recursively deletes all files and subdirectories under `path`, then removes
# the directory itself.
func delete_directory_recursive(path: String) -> void:
	var dir := DirAccess.open(path)
	if dir == null:
		return
	dir.list_dir_begin()
	var name := dir.get_next()
	while name != "":
		if name != "." and name != "..":
			var child := path.path_join(name)
			if dir.current_is_dir():
				delete_directory_recursive(child)
			else:
				dir.remove(name)  # removes file
		name = dir.get_next()
		
	DirAccess.remove_absolute(path)

# Recursive helper: mirrors a source directory tree into dest_path, creating
# subdirectories as needed and copying every file byte-for-byte.
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
			# Ensure parent directory exists
			DirAccess.make_dir_absolute(dest_file.get_base_dir())

			var source_file_access := FileAccess.open(source_file, FileAccess.READ)
			if source_file_access != null:
				var file_data := source_file_access.get_buffer(source_file_access.get_length())
				source_file_access.close()

				var dest_file_access := FileAccess.open(dest_file, FileAccess.WRITE)
				if dest_file_access != null:
					dest_file_access.store_buffer(file_data)
					dest_file_access.close()

		file_name = dir.get_next()
