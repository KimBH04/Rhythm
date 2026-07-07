using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public class ChartManager : MonoBehaviour
{
    public const float TICK_RATE = 1000000f;
    public static ChartData Chart { get; private set; }

    private static ChartPlayer player;

    private static readonly Dictionary<string, ChartData> ChartDatas = new();

    [SerializeField] private float speed = 5f;

    [SerializeField] private Transform rootTr;
    [SerializeField] private GameObject noteObj;
    [SerializeField] private float[] lineLocates;

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
                Debug.LogError($"Error of {name}'s datas:\n{e}");
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

    public void OnClick(Index index)
    {
        player.OnCilck(index);
    }

    public void OnCancel(Index index)
    {
        player.OnCancel(index);
    }

    private IEnumerator Start()
    {
        yield return LoadChartsCoroutine();
        SetChart("test");
        player = new();
        PlaceNotes();

        player.JudgeEvent += DisplayJudgment;
        player.OnPlayEnd += OnEnd;
        StartCoroutine(player.Play());

        while (!player.IsEnd)
        {
            rootTr.localPosition = new(0f, -player.CurrentTick * speed / TICK_RATE);
            yield return null;
        }
    }   

    private void PlaceNotes()
    {
        foreach (var note in Chart.Notes)
        {
            Vector3 pos = new(lineLocates[note.Line], note.Tick * speed / TICK_RATE);
            Instantiate(noteObj, pos + transform.position, Quaternion.identity, rootTr);
        }
    }

    private void DisplayJudgment(ChartPlayer.JudgeResult result)
    {
        Debug.Log(result.ToString());
    }

    private void OnEnd()
    {
        Debug.Log("end");
    }
}
