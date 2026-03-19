@tool
# Orchestrates the full template update pipeline:
#   download → extract → locate template root → apply → clean up temp directory.
# Called by DevToolsTab to perform the actual update work.
class_name TemplateUpdaterRuntime
extends RefCounted

const TemplateArchiveFetcherScript = preload("../Update/TemplateArchiveFetcher.gd")
const TemplateArchiveExtractorScript = preload("../Update/TemplateArchiveExtractor.gd")
const TemplateUpdateApplierScript = preload("../Update/TemplateUpdateApplier.gd")
const UpdateFileOps = preload("../Update/UpdateFileOps.gd")

# Runs the update pipeline using the latest commit on the main branch.
func update_from_main(project_root: String, host: Node, status_callback: Callable) -> Dictionary:
	return await _run_update(project_root, host, status_callback, false)

# Runs the update pipeline using the latest tagged GitHub release zip.
func update_from_release(project_root: String, host: Node, status_callback: Callable) -> Dictionary:
	return await _run_update(project_root, host, status_callback, true)

# Shared pipeline used by both update_from_main and update_from_release.
# Creates a temporary directory, performs download/extract/apply, then removes
# the temp directory regardless of success or failure.
func _run_update(project_root: String, host: Node, status_callback: Callable, from_release: bool) -> Dictionary:
	if host == null or not is_instance_valid(host):
		return _error_result("Updater host is not valid.")

	var fetcher = TemplateArchiveFetcherScript.new()
	var extractor = TemplateArchiveExtractorScript.new()
	var applier = TemplateUpdateApplierScript.new()

	var temp_root: String = project_root.path_join(".godot/setup_plugin_update_%d" % Time.get_unix_time_from_system())
	var archive_path: String = temp_root.path_join("template_update.zip")
	var extract_root: String = temp_root.path_join("extracted")
	DirAccess.make_dir_recursive_absolute(temp_root)

	_notify(status_callback, "Downloading template update...")
	var download_result: Dictionary
	if from_release:
		download_result = await fetcher.download_release_archive(host, archive_path)
	else:
		download_result = await fetcher.download_main_archive(host, archive_path)
	if not download_result.get("success", false):
		UpdateFileOps.delete_path_recursive(temp_root)
		return _error_result(download_result.get("message", "Failed to download update archive."))

	_notify(status_callback, "Extracting archive...")
	var extract_result: Dictionary = extractor.extract_zip(archive_path, extract_root)
	if not extract_result.get("success", false):
		UpdateFileOps.delete_path_recursive(temp_root)
		return _error_result(extract_result.get("message", "Failed to extract update archive."))

	var template_root: String = UpdateFileOps.find_template_directory(extract_root)
	if template_root.is_empty():
		UpdateFileOps.delete_path_recursive(temp_root)
		return _error_result("Template folder was not found in the downloaded archive.")

	_notify(status_callback, "Applying update files...")
	var apply_result: Dictionary = applier.apply(template_root, project_root, status_callback)
	UpdateFileOps.delete_path_recursive(temp_root)
	return apply_result

# Invokes the status callback with a progress message if the callable is valid.
func _notify(status_callback: Callable, message: String) -> void:
	if status_callback.is_valid():
		status_callback.call(message)

# Returns a standardised failure dictionary with the given error message.
func _error_result(message: String) -> Dictionary:
	return {"success": false, "message": message}
