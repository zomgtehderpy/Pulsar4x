using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class WindowTitleHelper
{
    /// <summary>
    /// Prepends the calling class path and name to the provided title string when in debug mode.
    /// </summary>
    /// <param name="title">The original window title</param>
    /// <param name="callerFilePath">Automatically populated with caller's file path</param>
    /// <returns>Modified title string with class path in debug mode, original string in release mode</returns>
    public static string GetDebugWindowTitle(string title, [CallerFilePath] string callerFilePath = "")
    {
        #if DEBUG
            // Get the full path and convert to project relative path
            string projectPath = GetProjectPath();
            string relativePath = "";

            if (!string.IsNullOrEmpty(projectPath) && callerFilePath.StartsWith(projectPath))
            {
                // Remove project path and leading slash to get relative path
                relativePath = callerFilePath.Substring(projectPath.Length).TrimStart('\\', '/');
                // Remove the file extension
                relativePath = Path.ChangeExtension(relativePath, null);
                return $"[{relativePath}] {title}";
            }

            // Fallback to just filename if we can't get relative path
            string className = Path.GetFileNameWithoutExtension(callerFilePath);
            return $"[{className}] {title}";
        #else
            return title;
        #endif
    }

    /// <summary>
    /// Attempts to find the project root directory by looking for the .csproj file
    /// </summary>
    private static string GetProjectPath()
    {
        try
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                if (Directory.GetFiles(currentDirectory, "*.csproj").Length > 0)
                {
                    return currentDirectory;
                }
                currentDirectory = Path.GetDirectoryName(currentDirectory);
            }
        }
        catch
        {
            // Silently fail and let the calling method handle the empty result
        }
        return string.Empty;
    }
}