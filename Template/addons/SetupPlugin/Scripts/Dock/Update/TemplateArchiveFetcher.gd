@tool
# Downloads the Template repository archive from GitHub.
# Supports two sources:
#   - Main branch: a live zip of the current HEAD commit
#   - Latest release: a tagged release zip, with GitHub API fallback if the
#     direct URL is not available
class_name TemplateArchiveFetcher
extends RefCounted

const MAIN_COMMIT_API_URL: String = "https://api.github.com/repos/CSharpGodotTools/Template/commits/main"
const RELEASE_API_URL: String = "https://api.github.com/repos/CSharpGodotTools/Template/releases/latest"

# Fetches the latest commit SHA on the main branch.
func fetch_latest_main_commit(host: Node) -> Dictionary:
	var response: Dictionary = await _request_body(host, MAIN_COMMIT_API_URL, "application/vnd.github+json")
	if not response.get("success", false):
		return response

	var payload: Variant = JSON.parse_string(response.get("body", ""))
	if payload == null or not (payload is Dictionary):
		return _error_result("Failed to parse latest main-branch commit response.")

	var sha: String = str((payload as Dictionary).get("sha", "")).strip_edges()
	if sha.is_empty():
		return _error_result("Latest main-branch commit id was not found.")

	return {
		"success": true,
		"commit": sha.substr(0, min(7, sha.length())),
		"full_commit": sha
	}

# Fetches the latest GitHub release version/tag.
func fetch_latest_release_version(host: Node) -> Dictionary:
	var release_result: Dictionary = await _request_latest_release_data(host)
	if not release_result.get("success", false):
		return release_result

	var release_data: Dictionary = release_result.get("release_data", {})
	var version: String = str(release_data.get("tag_name", "")).strip_edges()
	if version.is_empty():
		version = str(release_data.get("name", "")).strip_edges()
	if version.is_empty():
		return _error_result("Latest release version was not found.")

	return {"success": true, "version": version}

# Downloads the current state of the main branch as a .zip to `destination_path`.
func download_main_archive(host: Node, destination_path: String) -> Dictionary:
	return await _download_file(host, "https://codeload.github.com/CSharpGodotTools/Template/zip/refs/heads/main", destination_path)

# Downloads the latest tagged release .zip.
# Tries the well-known direct URL first; on failure, resolves the URL via the
# GitHub Releases REST API.
func download_release_archive(host: Node, destination_path: String) -> Dictionary:
	var result: Dictionary = await _download_file(host, "https://github.com/CSharpGodotTools/Template/releases/latest/download/Template.zip", destination_path)
	if result.get("success", false):
		return result

	var resolved_url: String = await _resolve_latest_release_zip_url(host)
	if resolved_url.is_empty():
		return result

	return await _download_file(host, resolved_url, destination_path)

# Performs an HTTP GET download to `destination_path` using an HTTPRequest node
# parented to `host` (needed for the signal/await mechanism).
# Cleans up the partial download file on any error.
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

# Queries the GitHub Releases API to find the first .zip asset URL in the
# latest release.  Prefers assets whose name contains "Template".
func _resolve_latest_release_zip_url(host: Node) -> String:
	var release_result: Dictionary = await _request_latest_release_data(host)
	if not release_result.get("success", false):
		return ""

	var release_data: Dictionary = release_result.get("release_data", {})
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

# Requests and parses the latest release payload from GitHub.
func _request_latest_release_data(host: Node) -> Dictionary:
	var response: Dictionary = await _request_body(host, RELEASE_API_URL, "application/vnd.github+json")
	if not response.get("success", false):
		return response

	var payload: Variant = JSON.parse_string(response.get("body", ""))
	if payload == null or not (payload is Dictionary):
		return _error_result("Failed to parse latest release metadata response.")

	return {"success": true, "release_data": payload as Dictionary}

# Sends an HTTP GET request and returns the response body as a UTF-8 string
# on success, or a failure dictionary on error.
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

# Returns a standardised failure dictionary with the given error message.
func _error_result(message: String) -> Dictionary:
	return {"success": false, "message": message}
