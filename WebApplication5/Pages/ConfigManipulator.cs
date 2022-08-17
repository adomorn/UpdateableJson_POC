using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration.Json;

namespace WebApplication5;

public static class ConfigManipulator
{
    public const string JsonAlternateName = ".Updatable.json";
    public static IEnumerable<string> GetJsonFiles(IConfigurationRoot root)
    {
        return root.Providers.OfType<JsonConfigurationProvider>().Select(x =>
            x.Source.FileProvider
                .GetFileInfo(x.Source.Path).PhysicalPath).Where(x => !x.EndsWith(JsonAlternateName)).ToList();
    }

    public static SortedDictionary<string, string> GetValues(IConfigurationRoot root, string filePath)
    {
        var updatablePath = GetUpdatableFilePath(filePath);
        var baseProvider = root.Providers.OfType<JsonConfigurationProvider>().FirstOrDefault(x =>
            x.Source.FileProvider.GetFileInfo(x.Source.Path).PhysicalPath == filePath);
        var updatableProvider = root.Providers.OfType<JsonConfigurationProvider>().FirstOrDefault(x =>
            x.Source.FileProvider.GetFileInfo(x.Source.Path).PhysicalPath == updatablePath);
        var baseValues = ReadValuesFromProvider(baseProvider);
        var updatableValues = ReadValuesFromProvider(updatableProvider);

        return new SortedDictionary<string, string>(MapValues(baseValues, updatableValues));
    }

    public static string GetUpdatableFilePath(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var updatablePath = Path.Combine(dir, $"{fileName}{JsonAlternateName}");
        return updatablePath;
    }

    private static void SetJsonVal(JsonNode node, object index, object value)
    {
        switch (index)
        {
            case int intindex:
                node[intindex] = JsonValue.Create(value);
                break;
            case string stringIndex:
                node[stringIndex] = JsonValue.Create(value);
                break;
        }
    }

    private static JsonNode GetJsonVal(JsonNode node, object index)
    {
        return index switch
        {
            int intindex => node[intindex],
            string stringIndex => node[stringIndex],
            _ => null
        };
    }

    public static bool UpdateValues(Dictionary<string, string> values, string configFile)
    {
        var jsonString = File.ReadAllText(configFile);
        var original = JsonNode.Parse(jsonString);

        //obj["Logging"]["LogLevel"]["Microsoft.AspNetCore"] = "hede";
        foreach (var value in values)
        {
            var f = original;
            var keyValues = value.Key.Split(":", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var val) ? val : (object) x).ToList();
            for (var index = 0; index < keyValues.Count - 1; index++)
            {
                var keyValue = keyValues[index];
                f = GetJsonVal(f, keyValue);
            }

            var last = keyValues.Last();
            if (GetJsonVal(f, last) == null)
                continue;
            var obj = GetJsonVal(f, last).AsValue();
            var elem = obj.GetValue<JsonElement>();
            var kind = elem.ValueKind;
            switch (kind)
            {
                case JsonValueKind.String:
                    SetJsonVal(f, last, value.Value);
                    break;
                case JsonValueKind.Number:
                    SetJsonVal(f, last, decimal.Parse(value.Value));
                    break;
                case JsonValueKind.True:  
                case JsonValueKind.False:
                    SetJsonVal(f, last, bool.Parse(value.Value));
                    break;
                case JsonValueKind.Null:
                    break;
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var json = original.ToJsonString(new JsonSerializerOptions {WriteIndented = true});
        SaveToFile(configFile, json);
        return true;
    }

    private static bool SaveToFile(string configFile, string json)
    {
        try
        {
            var fileName = GetUpdatableFilePath(configFile);
            File.WriteAllText(fileName, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Dictionary<string, string> MapValues(Dictionary<string, string> baseValues,
        Dictionary<string, string> updatableValues)
    {
        foreach (var updatableValue in updatableValues.Where(updatableValue =>
                     baseValues.ContainsKey(updatableValue.Key)))
        {
            baseValues[updatableValue.Key] = updatableValue.Value;
        }

        return baseValues;
    }

    private static Dictionary<string, string> ReadValuesFromProvider(JsonConfigurationProvider provider)
    {
        var dataElement = provider.GetType()
            .GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic);

        return (Dictionary<string, string>) dataElement.GetValue(provider);
    }
}