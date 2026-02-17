using System.IO;

namespace GOC.UISystem.Editor
{
    public static class PopupCodeGenerator
    {
        public static void Generate(string outputPath, string name, string gameNamespace, bool generateData)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string template = generateData
                ? TemplateStrings.PopupWithDataTemplate
                : TemplateStrings.PopupTemplate;

            var code = template
                .Replace("{NAME}", name)
                .Replace("{NAMESPACE}", gameNamespace);

            File.WriteAllText(outputPath, code);
        }

        public static void GenerateData(string outputPath, string name, string gameNamespace)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var code = TemplateStrings.PopupDataTemplate
                .Replace("{NAME}", name)
                .Replace("{NAMESPACE}", gameNamespace);

            File.WriteAllText(outputPath, code);
        }
    }
}
