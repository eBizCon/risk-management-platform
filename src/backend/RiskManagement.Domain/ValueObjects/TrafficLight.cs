using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.ValueObjects;

public sealed class TrafficLight : Enumeration<TrafficLight>
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

    private TrafficLight(string value) : base(value)
    {
    }

    public static TrafficLight From(string value)
    {
        return From(value, All);
    }
}