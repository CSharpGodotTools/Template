@tool
# Top-level logic for the Dev Tools dock tab.
# Coordinates UI wiring and delegates actions to component controllers.
class_name DevToolsTab
extends "res://addons/SetupPlugin/Scripts/Dock/DevToolsTabLayout.gd"

const PROJECT_ROOT_PATH: String = "res://"
const SOLUTION_FILE_NAME: String = "Template.sln"
const SIGNAL_PRESSED: StringName = &"pressed"
const SIGNAL_TOGGLED: StringName = &"toggled"
const SIGNAL_COLOR_CHANGED: StringName = &"color_changed"
const SIGNAL_ITEM_SELECTED: StringName = &"item_selected"
const SIGNAL_TIMEOUT: StringName = &"timeout"
const DEFERRED_CHECK_METHOD: StringName = &"_check_updates_on_startup"
const DevToolsStatusFeedbackScript = preload("DevTools/Components/DevToolsStatusFeedback.gd")
const DevToolsExternalEditorControllerScript = preload("DevTools/Components/DevToolsExternalEditorController.gd")
const DevToolsProjectToolsControllerScript = preload("DevTools/Components/DevToolsProjectToolsController.gd")
const DevToolsSceneControllerScript = preload("DevTools/Components/DevToolsSceneController.gd")
const DevToolsRenderingControllerScript = preload("DevTools/Components/DevToolsRenderingController.gd")
const DevToolsUpdateViewScript = preload("DevTools/Components/DevToolsUpdateView.gd")
const DevToolsUpdateRunnerScript = preload("DevTools/Components/DevToolsUpdateRunner.gd")
const DevToolsUpdateCoordinatorScript = preload("DevTools/Components/DevToolsUpdateCoordinator.gd")
const DevToolsUpdateStateScript = preload("DevTools/Components/DevToolsUpdateState.gd")
const DevToolsUpdateMetadataServiceScript = preload("DevTools/Components/DevToolsUpdateMetadataService.gd")
const DevToolsUidCleanupScript = preload("DevToolsUidCleanup.gd")
const EditorSceneActionsScript = preload("EditorSceneActions.gd")
const NullableProjectSettingsScript = preload("NullableProjectSettings.gd")
const SceneHierarchyActionsScript = preload("SceneHierarchyActions.gd")
const SetupDirectoryMaintenanceScript = preload("../Setup/SetupDirectoryMaintenance.gd")
const TemplateArchiveFetcherScript = preload("Update/TemplateArchiveFetcher.gd")
const TemplateArchiveExtractorScript = preload("Update/TemplateArchiveExtractor.gd")
const TemplateUpdateApplierScript = preload("Update/TemplateUpdateApplier.gd")
const TemplateUpdateCacheScript = preload("Update/TemplateUpdateCache.gd")
const UpdateFileOpsScript = preload("Update/UpdateFileOps.gd")

var _events_registered: bool = false
var _editor_scene_actions: EditorSceneActions
var _scene_hierarchy_actions: SceneHierarchyActions
var _status_feedback: DevToolsStatusFeedback
var _external_editor_controller: DevToolsExternalEditorController
var _project_tools_controller: DevToolsProjectToolsController
var _scene_controller: DevToolsSceneController
var _rendering_controller: DevToolsRenderingController
var _update_view: DevToolsUpdateView
var _update_runner: DevToolsUpdateRunner
var _update_coordinator: DevToolsUpdateCoordinator

# Initialises service objects, builds the UI, then wires all button signals.
func _ready() -> void:
	_editor_scene_actions = EditorSceneActionsScript.new()
	_scene_hierarchy_actions = SceneHierarchyActionsScript.new()
	_create_controls()
	_build_layout()
	_initialize_components()
	_register_events()
	_update_coordinator.initialize_state()
	if _update_coordinator.should_check_updates_on_startup():
		call_deferred(DEFERRED_CHECK_METHOD)

# Returns the absolute file-system path for the project root.
func _project_root() -> String:
	return ProjectSettings.globalize_path(PROJECT_ROOT_PATH)

func _initialize_components() -> void:
	var project_root: String = _project_root()
	_status_feedback = DevToolsStatusFeedbackScript.new(_status_label, _feedback_timer)
	_external_editor_controller = DevToolsExternalEditorControllerScript.new(project_root, SOLUTION_FILE_NAME, _external_editor_options, _status_feedback)
	_project_tools_controller = DevToolsProjectToolsControllerScript.new(project_root, _status_feedback, _cleanup_uids_button, _remove_empty_folders_button, _nullable_button, DevToolsUidCleanupScript, SetupDirectoryMaintenanceScript, NullableProjectSettingsScript)
	_scene_controller = DevToolsSceneControllerScript.new(_editor_scene_actions, _scene_hierarchy_actions, _status_feedback)
	_rendering_controller = DevToolsRenderingControllerScript.new(_status_feedback)
	_update_view = DevToolsUpdateViewScript.new(_update_warning_label, _update_feedback_timer, _tracked_main_commit_label, _tracked_release_version_label, _latest_main_commit_label, _latest_release_version_label, _check_updates_on_startup_checkbox, _update_from_main_button, _update_from_release_button, _check_updates_button, _reset_update_cache_button)
	_update_runner = DevToolsUpdateRunnerScript.new(self, project_root, _update_view, TemplateArchiveFetcherScript, TemplateArchiveExtractorScript, TemplateUpdateApplierScript, UpdateFileOpsScript)
	var update_state: DevToolsUpdateState = DevToolsUpdateStateScript.new(TemplateUpdateCacheScript)
	var update_metadata_service: DevToolsUpdateMetadataService = DevToolsUpdateMetadataServiceScript.new(self, TemplateArchiveFetcherScript)
	_update_coordinator = DevToolsUpdateCoordinatorScript.new(_update_view, _update_runner, update_state, update_metadata_service)

func _check_updates_on_startup() -> void:
	await _update_coordinator.check_for_updates(false)

# Connects every button pressed signal to its handler method.
# The guard prevents double-wiring on plugin hot-reload.
func _register_events() -> void:
	if _events_registered:
		return
	_connect_signal(_open_external_editor_button, SIGNAL_PRESSED, Callable(_external_editor_controller, "open_selected"))
	_connect_signal(_cleanup_uids_button, SIGNAL_PRESSED, Callable(_project_tools_controller, "cleanup_uids"))
	_connect_signal(_remove_empty_folders_button, SIGNAL_PRESSED, Callable(_project_tools_controller, "remove_empty_folders"))
	_connect_signal(_nullable_button, SIGNAL_PRESSED, Callable(_project_tools_controller, "toggle_nullable"))
	_connect_signal(_close_all_scene_tabs_button, SIGNAL_PRESSED, Callable(_scene_controller, "close_all_scene_tabs"))
	_connect_signal(_restart_editor_button, SIGNAL_PRESSED, Callable(_scene_controller, "restart_editor"))
	_connect_signal(_expand_to_level_button, SIGNAL_PRESSED, Callable(self, "_on_expand_to_level_pressed"))
	_connect_signal(_fully_expand_button, SIGNAL_PRESSED, Callable(_scene_controller, "fully_expand"))
	_connect_signal(_fully_collapse_button, SIGNAL_PRESSED, Callable(_scene_controller, "fully_collapse"))
	_connect_signal(_update_from_main_button, SIGNAL_PRESSED, Callable(_update_coordinator, "update_from_main"))
	_connect_signal(_update_from_release_button, SIGNAL_PRESSED, Callable(_update_coordinator, "update_from_release"))
	_connect_signal(_check_updates_button, SIGNAL_PRESSED, Callable(_update_coordinator, "check_for_updates_pressed"))
	_connect_signal(_reset_update_cache_button, SIGNAL_PRESSED, Callable(_update_coordinator, "reset_update_cache"))
	_connect_signal(_view_template_repo_button, SIGNAL_PRESSED, Callable(_update_coordinator, "open_template_repo"))
	_connect_signal(_link_to_commits_button, SIGNAL_PRESSED, Callable(_update_coordinator, "open_commits"))
	_connect_signal(_link_to_release_notes_button, SIGNAL_PRESSED, Callable(_update_coordinator, "open_release_notes"))
	_connect_signal(_check_updates_on_startup_checkbox, SIGNAL_TOGGLED, Callable(_update_coordinator, "set_check_updates_on_startup"))
	_connect_signal(_clear_color_picker, SIGNAL_COLOR_CHANGED, Callable(_rendering_controller, "set_clear_color"))
	_connect_signal(_anti_aliasing_options, SIGNAL_ITEM_SELECTED, Callable(_rendering_controller, "set_anti_aliasing"))
	_connect_signal(_feedback_timer, SIGNAL_TIMEOUT, Callable(_status_feedback, "on_timer_timeout"))
	_connect_signal(_update_feedback_timer, SIGNAL_TIMEOUT, Callable(_update_view, "on_feedback_timer_timeout"))
	_events_registered = true

# Disconnects all signals. Called before the dock node is freed.
func _unregister_events() -> void:
	if not _events_registered:
		return
	_events_registered = false
	_disconnect_signal(_open_external_editor_button, SIGNAL_PRESSED, Callable(_external_editor_controller, "open_selected"))
	_disconnect_signal(_cleanup_uids_button, SIGNAL_PRESSED, Callable(_project_tools_controller, "cleanup_uids"))
	_disconnect_signal(_remove_empty_folders_button, SIGNAL_PRESSED, Callable(_project_tools_controller, "remove_empty_folders"))
	_disconnect_signal(_nullable_button, SIGNAL_PRESSED, Callable(_project_tools_controller, "toggle_nullable"))
	_disconnect_signal(_close_all_scene_tabs_button, SIGNAL_PRESSED, Callable(_scene_controller, "close_all_scene_tabs"))
	_disconnect_signal(_restart_editor_button, SIGNAL_PRESSED, Callable(_scene_controller, "restart_editor"))
	_disconnect_signal(_expand_to_level_button, SIGNAL_PRESSED, Callable(self, "_on_expand_to_level_pressed"))
	_disconnect_signal(_fully_expand_button, SIGNAL_PRESSED, Callable(_scene_controller, "fully_expand"))
	_disconnect_signal(_fully_collapse_button, SIGNAL_PRESSED, Callable(_scene_controller, "fully_collapse"))
	_disconnect_signal(_update_from_main_button, SIGNAL_PRESSED, Callable(_update_coordinator, "update_from_main"))
	_disconnect_signal(_update_from_release_button, SIGNAL_PRESSED, Callable(_update_coordinator, "update_from_release"))
	_disconnect_signal(_check_updates_button, SIGNAL_PRESSED, Callable(_update_coordinator, "check_for_updates_pressed"))
	_disconnect_signal(_reset_update_cache_button, SIGNAL_PRESSED, Callable(_update_coordinator, "reset_update_cache"))
	_disconnect_signal(_view_template_repo_button, SIGNAL_PRESSED, Callable(_update_coordinator, "open_template_repo"))
	_disconnect_signal(_link_to_commits_button, SIGNAL_PRESSED, Callable(_update_coordinator, "open_commits"))
	_disconnect_signal(_link_to_release_notes_button, SIGNAL_PRESSED, Callable(_update_coordinator, "open_release_notes"))
	_disconnect_signal(_check_updates_on_startup_checkbox, SIGNAL_TOGGLED, Callable(_update_coordinator, "set_check_updates_on_startup"))
	_disconnect_signal(_clear_color_picker, SIGNAL_COLOR_CHANGED, Callable(_rendering_controller, "set_clear_color"))
	_disconnect_signal(_anti_aliasing_options, SIGNAL_ITEM_SELECTED, Callable(_rendering_controller, "set_anti_aliasing"))
	_disconnect_signal(_feedback_timer, SIGNAL_TIMEOUT, Callable(_status_feedback, "on_timer_timeout"))
	_disconnect_signal(_update_feedback_timer, SIGNAL_TIMEOUT, Callable(_update_view, "on_feedback_timer_timeout"))

func _on_expand_to_level_pressed() -> void:
	var level: int = int(_hierarchy_level_spinbox.value)
	_scene_controller.expand_to_level(level)

func _connect_signal(source: Object, signal_name: StringName, handler: Callable) -> void:
	if source == null:
		return
	if source.is_connected(signal_name, handler):
		return
	source.connect(signal_name, handler)

func _disconnect_signal(source: Object, signal_name: StringName, handler: Callable) -> void:
	if source != null and source.is_connected(signal_name, handler):
		source.disconnect(signal_name, handler)

# Disconnects all signals before the dock node is freed.
func prepare_for_disable() -> void:
	_unregister_events()
