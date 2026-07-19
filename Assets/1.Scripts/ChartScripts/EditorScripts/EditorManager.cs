using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    public static EditorManager Instance { get; private set; }

    private readonly List<EditorNote> notes = new();

    [SerializeField] private GameObject editorNoteObj;
    [SerializeField] private Transform rootTr;

    [SerializeField] private float[] lineLocates = new float[4];

    [SerializeField] private GameObject beatLineObj;

    [SerializeField] private int lineCount = 100;
    private readonly List<LineRenderer> beatLineRenderers = new();

    public float LocateY    { get; set; } = 0f;
    public float Scale      { get; set; } = 1f;
    public int TopNumber    { get; private set; } = 4;
    public int BottomNumber { get; private set; } = 4;

    public int Division { get; private set; } = 4;

    public string Title { get; private set; } = "test";
    public int BPM      { get; private set; } = 120;
    public IReadOnlyList<EditorNote> Notes => notes;
    public IReadOnlyList<float> LineLocates => lineLocates;

    public void AddNote(int line, int measure, int numerator, int denominator)
    {
        var editNote = Instantiate(
            editorNoteObj,
            rootTr
        ).GetComponent<EditorNote>();
        editNote.MeasureCount = measure;
        editNote.Numerator    = numerator;
        editNote.Denominator  = denominator;
        editNote.Line = line;
        editNote.Type = NoteData.NoteType.Short;

        notes.Add(editNote);
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
        Debug.Log(index);
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
        
    }

    public void Load()
    {
        
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
            var y = (worldPointY - LocateY) / Scale - offset;
            var me = (int)y;

            var nu = (int)(y % 1 * Division + 0.5);
            var de = Division;
            if (nu >= Division)
            {
                nu = 0;
                me++;
            }

            AddNote(line, me, nu, de);
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
            int m = i / Division;
            int b = i % Division;
            var line = beatLineRenderers[i];
            Vector3 begin = new(-2, m + b / (float)Division);
            Vector3 end   = new( 2, m + b / (float)Division);
            line.SetPosition(0, begin);
            line.SetPosition(1, end);
        }
    }
}
