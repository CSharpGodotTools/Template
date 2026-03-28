@tool
class_name DevToolsUpdateView
extends RefCounted

const DEFAULT_WARNING_TEXT: String = "Remember to back up your project [b]before[/b] updating!"
const FEEDBACK_COLOR_TEMPLATE: String = "[color=#99f299]%s[/color]"
const PRINT_PREFIX: String = "[SetupPlugin][Update] "
const PRINT_TEMPLATE: String = "%s%s"
const EMPTY_TEXT: String = ""
const BB_OPEN: String = "["
const BB_CLOSE: String = "]"
const BB_OPEN_ESCAPED: String = "\\["
const BB_CLOSE_ESCAPED: String = "\\]"

var _update_warning_label: RichTextLabel
var _update_feedback_timer: Timer
var _tracked_main_commit_label: Label
var _tracked_release_version_label: Label
var _latest_main_commit_label: Label
var _latest_release_version_label: Label
var _check_updates_on_startup_checkbox: CheckButton
var _update_from_main_button: Button
var _update_from_release_button: Button
var _check_updates_button: Button
var _reset_update_cache_button: Button

func _init(update_warning_label: RichTextLabel, update_feedback_timer: Timer, tracked_main_commit_label: Label, tracked_release_version_label: Label, latest_main_commit_label: Label, latest_release_version_label: Label, check_updates_on_startup_checkbox: CheckButton, update_from_main_button: Button, update_from_release_button: Button, check_updates_button: Button, reset_update_cache_button: Button) -> void:
	_update_warning_label = update_warning_label
	_update_feedback_timer = update_feedback_timer
	_tracked_main_commit_label = tracked_main_commit_label
	_tracked_release_version_label = tracked_release_version_label
	_latest_main_commit_label = latest_main_commit_label
	_latest_release_version_label = latest_release_version_label
	_check_updates_on_startup_checkbox = check_updates_on_startup_checkbox
	_update_from_main_button = update_from_main_button
	_update_from_release_button = update_from_release_button
	_check_updates_button = check_updates_button
	_reset_update_cache_button = reset_update_cache_button
	_apply_feedback_text(EMPTY_TEXT)

func _apply_feedback_text(text: String) -> void:
	if _update_warning_label == null:
		return
	if text.is_empty():
		_update_warning_label.text = DEFAULT_WARNING_TEXT
		return
	var safe_text: String = _escape_bbcode(text)
	_update_warning_label.text = FEEDBACK_COLOR_TEMPLATE % safe_text

func _escape_bbcode(text: String) -> String:
	return text.replace(BB_OPEN, BB_OPEN_ESCAPED).replace(BB_CLOSE, BB_CLOSE_ESCAPED)

func set_tracked_labels(main_text: String, release_text: String) -> void:
	if _tracked_main_commit_label != null:
		_tracked_main_commit_label.text = main_text
	if _tracked_release_version_label != null:
		_tracked_release_version_label.text = release_text

func set_latest_labels(main_text: String, release_text: String) -> void:
	if _latest_main_commit_label != null:
		_latest_main_commit_label.text = main_text
	if _latest_release_version_label != null:
		_latest_release_version_label.text = release_text

func set_check_updates_on_startup(pressed: bool) -> void:
	if _check_updates_on_startup_checkbox == null:
		return
	_check_updates_on_startup_checkbox.button_pressed = pressed

func set_buttons_disabled(controls_locked: bool, disable_main: bool, disable_release: bool) -> void:
	if _check_updates_button != null:
		_check_updates_button.disabled = controls_locked
	if _reset_update_cache_button != null:
		_reset_update_cache_button.disabled = controls_locked
	if _update_from_main_button != null:
		_update_from_main_button.disabled = controls_locked or disable_main
	if _update_from_release_button != null:
		_update_from_release_button.disabled = controls_locked or disable_release

func set_status(text: String) -> void:
	_apply_feedback_text(text)

func show_feedback(text: String) -> void:
	if text.is_empty():
		_apply_feedback_text(EMPTY_TEXT)
		return
	print(PRINT_TEMPLATE % [PRINT_PREFIX, text])
	_apply_feedback_text(text)
	if _update_feedback_timer != null:
		_update_feedback_timer.start()

func on_feedback_timer_timeout() -> void:
	_apply_feedback_text(EMPTY_TEXT)
