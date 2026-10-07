using System;
using System.IO;
using System.Linq;
using System.Text;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using SimpleTweaksPlugin.TweakSystem;
using SimpleTweaksPlugin.Utility;

namespace SimpleTweaksPlugin.Tweaks;

[TweakName("Screenshot File Name")]
[TweakDescription("Change the file name format for screenshots.")]
[TweakAutoConfig]
[Changelog("1.10.2.0", "Added ability to use folders in screenshot path.")]
[Changelog("1.10.2.0", "Added ability to use character name and location in screenshot path.")]
[Changelog("1.10.5.0", "Added option to use milliseconds in name template.")]
public unsafe class ScreenshotFileName : Tweak {
    public class Configs : TweakConfig {
        public string DateFormatString = "ffxiv_%Y-%m-%d_%H%M%S";
    }

    [TweakConfig] public Configs Config { get; private set; }

    protected void DrawConfig() {
        ImGui.InputText(Loc.Ui("Format"), ref Config.DateFormatString, 512);

        using (ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled))) {
            ImGui.TextUnformatted(GetScreenshotName());
        }

        if (!ImGui.CollapsingHeader(Loc.Ui("Placeholders"))) return;
        if (ImGui.BeginTable("Placeholders", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.NoSavedSettings)) {
            ImGui.TableSetupColumn(Loc.Ui("Placeholder"), ImGuiTableColumnFlags.WidthFixed, 80 * ImGui.GetIO().FontGlobalScale);
            ImGui.TableSetupColumn(Loc.Ui("Value"), ImGuiTableColumnFlags.WidthFixed, 140 * ImGui.GetIO().FontGlobalScale);
            ImGui.TableSetupColumn(Loc.Ui("Description"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();
            foreach (var ph in ValidPlaceholderFormats) {
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"%{ph.Placeholder}");
                if (ImGui.IsItemClicked()) ImGui.SetClipboardText($"%{ph.Placeholder}");
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(Loc.Text("Click to Copy"));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{ph.GetValue()}");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(Loc.Text($"{ph.GetDescription()}"));
            }

            ImGui.EndTable();
        }
    }

    private delegate char* GetPath(char* destination, byte* p);

    [TweakHook, Signature("E8 ?? ?? ?? ?? 48 8B C7 49 8D 9E", DetourName = nameof(GetPathDetour))]
    private HookWrapper<GetPath> getPathHook;

    private string GetScreenshotName() {
        var str = $"{Config.DateFormatString}";
        foreach (var ph in ValidPlaceholderFormats) {
            if (str.Contains($"%{ph.Placeholder}")) str = str.Replace($"%{ph.Placeholder}", ph.GetValue());
        }

        return string.Join('/', str.Split('/', '\\').Select(s => string.Join('_', s.Split(Path.GetInvalidFileNameChars())))).Trim('/').Trim();
    }

    private char* GetPathDetour(char* destination, byte* p) {
        try {
            var pStr = MemoryHelper.ReadString((nint)p, 64);
            if (pStr.StartsWith("ffxiv_") && (pStr.EndsWith(".png") || pStr.EndsWith(".jpg") || pStr.EndsWith(".bmp"))) {
                var newName = $"{GetScreenshotName()}.{pStr.Split('.').Last()}";

                var bytes = Encoding.UTF8.GetBytes(newName);
                var b = stackalloc byte[bytes.Length + 1];
                for (var byteIndex = 0; byteIndex < bytes.Length; byteIndex++) b[byteIndex] = bytes[byteIndex];
                b[bytes.Length] = 0;
                var o = getPathHook.Original(destination, b);
                var str = string.Empty;
                var i = 0;
                while (o[i] != '\0') {
                    str += o[i++];
                }

                var fileInfo = new FileInfo(str);
                if (fileInfo.Exists) {
                    Service.Chat.PrintError($"同じ名前のスクリーンショットが存在します：{str}");
                }

                fileInfo.Directory?.Create();

                return o;
            }
        } catch (Exception ex) {
            SimpleLog.Error(ex);
        }

        return getPathHook.Original(destination, p);
    }

    private record PlaceholderFormat(string Placeholder, Func<string> GetDescription, Func<string> GetValue);

    private static readonly PlaceholderFormat[] ValidPlaceholderFormats = [
        new("Y", () => "西暦4桁", () => DateTime.Now.ToString("yyyy")),
        new("y", () => "西暦の下2桁", () => DateTime.Now.ToString("yy")),
        new("B", () => "月の名前", () => DateTime.Now.ToString("MMMM")),
        new("b", () => "月の名前（省略）", () => DateTime.Now.ToString("MMM")),
        new("m", () => "月（2桁）", () => DateTime.Now.ToString("MM")),
        new("d", () => "日（2桁）", () => DateTime.Now.ToString("dd")),
        new("j", () => "年内の通算日数（3桁）", () => (DateTime.Now.DayOfYear - 1).ToString("000")),
        new("A", () => "曜日の名前", () => DateTime.Now.ToString("dddd")),
        new("a", () => "曜日の名前（省略）", () => DateTime.Now.ToString("ddd")),
        new("H", () => "時（24時間制・2桁）", () => DateTime.Now.ToString("HH")),
        new("I", () => "時（12時間制・2桁）", () => DateTime.Now.ToString("hh")),
        new("p", () => "a.m. or p.m.", () => DateTime.Now.Hour < 12 ? "a.m." : "p.m."),
        new("M", () => "分（2桁）", () => DateTime.Now.ToString("mm")),
        new("S", () => "秒（2桁）", () => DateTime.Now.ToString("ss")),
        new("F", () => "ミリ秒（3桁）", () => DateTime.Now.Millisecond.ToString("D3")),
        new("ChrName", () => "現在のキャラクター名", () => UIState.Instance()->PlayerState.CharacterNameString),
        new("Location", () => "現在の場所の名前", () => Service.Data.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(Service.ClientState.TerritoryType)?.PlaceName.Value.Name.ExtractText() ?? "場所不明"),
    ];
}
