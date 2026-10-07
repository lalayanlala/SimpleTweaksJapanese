using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using SimpleTweaksPlugin.Tweaks.AbstractTweaks;
using SimpleTweaksPlugin.TweakSystem;
using SimpleTweaksPlugin.Utility;

namespace SimpleTweaksPlugin.Tweaks;

[TweakName("House Lights Command")]
[TweakDescription("Adds a command to control lighting in your own housing areas.")]
[TweakReleaseVersion("1.8.2.0")]
[Changelog("1.10.5.0", "Fixed tweak not working.")]
[Changelog("1.10.8.4", "Added ability to toggle SSAO with 'ssao-on' and 'ssao-off' parameters")]
public unsafe class HouseLightCommand : CommandTweak {
    public override bool Experimental => true;
    protected override string Command => "lights";
    protected override string HelpMessage => $"現在のハウス・アパルトメントの照明を調整します。/{CustomOrDefaultCommand} (0-5) [ssao-on | ssao-off] [save]";
    protected override bool ShowInHelp => true;

    private readonly string[] permanentMarkers = ["save"];

    protected override void OnCommand(string args) {
        var housingManager = HousingManager.Instance();

        if (!housingManager->IsInside()) {
            if (ShowCommandErrors) Service.Chat.PrintError("このコマンドはハウスまたはアパルトメント内で使用してください。");
            return;
        }
        
        if (!housingManager->HasHousePermissions()) {
            if (ShowCommandErrors) Service.Chat.PrintError("このハウス・アパルトメントの照明を変更する権限がありません。");
            return;
        }
        
        var s = args.Split(' ');
        if (s.Length < 1) {
            if (ShowCommandErrors) Service.Chat.PrintError($"/{CustomOrDefaultCommand} (0-5) [ssao-on | ssao-off] [save]");
            return;
        }

        var brightness = -1;
        var permanent = false;
        var ssaoEnable = housingManager->IndoorTerritory->SSAOEnable;
        
        foreach (var a in s) {
            if (byte.TryParse(a, out var o)) {
                if (o <= 5) brightness = o;
            }

            if (permanentMarkers.Contains(a, StringComparer.InvariantCultureIgnoreCase)) permanent = true;

            if (a.Equals("ssao-on", StringComparison.InvariantCultureIgnoreCase)) ssaoEnable = true;
            if (a.Equals("ssao-off", StringComparison.InvariantCultureIgnoreCase)) ssaoEnable = false;
        }

        if (brightness < 0) {
            if (ShowCommandErrors) Service.Chat.PrintError($"/{CustomOrDefaultCommand} (0-5) [ssao-on | ssao-off] [save]");
            return;
        }
        
        var agent = AgentModule.Instance()->GetAgentByInternalId(AgentId.Housing);
        var isOpen = agent->IsAgentActive();
        
        Common.SendEvent(agent, 33, permanent ? 0 : 3, brightness, ssaoEnable);
        if (permanent) {
            // This just stops the housing menu from toggling
            if (!isOpen)
                agent->Hide();
            else
                agent->Show();
        } else {
            Service.Chat.Print(
                new SeString(
                    new TextPayload("表示上の照明を変更しました。設定を保存する場合は "),
                    new UIForegroundPayload(500),
                    new TextPayload($"/{Command} {brightness}  {(ssaoEnable ? "ssao-on" : "ssao-off")} save"),
                    new UIForegroundPayload(0),
                    new TextPayload(" を実行してください。")
                    )
                );
        }
    }
}
