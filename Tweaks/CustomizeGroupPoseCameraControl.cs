using SimpleTweaksPlugin.TweakSystem;

namespace SimpleTweaksPlugin.Tweaks;

[TweakName("Customize Group Pose Camera Control")]
[TweakDescription("Allows you to customize the camera control in group pose")]
[TweakAutoConfig]
[TweakReleaseVersion("1.10.4.0")]
[TweakCategory(TweakCategory.QoL)]
public class CustomizeGroupPoseCameraControl : IDisabledTweak {
    public string DisabledMessage => "この機能はゲーム標準の設定に追加されました。グループポーズの設定でロール角補正を切り替えてください。";
}
