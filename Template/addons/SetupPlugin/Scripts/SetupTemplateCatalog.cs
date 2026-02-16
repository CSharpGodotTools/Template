using System;
using System.Collections.Generic;
using System.IO;

namespace Framework.Setup;

public sealed class SetupTemplateCatalog
{
    private readonly Dictionary<string, List<string>> _templatesByProjectType;

    private SetupTemplateCatalog(Dictionary<string, List<string>> templatesByProjectType)
    {
        _templatesByProjectType = templatesByProjectType;
    }

    public IEnumerable<string> ProjectTypes => _templatesByProjectType.Keys;

    public bool TryGetTemplates(string projectType, out IReadOnlyList<string> templates)
    {
        if (_templatesByProjectType.TryGetValue(projectType, out List<string> templateTypes))
        {
            templates = templateTypes;
            return true;
        }

        templates = Array.Empty<string>();
        return false;
    }

    public bool TryGetFirstSelection(out string projectType, out string templateType)
    {
        foreach (KeyValuePair<string, List<string>> entry in _templatesByProjectType)
        {
            if (entry.Value.Count == 0)
            {
                continue;
            }

            projectType = entry.Key;
            templateType = entry.Value[0];
            return true;
        }

        projectType = string.Empty;
        templateType = string.Empty;
        return false;
    }

    public static bool TryLoad(string mainScenesRoot, out SetupTemplateCatalog catalog, out string failureReason)
    {
        if (!Directory.Exists(mainScenesRoot))
        {
            catalog = null;
            failureReason = $"Missing setup templates directory: {mainScenesRoot}";
            return false;
        }

        Dictionary<string, List<string>> templatesByProjectType =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        string[] projectTypeDirectories = Directory.GetDirectories(mainScenesRoot);
        Array.Sort(projectTypeDirectories, StringComparer.OrdinalIgnoreCase);

        foreach (string projectTypeDirectory in projectTypeDirectories)
        {
            string projectTypeName = Path.GetFileName(projectTypeDirectory);
            if (string.IsNullOrWhiteSpace(projectTypeName))
            {
                continue;
            }

            string[] templateDirectories = Directory.GetDirectories(projectTypeDirectory);
            Array.Sort(templateDirectories, StringComparer.OrdinalIgnoreCase);

            List<string> templateTypes = new List<string>();
            foreach (string templateDirectory in templateDirectories)
            {
                string templateName = Path.GetFileName(templateDirectory);
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    continue;
                }

                templateTypes.Add(templateName);
            }

            if (templateTypes.Count == 0)
            {
                continue;
            }

            templatesByProjectType[projectTypeName] = templateTypes;
        }

        if (templatesByProjectType.Count == 0)
        {
            catalog = null;
            failureReason = "No setup templates were discovered.";
            return false;
        }

        catalog = new SetupTemplateCatalog(templatesByProjectType);
        failureReason = string.Empty;
        return true;
    }
}
