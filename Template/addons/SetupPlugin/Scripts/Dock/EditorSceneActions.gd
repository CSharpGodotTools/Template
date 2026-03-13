@tool
class_name EditorSceneActions
extends RefCounted

func close_all_open_scenes() -> int:
    var closed_count: int = 0
    while true:
        var result: Error = EditorInterface.close_scene()
        if result != OK:
            break
        closed_count += 1
    return closed_count

func restart_editor(save_before_restart: bool = true) -> void:
    EditorInterface.restart_editor(save_before_restart)
