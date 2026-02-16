using System.IO;

namespace Framework.Setup;

public static class SetupDirectoryMaintenance
{
    public static void DeleteEmptyDirectories(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
        {
            return;
        }

        DeleteEmptyDirectoriesRecursive(rootDirectory, isRootDirectory: true);
    }

    private static bool DeleteEmptyDirectoriesRecursive(string directory, bool isRootDirectory)
    {
        string[] childDirectories = Directory.GetDirectories(directory);
        bool hasFiles = Directory.GetFiles(directory).Length > 0;
        bool hasNonEmptyChildren = false;

        foreach (string childDirectory in childDirectories)
        {
            bool childHasContent = DeleteEmptyDirectoriesRecursive(childDirectory, isRootDirectory: false);
            if (childHasContent)
            {
                hasNonEmptyChildren = true;
            }
        }

        bool hasContent = hasFiles || hasNonEmptyChildren;
        if (!hasContent && !isRootDirectory)
        {
            Directory.Delete(directory, recursive: false);
        }

        return hasContent;
    }
}
