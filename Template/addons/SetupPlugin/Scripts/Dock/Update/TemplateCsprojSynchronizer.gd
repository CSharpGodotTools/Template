@tool
class_name TemplateCsprojSynchronizer
extends RefCounted

const NamespaceMigration = preload("res://addons/SetupPlugin/Scripts/Setup/NamespaceMigration.gd")

func sync_template_settings(template_csproj_path: String, project_root: String) -> Dictionary:
	if not FileAccess.file_exists(template_csproj_path):
		return _error_result("Template.csproj was not found in the downloaded update.")

	var target_csproj_path: String = _find_target_csproj_path(project_root)
	if target_csproj_path.is_empty():
		return _error_result("No .csproj was found in the project root.")

	var template_content: String = FileAccess.get_file_as_string(template_csproj_path)
	var target_content: String = FileAccess.get_file_as_string(target_csproj_path)
	if template_content.is_empty() or target_content.is_empty():
		return _error_result("Failed to read .csproj files for synchronization.")

	target_content = _sync_target_framework(template_content, target_content)
	target_content = _sync_lang_version(template_content, target_content)

	var packages = load("res://addons/SetupPlugin/Scripts/Dock/Update/TemplateCsprojPackages.gd").new()
	for package_variant in packages.extract_packages(template_content):
		if package_variant is Dictionary:
			target_content = packages.upsert_package_reference(target_content, package_variant as Dictionary)

	var write_file: FileAccess = FileAccess.open(target_csproj_path, FileAccess.WRITE)
	if write_file == null:
		return _error_result("Failed to write updated .csproj: %s" % target_csproj_path)
	write_file.store_string(target_content)
	write_file.close()

	if not FileAccess.file_exists(project_root.path_join("Template.csproj")):
		var root_namespace: String = _read_xml_tag_value(target_content, "RootNamespace")
		if not root_namespace.is_empty():
			NamespaceMigration.rename_template_namespaces(project_root, root_namespace, true)

	return {"success": true}

func _sync_target_framework(template_content: String, target_content: String) -> String:
	var target_framework: String = _read_xml_tag_value(template_content, "TargetFramework")
	return _set_xml_tag_value(target_content, "TargetFramework", target_framework) if not target_framework.is_empty() else target_content

func _sync_lang_version(template_content: String, target_content: String) -> String:
	var lang_version: String = _read_xml_tag_value(template_content, "LangVersion")
	return _set_xml_tag_value(target_content, "LangVersion", lang_version) if not lang_version.is_empty() else target_content

func _find_target_csproj_path(project_root: String) -> String:
	var root_dir: DirAccess = DirAccess.open(project_root)
	if root_dir == null:
		return ""

	var csproj_files: Array[String] = []
	root_dir.list_dir_begin()
	var name: String = root_dir.get_next()
	while name != "":
		if not root_dir.current_is_dir() and name.ends_with(".csproj"):
			csproj_files.append(project_root.path_join(name))
		name = root_dir.get_next()
	root_dir.list_dir_end()

	if csproj_files.is_empty():
		return ""
	csproj_files.sort()
	for csproj_file in csproj_files:
		if not csproj_file.ends_with("Template.csproj"):
			return csproj_file
	return csproj_files[0]

func _read_xml_tag_value(content: String, tag_name: String) -> String:
	var regex: RegEx = RegEx.new()
	if regex.compile("<%s>([^<]+)</%s>" % [tag_name, tag_name]) != OK:
		return ""
	var match: RegExMatch = regex.search(content)
	return match.get_string(1).strip_edges() if match != null else ""

func _set_xml_tag_value(content: String, tag_name: String, value: String) -> String:
	var tag_regex: RegEx = RegEx.new()
	if tag_regex.compile("<%s>[^<]*</%s>" % [tag_name, tag_name]) == OK:
		var existing_match: RegExMatch = tag_regex.search(content)
		if existing_match != null:
			var replacement: String = "<%s>%s</%s>" % [tag_name, value, tag_name]
			return content.substr(0, existing_match.get_start()) + replacement + content.substr(existing_match.get_end())

	var group_regex: RegEx = RegEx.new()
	if group_regex.compile("(?s)<PropertyGroup>.*?</PropertyGroup>") != OK:
		return content
	var group_match: RegExMatch = group_regex.search(content)
	if group_match == null:
		return content.replace("</Project>", "    <PropertyGroup>\n        <%s>%s</%s>\n    </PropertyGroup>\n</Project>" % [tag_name, value, tag_name]) if content.contains("</Project>") else content

	var property_group: String = group_match.get_string(0)
	var insertion: String = "\n        <%s>%s</%s>" % [tag_name, value, tag_name]
	var updated_group: String = property_group.replace("</PropertyGroup>", "%s\n    </PropertyGroup>" % insertion)
	return content.substr(0, group_match.get_start()) + updated_group + content.substr(group_match.get_end())

func _error_result(message: String) -> Dictionary:
	return {"success": false, "message": message}
