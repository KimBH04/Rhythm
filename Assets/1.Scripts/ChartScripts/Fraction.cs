using System;
using System.Text.Json.Serialization;

[Serializable]
public struct Fraction : IComparable<Fraction>
{
    private int numerator;
    private int denominator;

    public int Numerator
    {
        readonly get => numerator;
        set => numerator = value;
    }

    public int Denominator
    {
        readonly get => denominator;
        set => denominator = value != 0 ? value :
            throw new ArgumentException(nameof(value) + " is ZERO.");
    }

    [JsonConstructor]
    public Fraction(int numerator, int denominator)
    {
        if (denominator == 0)
            throw new ArgumentException(nameof(denominator) + " is ZERO.");

        this.numerator   = numerator;
        this.denominator = denominator;
    }

    public readonly double GetDouble()
    {
        return (double)Numerator / Denominator;
    }

    public readonly int CompareTo(Fraction other)
    {
        return GetDouble().CompareTo(other.GetDouble());
    }

    private static int GCD(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a == 0 ? 1 : a;
    }

    public readonly Fraction Reduce()
    {
        int g   = GCD(Numerator, Denominator);
        int num = Numerator   / g;
        int den = Denominator / g;
        if (den < 0)
        {
            num = -num;
            den = -den;
        }
        return new Fraction(num, den);
    }

    public static Fraction operator +(Fraction a, Fraction b)
    {
        checked
        {
            return new Fraction(
                a.Numerator   * b.Denominator + b.Numerator * a.Denominator,
                a.Denominator * b.Denominator).Reduce();
        }
    }

    public static Fraction operator -(Fraction a, Fraction b)
    {
        checked
        {
            return new Fraction(
                a.Numerator   * b.Denominator - b.Numerator * a.Denominator,
                a.Denominator * b.Denominator).Reduce();
        }
    }

    public static Fraction operator *(Fraction a, Fraction b)
    {
        checked
        {
            return new Fraction(
                a.Numerator   * b.Numerator,
                a.Denominator * b.Denominator).Reduce();
        }
    }

    public static Fraction operator /(Fraction a, Fraction b)
    {
        if (b.Numerator == 0)
            throw new DivideByZeroException(nameof(b) + " numerator is ZERO.");

        checked
        {
            return new Fraction(
                a.Numerator   * b.Denominator,
                a.Denominator * b.Numerator).Reduce();
        }
    }

    public static Fraction operator -(Fraction a)
    {
        checked
        {
            return new Fraction(-a.Numerator, a.Denominator);
        }
    }

    public readonly Fraction Add     (Fraction other) => this + other;
    public readonly Fraction Subtract(Fraction other) => this - other;
    public readonly Fraction Multiply(Fraction other) => this * other;
    public readonly Fraction Divide  (Fraction other) => this / other;

    public static bool operator ==(Fraction a, Fraction b) => a.CompareTo(b) == 0;
    public static bool operator !=(Fraction a, Fraction b) => a.CompareTo(b) != 0;
    public static bool operator < (Fraction a, Fraction b) => a.CompareTo(b) <  0;
    public static bool operator > (Fraction a, Fraction b) => a.CompareTo(b) >  0;
    public static bool operator <=(Fraction a, Fraction b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Fraction a, Fraction b) => a.CompareTo(b) >= 0;

    public override readonly bool Equals(object obj) =>
        obj is Fraction other && CompareTo(other) == 0;

    public override readonly int GetHashCode() => GetDouble().GetHashCode();

    public override readonly string ToString() => $"{Numerator} / {Denominator}";
}

[Serializable]
public struct GreatFraction : IComparable<GreatFraction>
{
    private int measure;
    private Fraction fraction;

    public int Measure
    {
        readonly get => measure;
        set => measure = value;
    }

    [JsonIgnore]
    public int Numerator
    {
        readonly get => fraction.Numerator;
        set => fraction.Numerator = value;
    }

    [JsonIgnore]
    public int Denominator
    {
        readonly get => fraction.Denominator;
        set => fraction.Denominator = value != 0 ? value :
            throw new InvalidOperationException(nameof(value) + " is ZERO");
    }
    
    public Fraction Fraction
    {
        readonly get => fraction;
        set => fraction = value;
    }

    [JsonConstructor]
    public GreatFraction(int measure, Fraction fraction)
    {
        this.measure  = measure;
        this.fraction = fraction;
    }

    public GreatFraction(int measure, int numerator, int denominator)
    {
        this.measure = measure;
        fraction     = new(numerator, denominator);
    }

    public readonly double GetDouble()
    {
        return Fraction.GetDouble() + Measure;
    }

    public readonly int CompareTo(GreatFraction other)
    {
        return GetDouble().CompareTo(other.GetDouble());
    }

    public readonly Fraction ToFraction()
    {
        checked
        {
            return new Fraction(Measure * Denominator + Numerator, Denominator);
        }
    }

    public static GreatFraction FromFraction(Fraction fraction)
    {
        Fraction reduced = fraction.Reduce();
        int measure   = reduced.Numerator / reduced.Denominator;
        int remainder = reduced.Numerator % reduced.Denominator;

        if (remainder < 0)
        {
            remainder += Math.Abs(reduced.Denominator);
            measure -= 1;
        }

        return new GreatFraction(measure, remainder, reduced.Denominator);
    }

    public static GreatFraction operator +(GreatFraction a, GreatFraction b)
    {
        return FromFraction(a.ToFraction() + b.ToFraction());
    }

    public static GreatFraction operator -(GreatFraction a, GreatFraction b)
    {
        return FromFraction(a.ToFraction() - b.ToFraction());
    }

    public static GreatFraction operator *(GreatFraction a, GreatFraction b)
    {
        return FromFraction(a.ToFraction() * b.ToFraction());
    }

    public static GreatFraction operator /(GreatFraction a, GreatFraction b)
    {
        return FromFraction(a.ToFraction() / b.ToFraction());
    }

    public static GreatFraction operator -(GreatFraction a)
    {
        return FromFraction(-a.ToFraction());
    }

    public readonly GreatFraction Add     (GreatFraction other) => this + other;
    public readonly GreatFraction Subtract(GreatFraction other) => this - other;
    public readonly GreatFraction Multiply(GreatFraction other) => this * other;
    public readonly GreatFraction Divide  (GreatFraction other) => this / other;

    public static bool operator ==(GreatFraction a, GreatFraction b) => a.CompareTo(b) == 0;
    public static bool operator !=(GreatFraction a, GreatFraction b) => a.CompareTo(b) != 0;
    public static bool operator < (GreatFraction a, GreatFraction b) => a.CompareTo(b) <  0;
    public static bool operator > (GreatFraction a, GreatFraction b) => a.CompareTo(b) >  0;
    public static bool operator <=(GreatFraction a, GreatFraction b) => a.CompareTo(b) <= 0;
    public static bool operator >=(GreatFraction a, GreatFraction b) => a.CompareTo(b) >= 0;

    public override readonly bool Equals(object obj) =>
        obj is GreatFraction other && CompareTo(other) == 0;

    public override readonly int GetHashCode() => GetDouble().GetHashCode();

    public override readonly string ToString() => Measure == 0
        ? Fraction.ToString()
        : $"{Measure} {Math.Abs(Numerator)} / {Math.Abs(Denominator)}";
}