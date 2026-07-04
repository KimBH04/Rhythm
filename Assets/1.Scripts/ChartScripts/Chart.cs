using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using UnityEngine;

[Serializable]
public struct NoteData : IComparable<NoteData>
{
    public ulong Tick { get; private set; }

    public byte Line { get; private set; }

    public NoteType Type { get; private set; }

    public ulong Length { get; private set; }

    public NoteData(ulong tick, byte line, NoteType type, ulong length = 0)
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
    }
}

[Serializable]
public class ChartData
{
    public string Title { get; private set; }

    public int BPM { get; private set; }

    public NoteData[] Notes { get; private set;}

    [JsonIgnore]
    public AudioClip Clip { get; set; }

    public ChartData(string title, int bpm, IEnumerable<NoteData> notes)
    {
        Title = title;
        BPM = bpm;
        Notes = notes.ToArray();
    }
}