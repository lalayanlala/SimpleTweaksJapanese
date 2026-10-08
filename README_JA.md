# Simple Tweaks 日本語版

Caraxi/SimpleTweaksPlugin 1.15.0.7（146e247d1785110ed4d9caee275ea43ecf414669）を基にした日本語化の派生版です。2026-10-07に変更しました。ライセンスはAGPL-3.0です。元作者と貢献者の権利・表記を維持しています。

この派生版の配布用InternalNameは **`SimpleTweaksJapanese`** です。本家の `SimpleTweaksPlugin` と同じInternalNameを使わないため、Dalamudのプラグイン一覧で本家に隠される問題を回避します。DLL名も `SimpleTweaksJapanese.dll` になります。C#のnamespaceは互換性のため `SimpleTweaksPlugin` のまま維持しています。

`Localization/ja.json`に翻訳キー別の日本語訳、`Localization/display-ja.json`に直接表示する文章の日本語訳を収録しています。両方をDLLに埋め込み、外部翻訳による上書きを止めています。機能名・説明・設定・通知・更新履歴を日本語にしています。設定の保存キーとコマンド名は維持しています。

.NET 10 SDKとDalamud API 15が必要です。Windowsでは`BUILD.bat`を実行すると、隣の`Plugin`フォルダに `SimpleTweaksJapanese.dll` を出力します。Dalamudの場所を変更する場合は`DALAMUD_HOME`を設定してください。`VERIFY_IDENTITY.bat`で配布ZIPのInternalName/DLL名を確認できます。

`/tweaks`コマンドは本家と共通のため、本家Simple Tweaksと日本語版を同時に有効にしないでください。

元のソース：https://github.com/Caraxi/SimpleTweaksPlugin  
日本語版：https://github.com/lalayanlala/SimpleTweaksJapanese
