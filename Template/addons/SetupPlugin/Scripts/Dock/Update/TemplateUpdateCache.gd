# Persists tracked update metadata (main commit + release version) between
# editor sessions so the update tab can detect when updates are unnecessary.
class_name TemplateUpdateCache
extends RefCounted

const CACHE_PATH: String = "user://setup_plugin_update_cache.cfg"
const SECTION_TRACKED: String = "tracked"
const KEY_MAIN_COMMIT: String = "main_commit"
const KEY_RELEASE_VERSION: String = "release_version"

# Loads tracked values from disk. Missing cache values are returned as empty strings.
static func load_state() -> Dictionary:
	var config: ConfigFile = ConfigFile.new()
	var load_result: Error = config.load(CACHE_PATH)
	if load_result != OK and load_result != ERR_FILE_NOT_FOUND:
		return _empty_state()

	return {
		KEY_MAIN_COMMIT: str(config.get_value(SECTION_TRACKED, KEY_MAIN_COMMIT, "")).strip_edges(),
		KEY_RELEASE_VERSION: str(config.get_value(SECTION_TRACKED, KEY_RELEASE_VERSION, "")).strip_edges()
	}

# Saves the provided tracked values to disk.
static func save_state(main_commit: String, release_version: String) -> bool:
	var config: ConfigFile = ConfigFile.new()
	var load_result: Error = config.load(CACHE_PATH)
	if load_result != OK and load_result != ERR_FILE_NOT_FOUND:
		return false

	config.set_value(SECTION_TRACKED, KEY_MAIN_COMMIT, main_commit.strip_edges())
	config.set_value(SECTION_TRACKED, KEY_RELEASE_VERSION, release_version.strip_edges())
	return config.save(CACHE_PATH) == OK

# Clears tracked values while keeping the cache file valid.
static func clear_state() -> bool:
	return save_state("", "")

static func _empty_state() -> Dictionary:
	return {
		KEY_MAIN_COMMIT: "",
		KEY_RELEASE_VERSION: ""
	}
