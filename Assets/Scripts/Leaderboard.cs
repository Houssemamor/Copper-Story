using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Best completion times per level, stored on this device with PlayerPrefs.
// Lower time is better. Only the top MaxEntries times are kept.
public static class Leaderboard
{
    public const int MaxEntries = 5;
    private const string KeyPrefix = "Leaderboard_";

    public static List<float> GetTimes(string levelKey)
    {
        var times = new List<float>();
        string raw = PlayerPrefs.GetString(KeyPrefix + levelKey, "");
        if (string.IsNullOrEmpty(raw))
        {
            return times;
        }

        foreach (string part in raw.Split(';'))
        {
            if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                times.Add(value);
            }
        }

        times.Sort();
        if (times.Count > MaxEntries)
        {
            times.RemoveRange(MaxEntries, times.Count - MaxEntries);
        }
        return times;
    }

    // Saves a finished time. Returns the 0-based place it earned, or -1 if it missed the board.
    public static int Submit(string levelKey, float timeSeconds)
    {
        List<float> times = GetTimes(levelKey);
        times.Add(timeSeconds);
        times.Sort();

        int rank = times.IndexOf(timeSeconds);

        if (times.Count > MaxEntries)
        {
            times.RemoveRange(MaxEntries, times.Count - MaxEntries);
        }

        var parts = new List<string>();
        foreach (float t in times)
        {
            parts.Add(t.ToString("R", CultureInfo.InvariantCulture));
        }
        PlayerPrefs.SetString(KeyPrefix + levelKey, string.Join(";", parts));
        PlayerPrefs.Save();

        return rank >= 0 && rank < MaxEntries ? rank : -1;
    }

    // Formats seconds as MM:SS.hh, e.g. 01:23.45
    public static string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int minutes = (int)(seconds / 60f);
        float remainder = seconds - minutes * 60f;
        return minutes.ToString("00", CultureInfo.InvariantCulture)
             + ":" + remainder.ToString("00.00", CultureInfo.InvariantCulture);
    }
}
