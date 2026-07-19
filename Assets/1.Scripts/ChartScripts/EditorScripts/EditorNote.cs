using System;
using UnityEngine;

public class EditorNote : MonoBehaviour, IComparable<EditorNote>
{
    [SerializeField] private LineRenderer tail;

    public int MeasureCount { get; set; }
    public int Numerator    { get; set; }
    public int Denominator  { get; set; }

    public int Line               { get; set; }
    public NoteData.NoteType Type { get; set; }
    public double Length          { get; set; }

    public NoteData GetNoteData()
    {
        double bpm    = EditorManager.Instance.BPM;
        double top    = EditorManager.Instance.TopNumber;
        double bottom = EditorManager.Instance.BottomNumber;
        double cnt    = MeasureCount;
        double nu     = Numerator;
        double de     = Denominator;

        double time = 240.0 * top * (nu + de * cnt) / (bottom * bpm * de);

        return new(
            (long)(time * ChartManager.TICK_RATE),
            Line,
            Type,
            Type == NoteData.NoteType.Long ?
                (long)(Length * ChartManager.TICK_RATE) : 0
        );
    }

    public int CompareTo(EditorNote other)
    {
        if (other == null)
        {
            return -1;
        }

        var o = (other.MeasureCount, (double)other.Numerator / other.Denominator);
        return (MeasureCount, (double)Numerator / Denominator).CompareTo(o);
    }

    private void Update()
    {
        transform.localPosition = new(
            EditorManager.Instance.LineLocates[Line],
            (MeasureCount + (float)Numerator / Denominator) * EditorManager.Instance.Scale
        );
    }
}
