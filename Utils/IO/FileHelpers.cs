namespace Utils.IO;

public abstract class FileHelpers
{
    public static string GetNullTerminatedString(string buffer, int offset)
    {
        if (offset < 0 || offset >= buffer.Length)
            return string.Empty;

        var end = buffer.IndexOf(value: '\0', offset);
        if (end == -1)
            end = buffer.Length;

        return buffer.Substring(offset, end - offset);
    }

    public static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "unnamed";

        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, newChar: '_'));
    }
}