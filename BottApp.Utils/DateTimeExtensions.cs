using System;

namespace BottApp.Utils;

public static class DateTimeExtensions
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    public static int ToUnix(this DateTime date)
    {
        return (int) date.Subtract(UnixEpoch).TotalSeconds;
    }


    public static int ToUnixTimestamp(this DateTime date)
    {
        return (int) date.ToUniversalTime().Subtract(UnixEpoch).TotalSeconds;
    }


    public static DateTime ToUtc(this int timestamp)
    {
        return UnixEpoch.AddSeconds(timestamp);
    }


    public static Timestamp ToTimestamp(this int time)
    {
        return new Timestamp(time);
    }
}