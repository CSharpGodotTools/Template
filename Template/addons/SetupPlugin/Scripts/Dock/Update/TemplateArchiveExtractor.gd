@tool
# Extracts a .zip archive to a destination directory using Godot's built-in
# ZIPReader.  Creates subdirectories as needed.
class_name TemplateArchiveExtractor
extends RefCounted

# Opens `zip_path` and writes every entry to `destination_root`, creating
# parent directories on the fly.  Returns a success/failure dictionary.
func extract_zip(zip_path: String, destination_root: String) -> Dictionary:
	DirAccess.make_dir_recursive_absolute(destination_root)

	var zip: ZIPReader = ZIPReader.new()
	var open_error: Error = zip.open(zip_path)
	if open_error != OK:
		return _error_result("Failed to open archive (%d)." % open_error)

	for entry in zip.get_files():
		var output_path: String = destination_root.path_join(entry)
		if entry.ends_with("/"):
			DirAccess.make_dir_recursive_absolute(output_path)
			continue

		DirAccess.make_dir_recursive_absolute(output_path.get_base_dir())
		var output_file: FileAccess = FileAccess.open(output_path, FileAccess.WRITE)
		if output_file == null:
			zip.close()
			return _error_result("Failed to write extracted file: %s" % output_path)

		output_file.store_buffer(zip.read_file(entry))
		output_file.close()

	zip.close()
	return {"success": true}

# Returns a standardised failure dictionary with the given error message.
func _error_result(message: String) -> Dictionary:
	return {"success": false, "message": message}
