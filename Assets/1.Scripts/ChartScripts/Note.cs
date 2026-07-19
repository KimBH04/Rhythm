using System.Collections;
using UnityEngine;

public class Note : MonoBehaviour
{
    public NoteData Data
    {
        get => data;
        set
        {
            data = value;
            if (data.Type == NoteData.NoteType.LongTail)
            {
                noteBody.SetActive(false);
                lineObj.gameObject.SetActive(true);
            }
            else
            {
                noteBody.SetActive(true);
                lineObj.gameObject.SetActive(false);
            }
        }
    }

    private NoteData data;

    [SerializeField] private GameObject noteBody;
    [SerializeField] private LineRenderer lineObj;

    private void Start()
    {
        if (ChartManager.Instance != null &&
            data.Type == NoteData.NoteType.LongTail)
        {
            StartCoroutine(DrawTail());
        }
    }

    private IEnumerator DrawTail()
    {
        while (isActiveAndEnabled)
        {
            float judgeDis = -(transform.position.y + 2.5f);
            float tailDis  = (float)(data.Length / ChartManager.TICK_RATE * ChartManager.Speed);
            float dis = Mathf.Max(judgeDis, tailDis);
            Vector3 pos = new(0f, dis);
            lineObj.SetPosition(1, pos);
            yield return null;
        }
    }
}
