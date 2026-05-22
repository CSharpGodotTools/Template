@tool
# Event bridge that emits a signal when debugger traffic/session state changes.
# Used by Debugger+ to refresh without periodic timers.
extends EditorDebuggerPlugin

signal debugger_event(event_name: StringName, message: String)

func _has_capture(_capture: String) -> bool:
	# Observe all debugger capture channels; _capture returns false so built-in
	# debugger handlers continue processing normally.
	return true

func _capture(_message: String, _data: Array, _session_id: int) -> bool:
	debugger_event.emit(&"capture", _message)
	return false

func _setup_session(session_id: int) -> void:
	var session: EditorDebuggerSession = get_session(session_id)
	if session == null:
		return

	if not session.is_connected("started", Callable(self, "_on_session_started")):
		session.started.connect(_on_session_started)
	if not session.is_connected("stopped", Callable(self, "_on_session_stopped")):
		session.stopped.connect(_on_session_stopped)
	if not session.is_connected("continued", Callable(self, "_on_session_continued")):
		session.continued.connect(_on_session_continued)
	if not session.is_connected("breaked", Callable(self, "_on_session_breaked")):
		session.breaked.connect(_on_session_breaked)

func _on_session_started() -> void:
	debugger_event.emit(&"started", "")

func _on_session_stopped() -> void:
	debugger_event.emit(&"stopped", "")

func _on_session_continued() -> void:
	debugger_event.emit(&"continued", "")

func _on_session_breaked(_can_debug: bool) -> void:
	debugger_event.emit(&"breaked", "")
