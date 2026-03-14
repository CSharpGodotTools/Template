@tool
class_name TemplateArchiveFetcher
extends RefCounted

const RELEASE_API_URL: String = "https://api.github.com/repos/CSharpGodotTools/Template/releases/latest"

func download_main_archive(host: Node, destination_path: String) -> Dictionary:
	return await _download_file(host, "https://codeload.github.com/CSharpGodotTools/Template/zip/refs/heads/main", destination_path)

func download_release_archive(host: Node, destination_path: String) -> Dictionary:
	var result: Dictionary = await _download_file(host, "https://github.com/CSharpGodotTools/Template/releases/latest/download/Template.zip", destination_path)
	if result.get("success", false):
		return result

	var resolved_url: String = await _resolve_latest_release_zip_url(host)
	if resolved_url.is_empty():
		return result

	return await _download_file(host, resolved_url, destination_path)

func _download_file(host: Node, url: String, destination_path: String) -> Dictionary:
	var request: HTTPRequest = HTTPRequest.new()
	request.timeout = 120
	request.download_file = destination_path
	host.add_child(request)

	var headers: PackedStringArray = [
		"User-Agent: SetupPluginUpdater",
		"Accept: application/octet-stream"
	]
	var start_error: Error = request.request(url, headers, HTTPClient.METHOD_GET)
	if start_error != OK:
		request.queue_free()
		return _error_result("Failed to start download (%d)." % start_error)

	var result: Array = await request.request_completed
	request.queue_free()

	var request_result: int = result[0]
	var response_code: int = result[1]
	if request_result != HTTPRequest.RESULT_SUCCESS:
		if FileAccess.file_exists(destination_path):
			DirAccess.remove_absolute(destination_path)
		return _error_result("Download failed (result=%d, code=%d)." % [request_result, response_code])

	if response_code < 200 or response_code >= 300:
		if FileAccess.file_exists(destination_path):
			DirAccess.remove_absolute(destination_path)
		return _error_result("Download failed with HTTP code %d." % response_code)

	if not FileAccess.file_exists(destination_path):
		return _error_result("Download finished but archive file was not written.")

	return {"success": true}

func _resolve_latest_release_zip_url(host: Node) -> String:
	var response: Dictionary = await _request_body(host, RELEASE_API_URL, "application/vnd.github+json")
	if not response.get("success", false):
		return ""

	var payload: Variant = JSON.parse_string(response.get("body", ""))
	if payload == null or not (payload is Dictionary):
		return ""

	var release_data: Dictionary = payload as Dictionary
	if not release_data.has("assets") or not (release_data["assets"] is Array):
		return ""

	for asset_variant in release_data["assets"]:
		if asset_variant is Dictionary:
			var url: String = str((asset_variant as Dictionary).get("browser_download_url", ""))
			if url.ends_with(".zip") and url.contains("Template"):
				return url

	for asset_variant in release_data["assets"]:
		if asset_variant is Dictionary:
			var fallback_url: String = str((asset_variant as Dictionary).get("browser_download_url", ""))
			if fallback_url.ends_with(".zip"):
				return fallback_url

	return ""

func _request_body(host: Node, url: String, accept_header: String) -> Dictionary:
	var request: HTTPRequest = HTTPRequest.new()
	request.timeout = 60
	host.add_child(request)

	var start_error: Error = request.request(url, ["User-Agent: SetupPluginUpdater", "Accept: %s" % accept_header], HTTPClient.METHOD_GET)
	if start_error != OK:
		request.queue_free()
		return _error_result("Failed to request %s (%d)." % [url, start_error])

	var result: Array = await request.request_completed
	request.queue_free()

	var request_result: int = result[0]
	var response_code: int = result[1]
	if request_result != HTTPRequest.RESULT_SUCCESS or response_code < 200 or response_code >= 300:
		return _error_result("Request failed for %s (result=%d, code=%d)." % [url, request_result, response_code])

	return {"success": true, "body": (result[3] as PackedByteArray).get_string_from_utf8()}

func _error_result(message: String) -> Dictionary:
	return {"success": false, "message": message}
