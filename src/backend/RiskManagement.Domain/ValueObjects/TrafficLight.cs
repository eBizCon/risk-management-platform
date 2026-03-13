namespace RiskManagement.Domain.ValueObjects;

public sealed class TrafficLight : IEquatable<TrafficLight>
{
    public static readonly TrafficLight Red = new("red");
    public static readonly TrafficLight Yellow = new("yellow");
    public static readonly TrafficLight Green = new("green");

    private static readonly Dictionary<string, TrafficLight> All = new()
    {
        [Red.Value] = Red,
        [Yellow.Value] = Yellow,
        [Green.Value] = Green
    };

    public string Value { get; }

    private TrafficLight(string value)
    {
        Value = value;
    }

    public static TrafficLight From(string value)
    {
        if (All.TryGetValue(value, out var light))
            return light;

        throw new ArgumentException($"Invalid TrafficLight: '{value}'", nameof(value));
    }

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(TrafficLight? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is TrafficLight other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(TrafficLight? left, TrafficLight? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(TrafficLight? left, TrafficLight? right)
    {
        return !(left == right);
    }
}