using System.Collections;
using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private GameObject noteBody;
    [SerializeField] private LineRenderer longTail;

    public long StartTick         { get; private set; }
    public long Length            { get; private set; }
    public int  Line              { get; private set; }
    public NoteData.NoteType Type { get; private set; }

    public void SetNoteData(ChartData chart, NoteData note)
    {
        Line = note.Line;
        Type = note.Type;
    
        var bpm = chart.BPM;
        var sig = chart.TimeSignature;

        var measure = sig.GetDouble() * 240.0 / bpm;
        var time = measure * note.StartPos.GetDouble();

        StartTick = (long)(time * ChartManager.TICK_RATE_DOUBLE);
        Length = Type != NoteData.NoteType.Short ?
            (long)((measure * note.EndPos.GetDouble() - time) * ChartManager.TICK_RATE_DOUBLE) : 0;

        var x = ChartManager.Instance.LinePositions[Line];
        var y = (float)(time * ChartManager.Instance.Speed);
        transform.localPosition = new(x, y);
    }
    
    private void Start()
    {
        if (Type == NoteData.NoteType.LongTail)
        {
            StartCoroutine(LongTailLocate());
            noteBody.SetActive(false);
        }
        else
        {
            longTail.gameObject.SetActive(false);
        }
    }

    private IEnumerator LongTailLocate()
    {
        longTail.positionCount = 2;
        while (longTail.gameObject.activeSelf && longTail.enabled)
        {
            float judgeDis = ChartManager.Instance.Offset - transform.position.y;
            float tailDis  = (float)(Length / ChartManager.TICK_RATE_DOUBLE * ChartManager.Instance.Speed);
            float dis = Mathf.Max(judgeDis, tailDis);
            Vector3 pos = new(0f, dis);
            longTail.SetPosition(1, pos);
            yield return null;
        }
    }
}