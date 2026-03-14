@tool
# Thin wrapper around EditorInterface for scene-level editor actions.
extends RefCounted

# Closes every currently open scene tab one by one.
# Returns the number of scenes that were closed.
func close_all_open_scenes() -> int:
	var closed_count: int = 0
	while true:
		var result: Error = EditorInterface.close_scene()
		if result != OK:
			break
		closed_count += 1
	return closed_count

# Saves the current scene (when save_before_restart is true) then restarts
# the Godot editor process.
func restart_editor(save_before_restart: bool = true) -> void:
	EditorInterface.restart_editor(save_before_restart)