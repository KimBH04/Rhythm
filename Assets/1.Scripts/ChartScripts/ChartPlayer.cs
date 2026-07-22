using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class ChartPlayer
{
    public const int LINE_COUNT = 4;

    public const long TICK_RATE = 1_000_000;

    public const long JUDGE_PERFECT =  30_000;
    public const long JUDGE_GOOD    = 100_000;
    public const long JUDGE_BAD     = 250_000;

    public static ChartData Data => ChartManager.Instance.Chart;

    public event Action<long> TickAdvancedBy;
    
    public event Action<JudgeResult> JudgeEvent;

    public event Action OnPlayEnd;

    private readonly Stopwatch timer = new();

    private readonly Line[] Lines = new Line[LINE_COUNT];

    public long CurrentTick => (long)(timer.Elapsed.TotalSeconds * TICK_RATE);
    
    public bool IsEnd => Lines.All(x => x.IsEnd());

    public ChartPlayer()
    {
        for (int i = 0; i < LINE_COUNT; i++)
        {
            var line = Lines[i] = new();

            line.JudgeEvent += judge => JudgeEvent?.Invoke(judge);

            TickAdvancedBy += line.EnqueueNote;
            TickAdvancedBy += line.DequeueNote;
        }

        foreach (var note in ChartManager.Instance.Notes)
        {
            Lines[note.Line].AddNote(note);
        }
    }

    public IEnumerator Play()
    {
        timer.Restart();

        while (!IsEnd)
        {
            TickAdvancedBy?.Invoke(CurrentTick);
            yield return null;
        }

        timer.Stop();
        OnPlayEnd?.Invoke();
    }

    public void OnCilck(int line)
    {
        Lines[line].Click(CurrentTick);
    }

    public void OnCancel(int line)
    {
        Lines[line].Cancel(CurrentTick);
    }

    public class Line
    {
        private int currentNoteIdx = 0;

        private readonly List<Note> NoteList = new();

        private readonly Queue<Note> JudgeQueue = new();

        public event Action<JudgeResult> JudgeEvent;

        public bool IsEnd()
        {
            return JudgeQueue.Count == 0 &&
                   NoteList.Count <= currentNoteIdx;
        }

        public void AddNote(Note data)
        {
            NoteList.Add(data);
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

                if (currentNoteIdx < NoteList.Count &&
                    NoteList[currentNoteIdx].Type == NoteData.NoteType.LongTail)
                {
                    JudgeQueue.Enqueue(NoteList[currentNoteIdx]);
                    currentNoteIdx++;
                }
            }
        }

        public void DequeueNote(long currentTick)
        {
            while (JudgeQueue.TryPeek(out Note note))
            {
                if (note.Type == NoteData.NoteType.LongTail && note.StartTick <= currentTick)
                {
                    DequeueAndDisable();
                }
                else if (Judge(note, currentTick) == JudgeResult.Miss)
                {
                    JudgeEvent?.Invoke(JudgeResult.Miss);
                    DequeueAndDisable();
                }
                else
                {
                    break;
                }
            }
        }

        public void DequeueAndDisable()
        {
            if (JudgeQueue.TryDequeue(out Note note))
            {
                note.gameObject.SetActive(false);
            }
        }

        public void Click(long clickTick)
        {
            if (JudgeQueue.TryPeek(out Note note))
            {
                var judge = Judge(note, clickTick);
                if (judge != JudgeResult.None &&
                    (note.Type == NoteData.NoteType.Short ||
                     note.Type == NoteData.NoteType.Long))
                {
                    JudgeEvent?.Invoke(judge);
                    DequeueAndDisable();
                }
            }
        }

        public void Cancel(long cancelTick)
        {
            if (JudgeQueue.TryPeek(out Note note) && note.Type == NoteData.NoteType.LongTail)
            {
                if (note.StartTick > cancelTick + JUDGE_BAD)
                {
                    JudgeEvent?.Invoke(JudgeResult.Miss);
                    DequeueAndDisable();
                }
            }
        }

        public static JudgeResult Judge(Note note, long clickTick) =>
            Judge(note.StartTick, clickTick);

        public static JudgeResult Judge(long noteTick, long clickTick)
        {
            long diff = clickTick - noteTick;
            long absDiff = diff < 0 ? -diff : diff;

            if (absDiff < JUDGE_PERFECT)
            {
                return JudgeResult.Perfect;
            }

            if (absDiff < JUDGE_GOOD)
            {
                return diff < 0 ? JudgeResult.Early : JudgeResult.Late;
            }

            if (absDiff < JUDGE_BAD)
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