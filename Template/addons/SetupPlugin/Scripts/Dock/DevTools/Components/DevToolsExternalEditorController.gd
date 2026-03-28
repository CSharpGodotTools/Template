@tool
class_name DevToolsExternalEditorController
extends RefCounted

const OS_NAME_WINDOWS: String = "Windows"
const OS_NAME_MAC: String = "macOS"
const EXTERNAL_EDITOR_VSCODE: String = "vscode"
const EXTERNAL_EDITOR_VISUAL_STUDIO: String = "visual_studio"
const EXTERNAL_EDITOR_RIDER: String = "rider"
const DEFAULT_EDITOR_KEY: String = EXTERNAL_EDITOR_VSCODE
const STATUS_OPENED: String = "Opened external editor."
const STATUS_FALLBACK: String = "Opened with system default."
const STATUS_FAILED: String = "Could not open external editor. Check your IDE install or PATH."
const ERROR_PROJECT_ROOT_EMPTY: String = "Project root path is empty."
const OPEN_EXECUTABLE: String = "open"
const OPEN_APP_FLAG: String = "-a"
const VSCODE_APP_NAME: String = "Visual Studio Code"
const VISUAL_STUDIO_APP_NAME: String = "Visual Studio"
const RIDER_APP_NAME: String = "Rider"
const JETBRAINS_RIDER_APP_NAME: String = "JetBrains Rider"
const VSCODE_EXECUTABLE: String = "code"
const VSCODE_INSIDERS_EXECUTABLE: String = "code-insiders"
const VISUAL_STUDIO_EXECUTABLE: String = "devenv"
const VISUAL_STUDIO_EXECUTABLE_EXE: String = "devenv.exe"
const VSCODE_OPEN_FLAG: String = "-r"
const VSCODE_URI_PREFIX: String = "vscode://file/"
const SPACE_CHAR: String = " "
const SPACE_ESCAPE: String = "%20"
const PROCESS_INVALID_PID: int = -1
const RIDER_WINDOWS_EXECUTABLES: Array[String] = ["rider64.exe", "rider.exe", "rider64", "rider"]
const RIDER_LINUX_EXECUTABLES: Array[String] = ["rider", "rider64", "rider.sh", "jetbrains-rider"]

var _project_root: String
var _solution_file_name: String
var _external_editor_options: OptionButton
var _status_feedback: DevToolsStatusFeedback

func _init(project_root: String, solution_file_name: String, external_editor_options: OptionButton, status_feedback: DevToolsStatusFeedback) -> void:
	_project_root = project_root
	_solution_file_name = solution_file_name
	_external_editor_options = external_editor_options
	_status_feedback = status_feedback

func _resolve_open_target() -> String:
	var solution_path: String = _project_root.path_join(_solution_file_name)
	if FileAccess.file_exists(solution_path):
		return solution_path
	return _project_root

func _selected_key() -> String:
	if _external_editor_options == null:
		return DEFAULT_EDITOR_KEY
	var index: int = _external_editor_options.selected
	var meta: Variant = _external_editor_options.get_item_metadata(index)
	if meta is String and not (meta as String).is_empty():
		return str(meta)
	return DEFAULT_EDITOR_KEY

func _open_for_selection(selection: String, open_target: String) -> bool:
	if selection == EXTERNAL_EDITOR_VSCODE:
		return _open_in_vscode(_project_root)
	if selection == EXTERNAL_EDITOR_VISUAL_STUDIO:
		return _open_in_visual_studio(open_target)
	if selection == EXTERNAL_EDITOR_RIDER:
		return _open_in_rider(open_target)
	return false

func _open_in_vscode(project_root: String) -> bool:
	var os_name: String = OS.get_name()
	if os_name == OS_NAME_MAC:
		if _try_launch(OPEN_EXECUTABLE, [OPEN_APP_FLAG, VSCODE_APP_NAME, project_root]):
			return true
	if _try_launch(VSCODE_EXECUTABLE, [VSCODE_OPEN_FLAG, project_root]):
		return true
	if _try_launch(VSCODE_INSIDERS_EXECUTABLE, [VSCODE_OPEN_FLAG, project_root]):
		return true
	var uri_path: String = project_root.replace(SPACE_CHAR, SPACE_ESCAPE)
	return _open_with_uri("%s%s" % [VSCODE_URI_PREFIX, uri_path])

func _open_in_visual_studio(solution_path: String) -> bool:
	var os_name: String = OS.get_name()
	if os_name == OS_NAME_WINDOWS:
		if _try_launch(VISUAL_STUDIO_EXECUTABLE, [solution_path]):
			return true
		if _try_launch(VISUAL_STUDIO_EXECUTABLE_EXE, [solution_path]):
			return true
	elif os_name == OS_NAME_MAC:
		if _try_launch(OPEN_EXECUTABLE, [OPEN_APP_FLAG, VISUAL_STUDIO_APP_NAME, solution_path]):
			return true
	return false

func _open_in_rider(solution_path: String) -> bool:
	var os_name: String = OS.get_name()
	if os_name == OS_NAME_WINDOWS:
		for exe in RIDER_WINDOWS_EXECUTABLES:
			if _try_launch(exe, [solution_path]):
				return true
	elif os_name == OS_NAME_MAC:
		if _try_launch(OPEN_EXECUTABLE, [OPEN_APP_FLAG, RIDER_APP_NAME, solution_path]):
			return true
		if _try_launch(OPEN_EXECUTABLE, [OPEN_APP_FLAG, JETBRAINS_RIDER_APP_NAME, solution_path]):
			return true
	else:
		for exe in RIDER_LINUX_EXECUTABLES:
			if _try_launch(exe, [solution_path]):
				return true
	return false

func _open_with_default(target_path: String) -> bool:
	return _open_with_uri(target_path)

func _open_with_uri(target_path: String) -> bool:
	return OS.shell_open(target_path) == OK

func _try_launch(executable: String, args: Array[String]) -> bool:
	var pid: int = OS.create_process(executable, args, false)
	return pid != PROCESS_INVALID_PID

func open_selected() -> void:
	if _project_root.is_empty():
		push_error(ERROR_PROJECT_ROOT_EMPTY)
		_status_feedback.show(ERROR_PROJECT_ROOT_EMPTY)
		return

	var open_target: String = _resolve_open_target()
	var selection: String = _selected_key()
	var opened: bool = _open_for_selection(selection, open_target)
	if opened:
		_status_feedback.show(STATUS_OPENED)
		return

	var fallback_opened: bool = _open_with_default(open_target)
	if fallback_opened:
		_status_feedback.show(STATUS_FALLBACK)
		return

	push_error(STATUS_FAILED)
	_status_feedback.show(STATUS_FAILED)
