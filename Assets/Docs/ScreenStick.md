# 目的
Unityの UI Toolkit と Input System を使用して、モバイル端末向けの「画面上バーチャルスティック（On-Screen Stick）」を実装するためのC#スクリプト、UXML、USSを生成してください。

# 前提条件
- Unity 6000.4 LTS 以降を想定
- 新しい「Input System」パッケージを使用
- UI Toolkit（UI Builder）で画面レイアウトを管理

# 実装要件
1. クラス名：`UIToolkitOnScreenStick` (MonoBehaviourを継承)
2. UIの取得：
    - UIDocument から、スティックの背景となる要素（`stick-background`）と、動くつまみ要素（`stick-thumb`）を名前（Name）で取得する。
3. 入力制御の仕組み：
    - Input Systemの `UnityEngine.InputSystem.OnScreen.OnScreenControl` を継承するか、内部で `InputSystem.QueueDeltaStateEvent` もしくは `SendValueToControl` を使用して、ゲームパッドの左スティック（Gamepad/leftStick）に入力値を送信する。
    - スティックの最大可動範囲（半径）は、Inspectorからピクセル単位（または背景要素のサイズ基準）で調整可能にする。
4. タッチ・マウス操作イベント：
    - UI Toolkitの PointerEvents (`PointerDownEvent`, `PointerMoveEvent`, `PointerUpEvent`) を使用してドラッグ操作を検知する。
    - つまみ（thumb）は背景（background）の範囲内でのみ可動し、範囲外にドラッグされた場合は境界上でクランプ（固定）されるようにする。
    - 指を離した（PointerUp）時は、つまみが中央（0, 0）に自動で戻り、入力値もリセットされる。

# 出力してほしいもの
1. C#スクリプト (`UIToolkitOnScreenStick.cs`)
2. 実際にC#スクリプトを利用するためのUXML,USS
