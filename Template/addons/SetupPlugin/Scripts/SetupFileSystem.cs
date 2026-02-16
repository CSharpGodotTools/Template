using System.IO;

namespace Framework.Setup;

public static class SetupFileSystem
{
    public static void EnsureGDIgnoreFilesInGDUnitTestFolders(string projectRoot)
    {
        string[] folders =
        [
            "TestResults",
            "gdunit4_testadapter_v5"
        ];

        foreach (string folder in folders)
        {
            string folderPath = Path.Combine(projectRoot, folder);
            Directory.CreateDirectory(folderPath);

            string gdIgnorePath = Path.Combine(folderPath, ".gdignore");
            if (File.Exists(gdIgnorePath))
            {
                continue;
            }

            File.WriteAllText(gdIgnorePath, string.Empty);
        }
    }

    public static string GetUidFromSceneFile(string sceneFilePath)
    {
        using StreamReader reader = new StreamReader(sceneFilePath);

        string firstLine = reader.ReadLine();
        if (string.IsNullOrEmpty(firstLine) || !firstLine.Contains("gd_scene"))
        {
            return null;
        }

        return firstLine.Split("uid=")[1].Split('"')[1];
    }
}
