using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class SaveUtils
{
    // ---------- Primitives ----------

    public static string Int(int value) => value.ToString();
    public static int ToInt(string value) => int.Parse(value);

    public static string Float(float value) =>
        value.ToString(CultureInfo.InvariantCulture);

    public static float ToFloat(string value) =>
        float.Parse(value, CultureInfo.InvariantCulture);

    public static string Bool(bool value) => value ? "1" : "0";
    public static bool ToBool(string value) => value == "1";

    public static string String(string value) => value ?? "";

    // ---------- Enums ----------
    public static string EnumToString<T>(T value) where T : Enum =>
        value.ToString();

    public static T StringToEnum<T>(string value) where T : Enum =>
        (T)System.Enum.Parse(typeof(T), value);
    // ---------- Lists ----------

    private const char ListSeparator = '|';

    public static string StringList(List<string> list)
    {
        if (list == null || list.Count == 0)
            return "";

        return string.Join(ListSeparator, list);
    }

    public static List<string> ToStringList(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new List<string>();

        return value.Split(ListSeparator).ToList();
    }
}

// Type     -   Save            -   Restore

// Enum     -   ToString()      -   Enum.Parse / ToEnum<T>()
// int      -   ToString()      -   int.Parse / ToInt()
// float    -   ToString()      -   float.Parse / ToFloat()
// bool     -   ToString()      -   bool.Parse / ToBool()
// List     -   string.Join()   -   Split()

