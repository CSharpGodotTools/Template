@tool
# Parses and patches NuGet PackageReference elements inside a .csproj XML string.
# Used during template updates to keep the project's NuGet packages in sync
# with the template's specification.
class_name TemplateCsprojPackages
extends RefCounted

# Parses all PackageReference entries from the given .csproj XML string.
# Returns an Array of Dictionaries, each with "include", "version", and
# "element" keys.  Entries without a Version attribute are skipped.
func extract_packages(template_content: String) -> Array:
	var package_regex: RegEx = RegEx.new()
	if package_regex.compile("(?s)<PackageReference\\b[^>]*Include=\"([^\"]+)\"[^>]*(?:/>|>.*?</PackageReference>)") != OK:
		return []

	var version_regex: RegEx = RegEx.new()
	if version_regex.compile("Version=\"([^\"]+)\"") != OK:
		return []

	var unique_by_include: Dictionary = {}
	for match in package_regex.search_all(template_content):
		var include_name: String = match.get_string(1)
		var package_element: String = match.get_string(0)
		var version_match: RegExMatch = version_regex.search(package_element)
		if version_match == null:
			continue

		unique_by_include[include_name] = {
			"include": include_name,
			"version": version_match.get_string(1),
			"element": package_element
		}

	return unique_by_include.values()

# Updates the version of an existing PackageReference, or inserts the full
# element if the package is not yet referenced in the target content.
func upsert_package_reference(target_content: String, package_data: Dictionary) -> String:
	var include_name: String = str(package_data.get("include", ""))
	var version: String = str(package_data.get("version", ""))
	var package_element: String = str(package_data.get("element", ""))
	if include_name.is_empty() or version.is_empty():
		return target_content

	var package_regex: RegEx = RegEx.new()
	if package_regex.compile("<PackageReference\\b[^>]*Include=\"%s\"[^>]*?/?>" % _escape_regex(include_name)) != OK:
		return target_content

	var existing_match: RegExMatch = package_regex.search(target_content)
	if existing_match == null:
		return _insert_package_reference(target_content, package_element)

	var existing_tag: String = existing_match.get_string(0)
	var version_regex: RegEx = RegEx.new()
	if version_regex.compile("Version=\"[^\"]*\"") != OK:
		return target_content

	var updated_tag: String = existing_tag
	if version_regex.search(existing_tag) != null:
		updated_tag = version_regex.sub(existing_tag, "Version=\"%s\"" % version, true)
	elif existing_tag.ends_with("/>"):
		updated_tag = existing_tag.trim_suffix("/>") + " Version=\"%s\" />" % version
	else:
		updated_tag = existing_tag.trim_suffix(">") + " Version=\"%s\">" % version

	return _replace_match(target_content, existing_match, updated_tag)

# Inserts a new PackageReference element into the first ItemGroup that already
# contains package references, or creates a new ItemGroup if none exists.
func _insert_package_reference(target_content: String, package_element: String) -> String:
	var group_regex: RegEx = RegEx.new()
	if group_regex.compile("(?s)<ItemGroup>.*?<PackageReference\\b.*?</ItemGroup>") == OK:
		var group_match: RegExMatch = group_regex.search(target_content)
		if group_match != null:
			var group_content: String = group_match.get_string(0)
			var insertion: String = "\n        %s\n    " % package_element.strip_edges()
			var updated_group: String = group_content.replace("</ItemGroup>", "%s</ItemGroup>" % insertion)
			return _replace_match(target_content, group_match, updated_group)

	if target_content.contains("</Project>"):
		var block: String = "    <ItemGroup>\n        %s\n    </ItemGroup>\n" % package_element.strip_edges()
		return target_content.replace("</Project>", "%s</Project>" % block)

	return target_content

# Replaces the substring captured by `match` with `replacement` text.
func _replace_match(content: String, match: RegExMatch, replacement: String) -> String:
	return content.substr(0, match.get_start()) + replacement + content.substr(match.get_end())

# Escapes all special regex metacharacters in `value` so it can be used as a
# literal string pattern inside a regex.
func _escape_regex(value: String) -> String:
	var escaped: String = value
	for token in ["\\", ".", "+", "*", "?", "[", "]", "(", ")", "{", "}", "^", "$", "|"]:
		escaped = escaped.replace(token, "\\" + token)
	return escaped
