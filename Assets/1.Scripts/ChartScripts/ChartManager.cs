using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public class ChartManager : MonoBehaviour
{
    public const double TICK_RATE_DOUBLE = ChartPlayer.TICK_RATE;

    public static ChartManager Instance { get; private set; }

    [SerializeField] private float speed = 5f;

    [SerializeField] private Transform offsetTr;
    [SerializeField] private Transform rootTr;
    [SerializeField] private GameObject noteObj;
    [SerializeField] private float[] linePositions;

    [SerializeField] private AudioSource audio;

    private AudioClip clip;

    private ChartPlayer player;

    private readonly List<Note> notes = new();

    private readonly Dictionary<string, ChartData> ChartDatas = new();

    public ChartData Chart { get; private set; }

    public float Speed => Instance.speed;

    public float Offset => offsetTr.position.y;

    public IReadOnlyList<float> LinePositions => linePositions;

    public IReadOnlyList<Note> Notes => notes;

    public IEnumerator LoadChartsCoroutine()
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

            this.clip = clip;
            if (!ChartDatas.TryAdd(name, chart))
            {
                Debug.LogWarning($"{name} is repeated.");
            }
        }
    }

    public bool SetChart(string name)
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

    public void OnClick(int index)
    {
        player.OnCilck(index);
    }

    public void OnCancel(int index)
    {
        player.OnCancel(index);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return LoadChartsCoroutine();
        SetChart("test");
        PlaceNotes();
        player = new();

        player.JudgeEvent += DisplayJudgment;
        player.OnPlayEnd += OnEnd;
        StartCoroutine(player.Play());

        while (!player.IsEnd)
        {
            rootTr.localPosition = new(0f, (float)(-player.CurrentTick / TICK_RATE_DOUBLE * speed));
            yield return null;
        }
    }   

    private void PlaceNotes()
    {
        notes.Clear();
        foreach (var data in Chart.Notes)
        {
            var note = Instantiate(noteObj, rootTr).GetComponent<Note>();
            note.SetNoteData(Chart, data);            
            notes.Add(note);
            
            if (data.Type == NoteData.NoteType.Long)
            {
                var tail = Instantiate(noteObj, rootTr).GetComponent<Note>();
                tail.SetNoteData(Chart, new(
                    data.EndPos,
                    data.StartPos,
                    data.Line,
                    NoteData.NoteType.LongTail
                ));
                notes.Add(tail);
            }
        }
    }

    private void DisplayJudgment(ChartPlayer.JudgeResult result)
    {
        Debug.Log(result.ToString());
    }

    private void OnEnd()
    {
        //Debug.Log("end");
    }
}
