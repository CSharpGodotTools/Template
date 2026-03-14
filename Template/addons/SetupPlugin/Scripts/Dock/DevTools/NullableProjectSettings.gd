# Reads and writes the C# nullable reference types setting across two project
# files: Template.csproj (<Nullable> XML element) and .editorconfig (diagnostic
# suppression rules that silence IDE warnings when nullable is disabled).
extends RefCounted

const CSPROJ_PATH: String = "Template.csproj"
const EDITORCONFIG_PATH: String = ".editorconfig"
const CS8632_SUPPRESSION: String = "dotnet_diagnostic.CS8632.severity = none # The annotation for nullable reference types should only be used in code within a '#nullable' annotations context."
const IDE0370_SUPPRESSION: String = "dotnet_diagnostic.IDE0370.severity = none # Disable the IDE suggestion to enable nullable reference types."

# Returns true if nullable reference types are currently enabled in
# Template.csproj (i.e. the file contains <Nullable>enable</Nullable>).
static func read_state(project_root: String) -> bool:
	var file: FileAccess = FileAccess.open(project_root.path_join(CSPROJ_PATH), FileAccess.READ)
	if file == null:
		return false
	var content: String = file.get_as_text()
	file.close()
	return content.contains("<Nullable>enable</Nullable>")

# Applies the given enabled/disabled state to both Template.csproj and
# .editorconfig atomically.
static func set_state(project_root: String, enabled: bool) -> void:
	_set_csproj_nullable(project_root, enabled)
	_set_editorconfig_suppressions(project_root, not enabled)

# Replaces the <Nullable> element value inside Template.csproj with either
# "enable" or "disable".
static func _set_csproj_nullable(project_root: String, enabled: bool) -> void:
	var path: String = project_root.path_join(CSPROJ_PATH)
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return
	var content: String = file.get_as_text()
	file.close()
	content = content.replace("<Nullable>disable</Nullable>", "<Nullable>enable</Nullable>") if enabled else content.replace("<Nullable>enable</Nullable>", "<Nullable>disable</Nullable>")
	var write_file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if write_file != null:
		write_file.store_string(content)
		write_file.close()

# Inserts or removes the CS8632 and IDE0370 diagnostic suppression lines in
# .editorconfig.  These suppressions keep the IDE quiet when nullable is off.
static func _set_editorconfig_suppressions(project_root: String, suppress: bool) -> void:
	var path: String = project_root.path_join(EDITORCONFIG_PATH)
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return
	var content: String = file.get_as_text()
	file.close()
	var result: PackedStringArray = []
	for line in content.split("\n"):
		if not suppress and (line.begins_with("dotnet_diagnostic.CS8632.severity = none") or line.begins_with("dotnet_diagnostic.IDE0370.severity = none")):
			continue
		result.append(line)
		if suppress and line.begins_with("dotnet_diagnostic.CA1816.severity = none"):
			if not content.contains(CS8632_SUPPRESSION):
				result.append(CS8632_SUPPRESSION)
			if not content.contains(IDE0370_SUPPRESSION):
				result.append(IDE0370_SUPPRESSION)
	var write_file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if write_file != null:
		write_file.store_string("\n".join(result))
		write_file.close()