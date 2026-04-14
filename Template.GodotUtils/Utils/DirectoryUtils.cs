using Godot;
using System;
using System.IO;

namespace GodotUtils;

/// <summary>
/// Directory traversal and cleanup helpers for Godot project paths.
/// </summary>
public static class DirectoryUtils
{
    /// <summary>
    /// Recursively traverses a directory tree and invokes a callback for each file encountered.
    /// The callback may control traversal flow (continue, skip children, or stop traversal).
    /// 
    /// <code>
    /// Traverse("res://", entry => GD.Print(entry.FullPath));
    /// </code>
    /// </summary>
    /// <param name="directory">Root directory path.</param>
    /// <param name="visitor">Callback invoked for each entry.</param>
    /// <returns>Final traversal decision.</returns>
    public static TraverseDecision Traverse(string directory, Func<TraverseEntry, TraverseDecision> visitor)
    {
        directory = NormalizePath(ProjectSettings.GlobalizePath(directory));

        using DirAccess dir = DirAccess.Open(directory);
        dir.ListDirBegin();

        string nextFileName;

        while ((nextFileName = dir.GetNext()) != string.Empty)
        {
            // Skip hidden entries to mirror Godot editor visibility defaults.
            if (nextFileName.StartsWith('.'))
                continue;

            string fullPath = Path.Combine(directory, nextFileName);
            bool isDir = dir.CurrentIsDir();

            TraverseDecision result = visitor(new TraverseEntry(fullPath, isDir));

            // Stop traversal as soon as callback requests termination.
            if (result == TraverseDecision.Stop)
            {
                dir.ListDirEnd();
                return TraverseDecision.Stop;
            }

            // Recurse into subdirectories unless callback requested a child skip.
            if (isDir && result != TraverseDecision.SkipChildren)
            {
                TraverseDecision childResult = Traverse(fullPath, visitor);

                // Propagate stop decisions from recursive calls.
                if (childResult == TraverseDecision.Stop)
                {
                    dir.ListDirEnd();
                    return TraverseDecision.Stop;
                }
            }
        }

        dir.ListDirEnd();
        return TraverseDecision.Continue;
    }

    /// <summary>
    /// Single directory entry visited during traversal.
    /// </summary>
    /// <param name="fullPath">Full path to the entry.</param>
    /// <param name="isDirectory">Whether the entry is a directory.</param>
    public readonly struct TraverseEntry(string fullPath, bool isDirectory)
    {
        /// <summary>
        /// Full normalized path.
        /// </summary>
        public string FullPath { get; } = fullPath;

        /// <summary>
        /// Indicates whether the entry is a directory.
        /// </summary>
        public bool IsDirectory { get; } = isDirectory;

        /// <summary>
        /// File or directory name component.
        /// </summary>
        /// <value>Terminal file-system name extracted from <see cref="FullPath"/>.</value>
        public string FileName => Path.GetFileName(FullPath);
    }

    /// <summary>
    /// Recursively searches for the file name and if found returns the full file path to
    /// that file.
    /// 
    /// <code>
    /// string fullPathToPlayer = FindFile("res://", "Player.tscn")
    /// </code>
    /// </summary>
    /// <param name="directory">Root directory to search.</param>
    /// <param name="fileName">Exact file name to locate.</param>
    /// <returns>Returns the full path to the file or null if the file is not found</returns>
    public static string? FindFile(string directory, string fileName)
    {
        string? foundPath = null;

        Traverse(directory, entry =>
        {
            // Stop at first name match and capture the resolved path.
            if (Path.GetFileName(entry.FullPath) == fileName)
            {
                foundPath = entry.FullPath;
                return TraverseDecision.Stop;
            }

            return TraverseDecision.Continue;
        });

        return foundPath;
    }

    /// <summary>
    /// Normalizes path separators to the current OS.
    /// </summary>
    /// <param name="path">Path to normalize.</param>
    /// <returns>Normalized path string.</returns>
    public static string NormalizePath(string path)
    {
        return path
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Recursively deletes all empty folders in this folder
    /// </summary>
    /// <param name="path">Root directory to clean.</param>
    public static void DeleteEmptyDirectories(string path)
    {
        path = NormalizePath(ProjectSettings.GlobalizePath(path));

        // Recurse only when the root path currently exists.
        if (Directory.Exists(path))
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteEmptyDirectories(directory);
                DeleteEmptyDirectory(directory);
            }
        }
    }

    /// <summary>
    /// Checks if the folder is empty and deletes it if it is
    /// </summary>
    /// <param name="path">Directory path to evaluate.</param>
    private static void DeleteEmptyDirectory(string path)
    {
        path = NormalizePath(ProjectSettings.GlobalizePath(path));

        // Delete only leaf directories that have no files and no subdirectories.
        if (IsDirectoryEmpty(path))
            Directory.Delete(path, recursive: false);
    }

    /// <summary>
    /// Checks if the directory is empty
    /// </summary>
    /// <param name="path">Directory path to evaluate.</param>
    /// <returns>Returns true if the directory is empty</returns>
    private static bool IsDirectoryEmpty(string path)
    {
        path = NormalizePath(ProjectSettings.GlobalizePath(path));

        return Directory.GetDirectories(path).Length == 0 && Directory.GetFiles(path).Length == 0;
    }
}
