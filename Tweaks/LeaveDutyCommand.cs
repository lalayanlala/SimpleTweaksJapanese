using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using SimpleTweaksPlugin.Tweaks.AbstractTweaks;
using SimpleTweaksPlugin.TweakSystem;
using SimpleTweaksPlugin.Utility;

namespace SimpleTweaksPlugin.Tweaks;

[TweakName("Leave Duty Command")]
[TweakDescription("Adds a command to leave the current duty.")]
[TweakReleaseVersion("1.9.0.0")]
public unsafe class LeaveDutyCommand : CommandTweak {
    protected override string Command => "/leaveduty";
    protected override string HelpMessage => Loc.Text("Leave the current duty");

    protected override void OnCommand(string args) {
        if (Service.Condition.Cutscene()) {
            if (ShowCommandErrors) Service.Chat.PrintError("カットシーン中は退出できません。");
            return;
        }

        if (!Service.Condition.Duty()) {
            if (ShowCommandErrors) Service.Chat.PrintError("コンテンツに参加していません。");
            return;
        }

        if (!EventFramework.CanLeaveCurrentContent()) {
            if (ShowCommandErrors) Service.Chat.PrintError("現在のコンテンツからは退出できません。");
            return;
        }

        var agent = AgentModule.Instance()->GetAgentByInternalId(AgentId.ContentsFinderMenu);
        if (agent == null) return;
        Common.SendEvent(agent, 0, 0);
    }
}
