using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SimpleTweaksPlugin;

public class LocalizedString {
    [JsonProperty("message")] public string Message { get; set; } = string.Empty;
    [JsonProperty("description")] public string Description { get; set; } = string.Empty;
}

// Japanese edition: translations ship with the plugin and are never overwritten by Crowdin.
internal static class Loc {
    private static SortedDictionary<string, LocalizedString> _localizationStrings = new();
    private static readonly Dictionary<string, string> DisplayStrings = ReadResource<Dictionary<string, string>>("display-ja");

    private static T ReadResource<T>(string name) where T : new() {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SimpleTweaksPlugin.Localization.{name}.json")
            ?? throw new InvalidOperationException($"日本語の翻訳リソースが見つかりません：{name}");
        using var reader = new StreamReader(stream);
        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd()) ?? new T();
    }

    internal static void LoadLanguage(string langCode) {
        _localizationStrings = ReadResource<SortedDictionary<string, LocalizedString>>("ja");
    }

    internal static string Localize(string key, string fallbackValue, string? description = null) {
        return _localizationStrings.TryGetValue(key, out var entry) && !string.IsNullOrWhiteSpace(entry.Message)
            ? entry.Message : Text(fallbackValue);
    }

    // Only known plugin UI messages are translated. Commands, settings keys and game data stay intact.
    internal static string Text(string? value) {
        if (value == null) return string.Empty;
        return DisplayStrings.TryGetValue(value, out var translated) ? translated : value;
    }

    internal static string Ui(string label) {
        var split = label.IndexOf("##", StringComparison.Ordinal);
        var visible = split < 0 ? label : label[..split];
        var translated = Text(visible);
        if (translated == visible) return label;
        // Preserve explicit ### IDs; legacy labels get a stable ID based on their original English text.
        var explicitId = label.LastIndexOf("###", StringComparison.Ordinal);
        var id = explicitId < 0 ? label : label[(explicitId + 3)..];
        return translated + "###" + id;
    }

    internal static string ExportLoadedDictionary() => JsonConvert.SerializeObject(_localizationStrings, Formatting.Indented);
    internal static void ImportDictionary(string json) {
        try {
            _localizationStrings = JsonConvert.DeserializeObject<SortedDictionary<string, LocalizedString>>(json) ?? new();
        } catch (Exception ex) {
            SimpleLog.Error(ex);
        }
    }

    public static void ClearCache() => LoadLanguage("ja");
    public static void UpdateTranslations(bool force = false, Action? callback = null) {
        DownloadError = null;
        LoadLanguage("ja");
        if (callback != null) Service.Framework.RunOnTick(callback);
    }
    public static bool LoadingTranslations => false;
    public static Exception? DownloadError { get; private set; }
}
