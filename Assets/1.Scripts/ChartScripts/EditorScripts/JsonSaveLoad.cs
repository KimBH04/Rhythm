using System;
using System.IO;
using System.Text.Json;
using UnityEngine;
using SFB;

public static class ChartJsonSaveLoad
{
    private static readonly ExtensionFilter[] filters = { new("JSON", "json") };

    private static string directoryCache = Application.persistentDataPath;

    public static bool Save(ChartFileInfo file)
    {
        try
        {
            string path = StandaloneFileBrowser.SaveFilePanel(
                file.Name,
                directoryCache = file.Directory,
                "rhythm.json",
                filters
            );

            string json = JsonSerializer.Serialize(file.Chart);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save the file.\n" + e.ToString());
            return false;
        }
    }

    public static bool Load(out ChartFileInfo file)
    {
        try
        {
            string path = StandaloneFileBrowser.OpenFilePanel(
                "",
                directoryCache,
                filters,
                false
            )[0];

            file.Name      = Path.GetFileName(path);
            file.Directory = directoryCache = Path.GetDirectoryName(path);

            string json = File.ReadAllText(path);
            file.Chart = JsonSerializer.Deserialize<ChartData>(json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load the file.\n" + e.ToString());

            file = default;
            return false;
        }
    }
}

public struct ChartFileInfo
{
    public string Name;
    public string Directory;
    public ChartData Chart;
}