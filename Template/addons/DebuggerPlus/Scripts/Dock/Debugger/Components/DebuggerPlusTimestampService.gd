@tool
extends RefCounted

var _run_started_at_msec: int = 0
var _entry_timestamps_by_raw: Dictionary = {}
var _pending_error_capture_elapsed_msec: Array[int] = []

func on_debugger_event(event_name: StringName) -> void:
	if event_name == &"started":
		_run_started_at_msec = Time.get_ticks_msec()
		_entry_timestamps_by_raw.clear()
		_pending_error_capture_elapsed_msec.clear()
		return
	if event_name == &"capture":
		_pending_error_capture_elapsed_msec.append(_current_elapsed_msec(Time.get_ticks_msec()))

func apply_timestamps(raw_entries: PackedStringArray) -> PackedStringArray:
	var now_msec: int = Time.get_ticks_msec()
	var active_occurrence_counts: Dictionary = {}
	var result: PackedStringArray = []

	for raw_entry in raw_entries:
		var key: String = raw_entry.strip_edges()
		if key.is_empty():
			continue
		if entry_has_timestamp_prefix(raw_entry):
			result.append(raw_entry)
			continue

		var occurrence_index: int = int(active_occurrence_counts.get(key, 0))
		active_occurrence_counts[key] = occurrence_index + 1
		var timestamps: Array[String] = _timestamps_for_key(key)
		while timestamps.size() <= occurrence_index:
			timestamps.append(_consume_next_entry_timestamp(now_msec))
		_entry_timestamps_by_raw[key] = timestamps
		result.append(prefix_entry_with_timestamp(raw_entry, timestamps[occurrence_index]))

	_prune_stale_timestamps(active_occurrence_counts)
	return result

func entries_without_timestamp(entries: PackedStringArray) -> PackedStringArray:
	var stripped: PackedStringArray = []
	for entry in entries:
		stripped.append(strip_timestamp_from_entry(entry))
	return stripped

func strip_timestamp_from_entry(entry: String) -> String:
	if entry.is_empty():
		return entry
	var lines: PackedStringArray = entry.split("\n", false)
	if lines.is_empty():
		return entry
	var regex: RegEx = RegEx.new()
	if regex.compile("^\\d+:\\d{2}:\\d{2}:\\d{3}\\s+") == OK:
		lines[0] = regex.sub(lines[0], "", false)
	return "\n".join(lines)

func prefix_entry_with_timestamp(entry: String, timestamp: String) -> String:
	if entry.is_empty():
		return entry
	var lines: PackedStringArray = entry.split("\n", false)
	if lines.is_empty():
		return "%s %s" % [timestamp, entry]
	lines[0] = "%s %s" % [timestamp, lines[0]]
	return "\n".join(lines)

func entry_has_timestamp_prefix(entry: String) -> bool:
	if entry.is_empty():
		return false
	var first_line: String = entry.split("\n", false)[0].strip_edges()
	var regex: RegEx = RegEx.new()
	if regex.compile("^\\d+:\\d{2}:\\d{2}:\\d{3}\\b") != OK:
		return false
	return regex.search(first_line) != null

func _timestamps_for_key(key: String) -> Array[String]:
	if not _entry_timestamps_by_raw.has(key):
		return []
	var stored: Variant = _entry_timestamps_by_raw[key]
	if stored is Array[String]:
		return stored as Array[String]
	if stored is Array:
		var fallback: Array[String] = []
		for value in stored:
			fallback.append(str(value))
		return fallback
	return []

func _prune_stale_timestamps(active_occurrence_counts: Dictionary) -> void:
	var stale_keys: Array = []
	for existing_key_variant in _entry_timestamps_by_raw.keys():
		var existing_key: String = str(existing_key_variant)
		if not active_occurrence_counts.has(existing_key):
			stale_keys.append(existing_key)
			continue
		var keep_count: int = int(active_occurrence_counts[existing_key])
		var timestamps: Array[String] = _timestamps_for_key(existing_key)
		if timestamps.size() > keep_count:
			timestamps.resize(keep_count)
			_entry_timestamps_by_raw[existing_key] = timestamps
	for stale_key_variant in stale_keys:
		_entry_timestamps_by_raw.erase(str(stale_key_variant))

func _consume_next_entry_timestamp(now_msec: int) -> String:
	if not _pending_error_capture_elapsed_msec.is_empty():
		var elapsed: int = int(_pending_error_capture_elapsed_msec[0])
		_pending_error_capture_elapsed_msec.remove_at(0)
		return _format_elapsed_from_msec(elapsed)
	return _format_elapsed_from_msec(_current_elapsed_msec(now_msec))

func _current_elapsed_msec(now_msec: int) -> int:
	var base_msec: int = _run_started_at_msec
	if base_msec <= 0:
		base_msec = now_msec
	return maxi(0, now_msec - base_msec)

func _format_elapsed_from_msec(elapsed: int) -> String:
	var total_seconds: int = elapsed / 1000
	var hours: int = total_seconds / 3600
	var minutes: int = (total_seconds % 3600) / 60
	var seconds: int = total_seconds % 60
	var millis: int = elapsed % 1000
	return "%d:%02d:%02d:%03d" % [hours, minutes, seconds, millis]
