using OpenVTT.Models;

namespace OpenVTT.Services
{
    public static class ScriptParser
    {
        // A scene is any line starting with "# " or "## " (Markdown-style headings)
        public static List<Scene> ParseScenes(string script)
        {
            var scenes = new List<Scene>();
            int index = 0;
            using var reader = new StringReader(script);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("# ") || line.StartsWith("## "))
                {
                    string title = line.TrimStart('#', ' ').Trim();
                    scenes.Add(new Scene(title.Length > 0 ? title : "Untitled Scene", index));
                }
                index += line.Length + 1; // newline
            }
            if (scenes.Count == 0) scenes.Add(new Scene("Whole Script", 0));
            return scenes;
        }

        public static string GetSceneText(string script, Scene scene, Scene? next)
        {
            int start = Math.Clamp(scene.StartCharIndex, 0, script.Length);
            int end = next is null ? script.Length : Math.Clamp(next.StartCharIndex, start, script.Length);
            return script.Substring(start, end - start).Trim();
        }
    }
}
