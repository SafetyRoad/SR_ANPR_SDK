using System.Runtime.InteropServices;

namespace Titan.Anpr.NativeInterop;

/// <summary>
/// Locates and loads the native Titan-ANPR DLL before P/Invoke runs.
/// </summary>
/// <remarks>
/// <para>
/// Resolution order:
/// </para>
/// <list type="number">
/// <item><description><see cref="BinDirectoryOverride"/> — optional path set by the host app (e.g. sample <c>Main</c>).</description></item>
/// <item><description><c>TITAN_ANPR_NATIVE_DIR</c> — folder that contains <c>Titan-ANPR.dll</c> (environment override).</description></item>
/// <item><description>Repository layout — walk upward from <see cref="AppContext.BaseDirectory"/> and use the first match for
/// <c>bin/Titan-ANPR/Release/x64/bin</c> or <c>bin/Titan-ANPR/Debug/x64/bin</c> (matches the redistributable tree under the SDK repo).</description></item>
/// <item><description>Application directory — <c>Titan-ANPR.dll</c> next to the managed assembly.</description></item>
/// </list>
/// </remarks>
public static class NativeDll
{
    public const string DllName = "Titan-ANPR.dll";

    /// <summary>
    /// Optional folder that contains <see cref="DllName"/>. Set by the application before <see cref="EnsureLoaded"/>.
    /// Takes precedence over <c>TITAN_ANPR_NATIVE_DIR</c> and automatic discovery.
    /// </summary>
    public static string? BinDirectoryOverride { get; set; }

    private static readonly string? NativeDirectoryOverride =
        Environment.GetEnvironmentVariable("TITAN_ANPR_NATIVE_DIR");

    /// <summary>
    /// Resolves the full path to <see cref="DllName"/> and loads it so missing-DLL errors surface before the first P/Invoke call.
    /// </summary>
    /// <exception cref="DllNotFoundException">Thrown when the native library cannot be located.</exception>
    public static void EnsureLoaded()
    {
        var path = ResolveDllPathOrThrow();
        NativeLibrary.Load(path);
    }

    private static string ResolveDllPathOrThrow()
    {
        var path = TryResolveDllPath();
        if (path != null)
            return path;

        var baseDir = AppContext.BaseDirectory;
        throw new DllNotFoundException(
            $"Could not find {DllName}. Set {nameof(BinDirectoryOverride)} in Main, TITAN_ANPR_NATIVE_DIR, place the DLL next to the app ({baseDir}), " +
            "or build the native SDK under bin/Titan-ANPR/{{Release|Debug}}/x64/bin relative to the repository root.");
    }

    private static string? TryResolveDllPath()
    {
        if (!string.IsNullOrWhiteSpace(BinDirectoryOverride))
        {
            if (TryPath(Path.Combine(BinDirectoryOverride.Trim(), DllName), out var fromCode))
                return fromCode;
        }

        if (!string.IsNullOrWhiteSpace(NativeDirectoryOverride))
        {
            if (TryPath(Path.Combine(NativeDirectoryOverride.Trim(), DllName), out var fromEnv))
                return fromEnv;
        }

        if (TryFindInRepositoryLayout(out var fromRepo))
            return fromRepo;

        if (TryPath(Path.Combine(AppContext.BaseDirectory, DllName), out var fromBase))
            return fromBase;

        return null;
    }

    private static bool TryPath(string candidate, out string fullPath)
    {
        fullPath = "";
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        try
        {
            var resolved = Path.GetFullPath(candidate);
            if (File.Exists(resolved))
            {
                fullPath = resolved;
                return true;
            }
        }
        catch (IOException)
        {
            // Invalid path characters, etc.
        }

        return false;
    }

    private static bool TryFindInRepositoryLayout(out string fullPath)
    {
        fullPath = "";
        var baseDir = AppContext.BaseDirectory;
        if (string.IsNullOrEmpty(baseDir))
            return false;

        static IEnumerable<string> LayoutBinFolders()
        {
            yield return Path.Combine("bin", "Titan-ANPR", "Release", "x64", "bin");
            yield return Path.Combine("bin", "Titan-ANPR", "Debug", "x64", "bin");
        }

        try
        {
            var dir = new DirectoryInfo(baseDir);
            for (var i = 0; i < 16 && dir != null; i++)
            {
                foreach (var relative in LayoutBinFolders())
                {
                    var folder = Path.Combine(dir.FullName, relative);
                    var dll = Path.Combine(folder, DllName);
                    if (File.Exists(dll))
                    {
                        fullPath = dll;
                        return true;
                    }
                }

                dir = dir.Parent;
            }
        }
        catch (IOException)
        {
            return false;
        }

        return false;
    }
}
