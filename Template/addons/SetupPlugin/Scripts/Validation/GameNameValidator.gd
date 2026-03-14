# Drives real-time game name validation for the Setup dock.
# Updates the preview label on every keystroke, rejects invalid characters or
# a leading digit, and uses a short timer to briefly show the error before
# reverting to the last valid display name.
class_name GameNameValidator

const FEEDBACK_RESET_TIME: float = 2.0

var _game_name_preview: Label
var _feedback_reset_timer: Timer
var _project_name_edit: LineEdit

var _previous_valid_game_name: String = ""

# Stores references to the three UI controls used for live validation feedback.
func _init(game_name_preview: Label, feedback_reset_timer: Timer, project_name_edit: LineEdit) -> void:
	_game_name_preview = game_name_preview
	_feedback_reset_timer = feedback_reset_timer
	_project_name_edit = project_name_edit

# Called on every text_changed signal from the project name LineEdit.
# Rejects names that start with a digit or contain special characters.
# Updates the preview label with the formatted name for valid input.
func validate(game_name: String) -> void:
	_feedback_reset_timer.stop()
	
	if game_name.is_empty():
		_game_name_preview.text = ""
		_previous_valid_game_name = ""
		return
	
	var trimmed: String = game_name.strip_edges()
	if trimmed[0].is_valid_int():
		_game_name_preview.text = "The first character cannot be a number"
		_feedback_reset_timer.start(FEEDBACK_RESET_TIME)
		reset_name_edit()
		return
	
	if not GameNameRules.is_alpha_numeric_and_allow_spaces(game_name):
		_game_name_preview.text = "Special characters are not allowed"
		_feedback_reset_timer.start(FEEDBACK_RESET_TIME)
		reset_name_edit()
		return
	
	_game_name_preview.text = GameNameRules.format_game_name(game_name)
	_previous_valid_game_name = game_name

# Reverts the LineEdit's text to the last known valid name value, preserving
# the caret at the end of the text.
func reset_name_edit() -> void:
	_project_name_edit.text = _previous_valid_game_name
	_project_name_edit.caret_column = _previous_valid_game_name.length()

# Restores the preview label to the formatted last valid game name.
# Called when the feedback display timer expires.
func restore_previous_game_name_preview() -> void:
	_game_name_preview.text = GameNameRules.format_game_name(_previous_valid_game_name)
