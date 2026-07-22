using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    public static EditorManager Instance { get; private set; }

    [SerializeField] private GameObject editorNoteObj;
    [SerializeField] private Transform rootTr;

    [SerializeField] private float[] linePositions = new float[4];

    [SerializeField] private GameObject beatLineObj;

    [SerializeField] private int lineCount = 100;

    private readonly List<EditorNote> notes = new();

    private readonly List<LineRenderer> beatLineRenderers = new();

    public float LocateY { get; set; } = 0f;
    public float Scale   { get; set; } = 4f;
    public Fraction TimeSignature { get; private set; } = new(4, 4);

    public int Division { get; private set; } = 4;

    public string Title { get; private set; } = "test";
    public int    BPM   { get; private set; } = 120;
    public IReadOnlyList<EditorNote> Notes => notes;
    public IReadOnlyList<float> LinePositions => linePositions;

    public void AddNote(int line, int measure, int numerator, int denominator)
    {
        var editNote = Instantiate(
            editorNoteObj,
            rootTr
        ).GetComponent<EditorNote>();
        editNote.StartPosition = new(measure, numerator, denominator);
        editNote.Line = line;
        editNote.Type = NoteData.NoteType.Short;

        notes.Add(editNote);
    }

    public void AddNote(NoteData data)
    {
        var editNote = Instantiate(
            editorNoteObj,
            rootTr
        ).GetComponent<EditorNote>();
        editNote.StartPosition = data.StartPos;
        editNote.EndPosition   = data.EndPos;
        editNote.Line = data.Line;
        editNote.Type = data.Type;
    }

    public void SetTitle(string title)
    {
        Title = title;
    }
    
    public void SetBpm(string bpmStr)
    {
        if (int.TryParse(bpmStr, out var bpm))
        {
            BPM = bpm;
        }
        else
        {
            Debug.LogWarning(bpmStr + " is invalid value.");
        }
    }

    public void SetDivision(int index)
    {
        int[] values = { 4, 3, 8, 6, 16, 12, 32, 24, 64, 48, 128, 96, 256, 192 };
        Division = values[index];
    }

    public void Play()
    {
        
    }

    public void Pause()
    {
        
    }

    public void Save()
    {
        ChartData data = new(Title, BPM, TimeSignature, notes.Select(n => n.GetNoteData()).OrderBy(n => n));
        ChartFileInfo info = new()
        {
            Name = new(Title.Where(char.IsLetterOrDigit).ToArray()),
            Chart = data
        };
        ChartJsonSaveLoad.Save(info);
    }

    public void Load()
    {
        if (ChartJsonSaveLoad.Load(out var chartFile))
        {
            Title = chartFile.Chart.Title;
            BPM   = chartFile.Chart.BPM;
            
            TimeSignature = chartFile.Chart.TimeSignature;
            
            notes.ForEach(n => Destroy(n.gameObject));
            notes.Clear();

            chartFile.Chart.Notes.ForEach(AddNote);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var offset = rootTr.position.y;
        NotePlacementInput.OnTrackClickEvent += (line, worldPointY) =>
        {
            checked
            {
                var y = (worldPointY - LocateY - offset) / Scale;
                var me = (int)y;
                var nu = (int)Math.Round(y % 1 * Division);
                var de = Division;

                AddNote(line, me, nu, de);
            }
        };
        
        for (int i = 0; i < lineCount; i++)
        {
            var line = Instantiate(beatLineObj, transform).GetComponent<LineRenderer>();
            line.positionCount = 2;
            beatLineRenderers.Add(line);
        }
    }

    private void Update()
    {
        BeatLine();
    }

    private void BeatLine()
    {
        for (int i = 0; i < lineCount; i++)
        {
            var pos  = (double)(i + LocateY) * Scale / Division;
            var line = beatLineRenderers[i];
            Vector3 begin = new(-2, (float)pos);
            Vector3 end   = new( 2, (float)pos);
            line.SetPosition(0, begin);
            line.SetPosition(1, end);
        }
    }
}
