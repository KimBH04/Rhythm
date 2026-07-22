using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using UnityEngine;

[Serializable]
public class NoteData : IComparable<NoteData>
{
    public GreatFraction StartPos { get; private set; }

    public GreatFraction EndPos   { get; private set; }

    public int Line { get; private set; }

    public NoteType Type { get; private set; }

    public NoteData(
        int measure,
        int numerator,
        int denominator,
        int endMeasure,
        int endNumerator,
        int endDenominator,
        int line,
        NoteType type
    )
    {
        StartPos = new(measure, numerator, denominator);

        EndPos = new(endMeasure, endNumerator, endDenominator);

        Line = line;
        Type = type;
    }

    [JsonConstructor]
    public NoteData(
        GreatFraction startPos,
        GreatFraction endPos,
        int line,
        NoteType type
    )
    {
        StartPos = startPos;
        EndPos   = endPos;
        Line     = line;
        Type     = type;
    }

    public int CompareTo(NoteData other)
    {
        return StartPos.CompareTo(other.StartPos);
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

    public Fraction TimeSignature { get; private set; }

    public List<NoteData> Notes { get; private set; }

    [JsonConstructor]
    public ChartData(string title, int bpm, Fraction timeSignature, List<NoteData> notes)
    {
        Title = title;
        BPM = bpm;
        TimeSignature = timeSignature;
        Notes = notes.ToList();
    }

    public ChartData(string title, int bpm, Fraction timeSignature, IEnumerable<NoteData> notes)
    {
        Title = title;
        BPM = bpm;
        TimeSignature = timeSignature;
        Notes = notes.ToList();
    }
}