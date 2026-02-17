using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GOC.UISystem.Editor
{
    [System.Serializable]
    public class RegistryEntry
    {
        public string name;
        public int index;
    }

    [System.Serializable]
    public class RegistryData
    {
        public List<RegistryEntry> entries = new List<RegistryEntry>();
    }

    public static class EnumCodeGenerator
    {
        public static RegistryData LoadRegistry(string registryPath)
        {
            if (!File.Exists(registryPath))
                return new RegistryData();

            var json = File.ReadAllText(registryPath);
            return JsonUtility.FromJson<RegistryData>(json);
        }

        public static void SaveRegistry(string registryPath, RegistryData data)
        {
            var dir = Path.GetDirectoryName(registryPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(registryPath, json);
        }

        public static int AddEntry(RegistryData data, string name)
        {
            for (int i = 0; i < data.entries.Count; i++)
            {
                if (data.entries[i].name == name)
                    return data.entries[i].index;
            }

            int maxIndex = 0;
            for (int i = 0; i < data.entries.Count; i++)
            {
                if (data.entries[i].index > maxIndex)
                    maxIndex = data.entries[i].index;
            }

            var entry = new RegistryEntry { name = name, index = maxIndex + 1 };
            data.entries.Add(entry);
            return entry.index;
        }

        public static bool HasEntry(RegistryData data, string name)
        {
            for (int i = 0; i < data.entries.Count; i++)
            {
                if (data.entries[i].name == name)
                    return true;
            }
            return false;
        }

        public static void GenerateScreenIds(string outputPath, string gameNamespace, RegistryData data)
        {
            var enumEntries = new StringBuilder();
            var staticEntries = new StringBuilder();
            var switchEntries = new StringBuilder();

            for (int i = 0; i < data.entries.Count; i++)
            {
                var entry = data.entries[i];
                enumEntries.AppendLine($"        {entry.name} = {entry.index},");
                staticEntries.AppendLine($"        public static readonly ScreenId {entry.name} = new ScreenId(\"{entry.name}\");");
                switchEntries.AppendLine($"            ScreenIdEnum.{entry.name} => ScreenIds.{entry.name},");
            }

            var code = TemplateStrings.ScreenIdsTemplate
                .Replace("{NAMESPACE}", gameNamespace)
                .Replace("{ENUM_ENTRIES}", enumEntries.ToString().TrimEnd())
                .Replace("{STATIC_ENTRIES}", staticEntries.ToString().TrimEnd())
                .Replace("{SWITCH_ENTRIES}", switchEntries.ToString().TrimEnd());

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(outputPath, code);
        }

        public static void GeneratePopupIds(string outputPath, string gameNamespace, RegistryData data)
        {
            var enumEntries = new StringBuilder();
            var staticEntries = new StringBuilder();
            var switchEntries = new StringBuilder();

            for (int i = 0; i < data.entries.Count; i++)
            {
                var entry = data.entries[i];
                enumEntries.AppendLine($"        {entry.name} = {entry.index},");
                staticEntries.AppendLine($"        public static readonly PopupId {entry.name} = new PopupId(\"{entry.name}\");");
                switchEntries.AppendLine($"            PopupIdEnum.{entry.name} => PopupIds.{entry.name},");
            }

            var code = TemplateStrings.PopupIdsTemplate
                .Replace("{NAMESPACE}", gameNamespace)
                .Replace("{ENUM_ENTRIES}", enumEntries.ToString().TrimEnd())
                .Replace("{STATIC_ENTRIES}", staticEntries.ToString().TrimEnd())
                .Replace("{SWITCH_ENTRIES}", switchEntries.ToString().TrimEnd());

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(outputPath, code);
        }
    }
}
