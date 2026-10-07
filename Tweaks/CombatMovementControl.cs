using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Dalamud.Bindings.ImGui;
using SimpleTweaksPlugin.Events;
using SimpleTweaksPlugin.TweakSystem;

namespace SimpleTweaksPlugin.Tweaks;

[TweakName("Combat Movement Type Control")]
[TweakDescription("Set movement type between Standard and Legacy when in/out of combat or when weapon is drawn/sheathed.")]
[TweakAutoConfig]
public unsafe class CombatMovementControl : Tweak {
    public enum MoveModeType {
        Ignore = -1,
        Standard,
        Legacy,
    }

    public class Configs : TweakConfig {
        public MoveModeType InCombat = MoveModeType.Ignore;
        public MoveModeType OutOfCombat = MoveModeType.Ignore;
        public MoveModeType WeaponDrawn = MoveModeType.Ignore;
        public MoveModeType WeaponSheathed = MoveModeType.Ignore;
    }

    public Configs Config { get; private set; }

    protected void DrawConfig() {
        void ShowOption(string label, ref MoveModeType c) {
            if (ImGui.BeginCombo(Loc.Ui(label), Loc.Text($"{c}"))) {
                if (ImGui.Selectable(Loc.Ui(nameof(MoveModeType.Ignore)), c == MoveModeType.Ignore)) c = MoveModeType.Ignore;
                if (ImGui.Selectable(Loc.Ui(nameof(MoveModeType.Standard)), c == MoveModeType.Standard)) c = MoveModeType.Standard;
                if (ImGui.Selectable(Loc.Ui(nameof(MoveModeType.Legacy)), c == MoveModeType.Standard)) c = MoveModeType.Legacy;
                ImGui.EndCombo();
            }
        }

        ShowOption(Loc.Text("Out of Combat"), ref Config.OutOfCombat);
        ShowOption(Loc.Text("In Combat"), ref Config.InCombat);
        ShowOption(Loc.Text("Weapon Drawn"), ref Config.WeaponDrawn);
        ShowOption(Loc.Text("Weapon Sheathed"), ref Config.WeaponSheathed);
    }

    protected override void Enable() {
        Service.Condition.ConditionChange += OnConditionChange;
    }

    private bool? previousUnsheathedState;

    [FrameworkUpdate]
    private void OnFrameworkUpdate() {
        var unsheathedState = UIState.Instance()->WeaponState.IsUnsheathed;
        if (previousUnsheathedState == null) {
            previousUnsheathedState = unsheathedState;
            return;
        }

        if (unsheathedState != previousUnsheathedState) {
            previousUnsheathedState = unsheathedState;
            var v = unsheathedState ? Config.WeaponDrawn : Config.WeaponSheathed;
            if (v == MoveModeType.Ignore) return;
            Service.GameConfig.UiControl.Set("MoveMode", (uint)v);
        }
    }

    private void OnConditionChange(ConditionFlag flag, bool value) {
        if (flag == ConditionFlag.InCombat) {
            var v = value ? Config.InCombat : Config.OutOfCombat;
            if (v == MoveModeType.Ignore) return;
            Service.GameConfig.UiControl.Set("MoveMode", (uint)v);
        }
    }

    protected override void Disable() {
        Service.Condition.ConditionChange -= OnConditionChange;
    }
}
