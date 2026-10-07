using System.Collections.Generic;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;
using SimpleTweaksPlugin.TweakSystem;
using SimpleTweaksPlugin.Utility;

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment;

[TweakCategory(TweakCategory.UI)]
[TweakName("Keep Windows Open")]
[TweakDescription("Prevents certain windows from hiding under specific circumstances.")]
[TweakReleaseVersion("1.8.2.1")]
[TweakAutoConfig]
[Changelog("1.10.3.1", "Added option for 'Raw Material List'")]
public unsafe class KeepOpen : Tweak {
    private delegate void* HideUnitBase(AtkUnitBase* atkUnitBase, byte a2, byte a3, int a4);

    [TweakHook, Signature("E8 ?? ?? ?? ?? 32 DB 0F B6 D3", DetourName = nameof(HideDetour))]
    private HookWrapper<HideUnitBase> hideHook;

    public class Configs : TweakConfig {
        public SortedSet<string> EnabledWindows = new();
    }

    public Configs Config { get; private set; }

    protected void DrawConfig(ref bool hasChanged) {
        ImGui.TextWrapped(Loc.Text("Windows to keep open when accessing retainers, shops or market board:"));
        foreach (var (internalName, displayName) in windows) {
            var enabled = Config.EnabledWindows.Contains(internalName);

            if (ImGui.Checkbox($"{Loc.Text(displayName.ToString())}###keepOpenWindow_{internalName}", ref enabled)) {
                if (enabled) {
                    Config.EnabledWindows.Add(internalName);
                } else {
                    Config.EnabledWindows.Remove(internalName);
                }

                hasChanged = true;
            }
        }
    }

    private record WindowInfo(string DisplayName, bool UseReopenMethod = false) {
        public static implicit operator WindowInfo(string displayName) => new(displayName);
        public override string ToString() => DisplayName;
    };

    private readonly Dictionary<string, WindowInfo> windows = new() {
        ["ItemFinder"] = "アイテム検索結果",
        ["RecipeNote"] = "製作手帳",
        ["RecipeTree"] = "レシピツリー",
        ["ContentsInfoDetail"] = "コンテンツ情報の詳細",
        ["ContentsInfo"] = "コンテンツ情報",
        ["RecipeMaterialList"] = "素材一覧",
        ["Character"] = new("キャラクター画面", true),
    };

    private void* HideDetour(AtkUnitBase* atkUnitBase, byte a2, byte a3, int a4) {
        var doReopen = false;
        try {
            if (atkUnitBase != null && a2 == 1 && a3 == 0 && a4 == 2) {
                var name = atkUnitBase->NameString;

                if (windows.TryGetValue(name, out var windowInfo)) {
                    if (Config.EnabledWindows.Contains(name)) {
                        if (windowInfo.UseReopenMethod) {
                            doReopen = true;
                            SimpleLog.Log($"Attempting Reopen: {name}");
                            return hideHook.Original(atkUnitBase, a2, a3, a4);
                        }

                        SimpleLog.Log($"Suppress Hide: {name}");
                        return null;
                    }
                } else {
                    SimpleLog.Verbose($"Allowed Hiding: {name}");
                }
            }

            return hideHook.Original(atkUnitBase, a2, a3, a4);
        } catch {
            doReopen = false;
            return hideHook.Original(atkUnitBase, a2, a3, a4);
        } finally {
            if (doReopen) {
                atkUnitBase->Show(false, 0);
            }
        }
    }
}
