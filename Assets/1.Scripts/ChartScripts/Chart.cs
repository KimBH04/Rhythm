using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using UnityEngine;

[Serializable]
public struct NoteData : IComparable<NoteData>
{
    public long Tick { get; private set; }

    public int Line { get; private set; }

    public NoteType Type { get; private set; }

    public long Length { get; private set; }
    
    [JsonConstructor]
    public NoteData(long tick, int line, NoteType type, long length = 0)
    {
        Tick = tick;
        Line = line;
        Type = type;
        Length = length;
    }

    public readonly int CompareTo(NoteData other)
    {
        return Tick.CompareTo(other.Tick);
    }

    public enum NoteType
    {
        Short,
        Long,
        LongTail,
    }
}

[Serializable]
public class ChartData
{
    public string Title { get; private set; }

    public int BPM { get; private set; }

    public List<NoteData> Notes { get; private set; }

    [JsonIgnore]
    public AudioClip Clip { get; set; }

    [JsonConstructor]
    public ChartData(string title, int bpm, List<NoteData> notes)
    {
        Title = title;
        BPM = bpm;
        Notes = notes.ToList();
    }

    public ChartData(string title, int bpm, IEnumerable<NoteData> notes)
    {
        Title = title;
        BPM = bpm;
        Notes = notes.ToList();
    }
}