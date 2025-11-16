namespace RP6_UnpackCLI;

public class Options
{
    private const string IniFileHeader = "; RP6_UnpackCLI options\n[Fixups]\n";
    private const string KeyConvert = "EnablePngFixup";
    private const string KeyDumpRaw = "EnableRawDumping";

    public bool EnablePngFixup { get; set; } = true;
    public bool EnableRawDumping { get; set; } = false;
    public static Options Current { get; private set; } = new Options();
    
    public static void LoadOrCreate(string path)
    {
        var opts = new Options();

        try
        {
            if (!File.Exists(path))
            {
                // write defaults
                opts.Save(path);
                Current = opts;
                Console.WriteLine($"[INFO] Created default options INI at: {path}");
                return;
            }

            var text = File.ReadAllText(path);
            var parsed = ParseIni(text, opts);
            Current = parsed;

            // Write missing defaults if keys are missing
            bool missingKey = false;
            if (!text.Contains(KeyConvert, StringComparison.OrdinalIgnoreCase))
            {
                Current.EnablePngFixup = opts.EnablePngFixup;
                missingKey = true;
            }
            if (!text.Contains(KeyDumpRaw, StringComparison.OrdinalIgnoreCase))
            {
                Current.EnableRawDumping = opts.EnableRawDumping;
                missingKey = true;
            }

            if (!missingKey)
                return;

            Current.Save(path);
            Console.WriteLine($"[INFO] Wrote missing default option(s) to: {path}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Could not read options INI ({path}): {ex.Message}. Using defaults.");
            try
            {
                opts.Save(path); Console.WriteLine($"[INFO] Saved default options INI at: {path}");
            } catch { /* idk */ }
            Current = opts;
        }
    }

    private static Options ParseIni(string content, Options defaults)
    {
        if (string.IsNullOrWhiteSpace(content))
            return defaults;
        
        var res = new Options
        {
            EnablePngFixup = defaults.EnablePngFixup,
            EnableRawDumping = defaults.EnableRawDumping
        };

        using var sr = new StringReader(content);
        while (sr.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith(';') || line.StartsWith('#')) continue; // comment
            if (line.StartsWith('[')) continue; // section header
            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            var val = line[(idx + 1)..].Trim();

            if (string.Equals(key, KeyConvert, StringComparison.OrdinalIgnoreCase))
            {
                res.EnablePngFixup = TryParseBool(val, out var b) ? b : defaults.EnablePngFixup;
            }
            else if (string.Equals(key, KeyDumpRaw, StringComparison.OrdinalIgnoreCase))
            {
                res.EnableRawDumping = TryParseBool(val, out var b) ? b : defaults.EnableRawDumping;
            }
        }

        return res;
    }

    private static bool TryParseBool(string s, out bool value)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            value = true;
            return false;
        }

        s = s.Trim().ToLowerInvariant();
        switch (s)
        {
            case "1":
            case "true":
            case "yes":
            case "on":
                value = true; return true;
            case "0":
            case "false":
            case "no":
            case "off":
                value = false; return true;
            default:
                value = true;
                return false;
        }
    }
    private void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var sw = new StreamWriter(path, false);
        sw.WriteLine(IniFileHeader.TrimEnd());
        sw.WriteLine($"{KeyConvert}={EnablePngFixup.ToString().ToLowerInvariant()}");
        sw.WriteLine($"{KeyDumpRaw}={EnableRawDumping.ToString().ToLowerInvariant()}");
    }
}