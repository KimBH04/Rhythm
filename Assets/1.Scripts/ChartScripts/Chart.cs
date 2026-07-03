using System;
using System.Text.Json.Serialization;
using UnityEngine;

[Serializable]
public struct NoteData
{
    public ulong Tick { get; private set; }

    public byte Line { get; private set; }

    public NoteType Type { get; private set; }

    public ulong Length { get; private set; }

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
}