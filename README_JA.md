# Simple Tweaks 日本語版

Caraxi/SimpleTweaksPlugin 1.15.0.7（146e247d1785110ed4d9caee275ea43ecf414669）を基にした日本語化の派生版です。2026-10-07に変更しました。ライセンスはAGPL-3.0です。元作者と貢献者の権利・表記を維持しています。

`Localization/ja.json`に翻訳キー別の日本語訳、`Localization/display-ja.json`に直接表示する文章の日本語訳を収録しています。両方をDLLに埋め込み、外部翻訳による上書きを止めています。機能名・説明・設定・通知・更新履歴を日本語にしています。設定の保存キーとコマンド名は維持しています。

原文の更新履歴・技術資料は元のファイルに残しています。開発者用デバッグ画面とOSなどが生成する例外原文は翻訳対象外です。

.NET 10 SDKとDalamud API 15が必要です。Windowsでは`BUILD.bat`を実行すると、隣の`Plugin`フォルダに出力します。Dalamudの場所を変更する場合は`DALAMUD_HOME`を設定してください。

Releaseビルドと翻訳読み込み・書式・識別子の検証を実施しました。FF14上では未確認です。元のSimple Tweaksと同時に有効にしないでください。

元のソース：https://github.com/Caraxi/SimpleTweaksPlugin
