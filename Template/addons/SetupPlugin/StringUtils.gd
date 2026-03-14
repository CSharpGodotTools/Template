# Shared string utility functions used across the plugin.
class_name StringUtils

# Returns true if every character in text is alphanumeric [a-zA-Z0-9].
# Returns false for empty strings.
static func is_alphanumeric(text: String) -> bool:
	if text.is_empty():
		return false
	
	var regex = RegEx.new()
	regex.compile("^[a-zA-Z0-9]+$")
	return regex.search(text) != null
