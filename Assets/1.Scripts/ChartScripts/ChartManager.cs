using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public class ChartManager : MonoBehaviour
{
    public static ChartData Chart { get; private set; }

    private static readonly Dictionary<string, ChartData> ChartDatas = new();

    public static IEnumerator LoadChartsCoroutine()
    {
        if (ChartDatas.Count > 0)
        {
            Debug.LogWarning("Charts are loaded.");
            yield break;
        }
        
        var levelsAsync = Resources.LoadAsync<TextAsset>("Levels/Levels"); 
        yield return levelsAsync;
        var levels = levelsAsync.asset as TextAsset;

        if (levels == null)
        {
            Debug.LogError("Failed to find 'Levels.json'.");
            yield break;
        }

        var list = JsonSerializer.Deserialize<string[]>(levels.text);
        foreach (var name in list)
        {
            var jsonAsync = Resources.LoadAsync<TextAsset>($"Levels/Charts/{name}");
            var clipAsync = Resources.LoadAsync<AudioClip>($"Levels/Musics/{name}");
            yield return jsonAsync;
            yield return clipAsync;
            var json = jsonAsync.asset as TextAsset;
            var clip = clipAsync.asset as AudioClip;

            if (json == null || clip == null)
            {
                Debug.LogWarning($"Failed to find '{name}'.");
                continue;
            }

            ChartData chart;
            try
            {
                chart = JsonSerializer.Deserialize<ChartData>(json.text)
                    ?? throw new ArgumentNullException();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                continue;
            }

            chart.Clip = clip;

            if (!ChartDatas.TryAdd(name, chart))
            {
                Debug.LogWarning($"{name} is repeated.");
            }
        }
    }

    public static bool SetChart(string name)
    {
        if (ChartDatas.TryGetValue(name, out ChartData chart))
        {
            Chart = chart;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Start()
    {
        StartCoroutine(LoadChartsCoroutine());
    }
}
