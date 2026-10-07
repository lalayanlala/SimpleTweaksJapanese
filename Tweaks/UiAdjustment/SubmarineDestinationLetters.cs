using SimpleTweaksPlugin.TweakSystem;

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment;

[TweakName("Label Submarine Destinations with Letters")]
[TweakDescription("Uses the standard A-Z lettering to identify submarine destinations for easier use with other tools.")]
[TweakVersion(2)]
[Changelog("1.10.9.4", "Rewritten to fix issues, re-enabled tweak.")]
public unsafe class SubmarineDestinationLetters : UiAdjustments.SubTweak, IDisabledTweak {
    public string DisabledMessage => "パッチ7.4でゲーム標準の機能に追加されました。";
}
