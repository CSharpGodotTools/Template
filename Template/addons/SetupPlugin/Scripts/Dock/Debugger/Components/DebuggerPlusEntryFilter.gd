@tool
extends RefCounted

var _entry_text
var _all_entries: PackedStringArray = []
var _visible_entries: PackedStringArray = []
var _filter_text: String = ""

func _init(entry_text) -> void:
	_entry_text = entry_text

func set_entries(entries: PackedStringArray) -> bool:
	var has_changed: bool = not _packed_string_arrays_equal(_all_entries, entries)
	if has_changed:
		_all_entries = entries
	return has_changed

func set_filter_text(text: String) -> void:
	_filter_text = text.strip_edges().to_lower()

func apply(show_errors: bool, show_warnings: bool) -> PackedStringArray:
	_visible_entries = []
	for entry in _all_entries:
		if not _should_include_entry(entry, show_errors, show_warnings):
			continue
		if _filter_text.is_empty() or entry.to_lower().contains(_filter_text):
			_visible_entries.append(entry)
	return _visible_entries

func get_all_entries() -> PackedStringArray:
	return _all_entries

func get_visible_entries() -> PackedStringArray:
	return _visible_entries

func visible_count() -> int:
	return _visible_entries.size()

func all_count() -> int:
	return _all_entries.size()

func _should_include_entry(entry: String, show_errors: bool, show_warnings: bool) -> bool:
	if not show_errors and _entry_text.entry_is_error(entry):
		return false
	if not show_warnings and _entry_text.entry_is_warning(entry):
		return false
	return true

func _packed_string_arrays_equal(left: PackedStringArray, right: PackedStringArray) -> bool:
	if left.size() != right.size():
		return false
	for index in range(left.size()):
		if left[index] != right[index]:
			return false
	return true
