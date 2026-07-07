using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class ChartPlayer
{
    public const int LINE_COUNT = 4;

    public const long TICK_RATE = 1_000_000;

    public const long JUDGE_PERFECT =  30_000;
    public const long JUDGE_GOOD    = 100_000;
    public const long JUDGE_BAD     = 250_000;

    public static ChartData Data => ChartManager.Chart;

    public event Action<long> TickAdvancedBy;
    
    public event Action<JudgeResult> JudgeEvent;

    public event Action OnPlayEnd;

    public long CurrentTick => (long)(timer.Elapsed.TotalSeconds * TICK_RATE);
    
    public bool IsEnd => Lines.All(x => x.IsEnd());

    private readonly Stopwatch timer = new();

    private readonly LineData[] Lines = new LineData[LINE_COUNT];

    public ChartPlayer()
    {
        for (int i = 0; i < LINE_COUNT; i++)
        {
            var line = Lines[i] = new();

            line.JudgeEvent += judge => JudgeEvent?.Invoke(judge);

            TickAdvancedBy += line.EnqueueNote;
            TickAdvancedBy += line.DequeueNote;
        }

        foreach (var note in Data.Notes)
        {
            Lines[note.Line].AddNote(note);
        }
    }

    public IEnumerator Play()
    {
        timer.Start();

        while (!IsEnd)
        {
            TickAdvancedBy?.Invoke(CurrentTick);
            yield return null;
        }

        timer.Stop();
        OnPlayEnd?.Invoke();
    }

    public void OnCilck(Index line)
    {
        Lines[line].Click(CurrentTick);
    }

    public void OnCancel(Index line)
    {
        Lines[line].Cancel(CurrentTick);
    }

    public class LineData
    {
        private int currentNoteIdx = 0;

        private readonly List<NoteData> NoteList = new();

        private readonly Queue<NoteData> JudgeQueue = new();

        public event Action<JudgeResult> JudgeEvent;

        public bool IsEnd()
        {
            return JudgeQueue.Count == 0 &&
                   NoteList.Count == currentNoteIdx;
        }

        public void AddNote(NoteData data)
        {
            int left = 0;
            int right = NoteList.Count;
            while (left < right)
            {
                int mid = (left + right) >> 1;
                if (NoteList[mid].Tick < data.Tick)
                {
                    left = mid + 1;
                }
                else if (NoteList[mid].Tick > data.Tick)
                {
                    right = mid;
                }
                else
                {
                    Debug.LogWarning($"{data.Tick} is repeated.");
                    return;
                }
            }

            if (data.Type == NoteData.NoteType.Long)
            {
                NoteList.Insert(left,
                    new(data.Tick + data.Length,
                        data.Line,
                        NoteData.NoteType.LongTail)
                );
            }

            NoteList.Insert(left, data);
        }

        public void EnqueueNote(long currentTick)
        {
            int max = NoteList.Count;
            while (
                currentNoteIdx < max &&
                Judge(NoteList[currentNoteIdx], currentTick) != JudgeResult.None)
            {
                JudgeQueue.Enqueue(NoteList[currentNoteIdx]);
                currentNoteIdx++;
            }
        }

        public void DequeueNote(long currentTick)
        {
            while (JudgeQueue.TryPeek(out NoteData note) &&
                   (Judge(note, currentTick) == JudgeResult.Miss ||
                    (note.Type == NoteData.NoteType.LongTail && note.Tick <= currentTick)))
            {
                JudgeQueue.Dequeue();
                JudgeEvent?.Invoke(JudgeResult.Miss);
            }
        }

        public void Click(long clickTick)
        {
            if (JudgeQueue.TryPeek(out NoteData note))
            {
                var judge = Judge(note, clickTick);
                if (judge != JudgeResult.None &&
                    (note.Type == NoteData.NoteType.Short ||
                     note.Type == NoteData.NoteType.Long))
                {
                    JudgeQueue.Dequeue();
                    JudgeEvent?.Invoke(judge);
                }
            }
        }

        public void Cancel(long cancelTick)
        {
            if (JudgeQueue.TryPeek(out NoteData note) && note.Type == NoteData.NoteType.LongTail)
            {
                var judge = Judge(note.Tick, cancelTick);
                if (judge == JudgeResult.None || judge == JudgeResult.VeryEarly)
                {
                    JudgeEvent?.Invoke(JudgeResult.Miss);
                }
                JudgeQueue.Dequeue();
            }
        }

        public static JudgeResult Judge(NoteData note, long clickTick) =>
            Judge(note.Tick, clickTick);

        public static JudgeResult Judge(long noteTick, long clickTick)
        {
            long diff = clickTick - noteTick;
            long absDiff = diff < 0 ? -diff : diff;

            if (absDiff <= JUDGE_PERFECT)
            {
                return JudgeResult.Perfect;
            }

            if (absDiff <= JUDGE_GOOD)
            {
                return diff < 0 ? JudgeResult.Early : JudgeResult.Late;
            }

            if (absDiff <= JUDGE_BAD)
            {
                return diff < 0 ? JudgeResult.VeryEarly : JudgeResult.VeryLate;
            }

            return diff < 0 ? JudgeResult.None : JudgeResult.Miss;
        }
    }

    public enum JudgeResult
    {
        None,
        VeryEarly,
        Early,
        Perfect,
        Late,
        VeryLate,
        Miss,
    }
}