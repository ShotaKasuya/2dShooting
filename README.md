# 2dShooting
ぼっちunity 2week成果物
[unity room](https://unityroom.com/games/shooting2d_2w)

## やってみたかったこと

- [x] UIToolkitを触ってみる
- [x] Scriptable Audio Pipelineを触ってみる
- [x] Visual Effect Graphを触ってみる

## 反省会

### UIToolkitを触ってみる

UXML,USSを適当にAIに吐かせることができてとても良い。
Unity roomに投稿するにあたって「枠からボタンがはみ出る」「ボタンの押し判定がうまく取れない」といった問題が発生した。

前者はSizeの設定に%を使うことを強制するなどすれば出会わないかもしれない。
後者は今でもよくわからない。
もっとAIにお金を払えばよい可能性がある。

UXMLとUSS周りのスクリプトはAIに吐かせたのでエラーになったことはないが、
stringで要素を取得するのは気持ち悪い。UXMLからconst定義用クラスを作るエディタ拡張を書いたらいいかも。

### Scriptable Audio Pipelineを触ってみる

音声に後処理を加えたら素材を減らしてもある程度繰り返し感が減るかもと思い、行った。
そもそも音に関する知識がないのでAIだよりになり、結果として提案されたフィルタを実装させたが、違いはよく分からなかった。

事例をもっと知りたい

### Visual Effect Graphを触ってみる

これまで見た目の話を避け続けてきたので触ってみた。
一番時間をかけて取り組んだが、Unity roomでは見えなくなった。

なんとなく雰囲気だけ触ったので、まだまだ勉強が必要そう。
