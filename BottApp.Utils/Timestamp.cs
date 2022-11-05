using System;

namespace BottApp.Utils;

public struct Timestamp
{
    public readonly int Value;


    public static Timestamp Now => new Timestamp(DateTime.UtcNow.ToUnixTimestamp());

    public static Timestamp Empty = new Timestamp(0);


    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int SecondsInDay = 3600 * 24;


    public Timestamp(int value)
    {
        Value = value;
    }


    public Timestamp(DateTime date)
        : this((int) date.ToUniversalTime().Subtract(UnixEpoch).TotalSeconds)
    {
    }


    public DateTime ToUtc()
    {
        return UnixEpoch.AddSeconds(Value);
    }


    public static implicit operator int(Timestamp timestamp)
    {
        return timestamp.Value;
    }


    public Timestamp WithAddedDays(int days)
    {
        return new Timestamp(Value + (days * SecondsInDay));
    }


    public static Timestamp operator +(Timestamp timestamp, int seconds)
    {
        return new Timestamp(timestamp.Value + seconds);
    }
}