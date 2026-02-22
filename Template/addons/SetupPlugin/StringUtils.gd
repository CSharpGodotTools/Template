class_name StringUtils

static func is_alphanumeric(text: String) -> bool:
	if text.is_empty():
		return false
	
	var regex = RegEx.new()
	regex.compile("^[a-zA-Z0-9]+$")
	return regex.search(text) != null
