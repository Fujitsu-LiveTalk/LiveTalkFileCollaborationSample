# LiveTalkFileCollaborationSample
LiveTalkのファイル入出力インターフェースと連携するサンプル
![Process](https://github.com/FujitsuSSL-LiveTalk/LiveTalkFileCollaborationSample/blob/images/README.png)
本サンプルコードは、.NET 6.0で作成しています。コードレベルでは.NET Framework 4.6と互換性があります。

# サンプルコードの動作
サンプルコード動作を簡単に説明すると次のような動作をします。
1. LiveTalkで音声認識した結果がファイルに出力され、それをコマンドプロンプトに表示。
2. コマンドプロンプトで入力したテキストは、自動的にLiveTalk画面に表示（発話者名はファイル名）。  
3. Program.csの63行当たりでは、items[2]変数にLiveTalkでの音声認識結果が１行分はいっています。
4. ここに独自処理を入れれば、音声認識結果に対する処理ができます。他システムへの入力につないでもいいですね。
5. 他システムから入力に対する応答があるならば、var answer = "";の代わりに応答テキストをanswerにいれるようにしてあげると、それがLiveTalkに表示されます。

# 連絡事項
本ソースコードは、LiveTalkの保守サポート範囲に含まれません。  
頂いたissueについては、必ずしも返信できない場合があります。  
LiveTalkそのものに関するご質問は、公式WEBサイトのお問い合わせ窓口からご連絡ください。
