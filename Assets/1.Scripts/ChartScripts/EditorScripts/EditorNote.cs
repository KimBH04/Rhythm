using System;
using UnityEngine;

public class EditorNote : MonoBehaviour, IComparable<EditorNote>
{
    [SerializeField] private LineRenderer tail;

    public GreatFraction StartPosition { get; set; }
    public GreatFraction EndPosition   { get; set; }

    public int Line               { get; set; }
    public NoteData.NoteType Type { get; set; }

    public NoteData GetNoteData()
    {
        return new(
            StartPosition,
            EndPosition,
            Line,
            Type
        );
    }

    public int CompareTo(EditorNote other)
    {
        if (other == null)
        {
            return -1;
        }
        
        return StartPosition.CompareTo(other.StartPosition);
    }

    private void Update()
    {
        var y = (StartPosition.GetDouble() - EditorManager.Instance.Scroll) * EditorManager.Instance.Scale;
        transform.localPosition = new(
            EditorManager.Instance.LinePositions[Line],
            (float)y
        );
    }
}
