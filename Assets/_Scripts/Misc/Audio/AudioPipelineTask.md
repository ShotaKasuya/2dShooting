# Task

Unity用のリアルタイム音声DSPフィルタ「Plasma Resonator」を実装してください。

このフィルタは SpaceFantasy 系シューティングゲーム向けの効果音加工を目的とします。

対象:

* レーザー
* プラズマ砲
* シールド
* ワープ
* エネルギーエンジン
* SF UI
* ボス兵器

に適用しやすい設計にしてください。

---

# Environment

* Unity 6
* C#
* Burst対応を考慮
* ScriptableAudioPipeline ベース
* リアルタイム処理
* GC Alloc 0
* モバイルでも動作可能な軽量実装
* DSPGraph互換性を意識

---

# Required Architecture

以下のようなモジュール構造にしてください。

```text
IAudioFilter
 ├ PlasmaResonatorFilter
 ├ CombFilter
 ├ FrequencyShifter
 ├ SoftSaturation
 ├ DynamicLowpass
```

各フィルタは独立したDSPモジュールとして設計してください。

---

# Plasma Resonator Specification

Plasma Resonator は以下のDSPを直列接続してください。

```text
Input
 ↓
Comb Filter
 ↓
Frequency Shifter
 ↓
Soft Saturation
 ↓
Dynamic Lowpass
 ↓
Output
```

---

# DSP Details

## 1. Comb Filter

目的:

* プラズマ感
* 共鳴感
* エネルギー兵器感

仕様:

* Short feedback delay
* Delay: 5ms〜30ms
* Feedback: 0.5〜0.95
* Damping LPF付き

数式例:

```text
y[n] = x[n] + feedback * y[n - delay]
```

必要:

* リングバッファ
* サンプル単位処理
* 可変ディレイ

---

## 2. Frequency Shifter

目的:

* 異星感
* 宇宙感
* 現実離れした質感

仕様:

* 周波数を固定Hzだけシフト
* Pitch Shiftではない
* 軽量近似版で良い

最低限:

* Sine modulation
* Quadrature approximation

パラメータ:

* ShiftHz: 0〜1000Hz

---

## 3. Soft Saturation

目的:

* 音圧
* エネルギー感
* デジタル臭さ軽減

仕様:

* Soft clip
* tanh approximation
* branch削減

例:

```text
y = x / (1 + abs(x))
```

---

## 4. Dynamic Lowpass

目的:

* 熱暴走感
* 武器チャージ感
* 発振感

仕様:

* 入力音量に応じて cutoff が変化
* Envelope follower 使用

例:

* 大音量時:

    * cutoff低下
    * resonance増加

---

# Required Features

## Exposed Parameters

```text
Resonance
DelayTimeMs
Feedback
ShiftHz
Drive
LowpassBaseCutoff
DynamicAmount
Wet
Dry
```

---

# Performance Requirements

必須:

* allocation禁止
* LINQ禁止
* virtual call最小化
* SIMD/Burst最適化しやすい構造
* NativeArray利用可能設計

---

# Unity Integration

以下を実装してください。

* MonoBehaviour wrapper
* ScriptableObject preset
* Inspector parameter UI
* Runtime parameter automation
* AudioSource接続例

---

# Presets

最低限以下を用意してください。

## Plasma Cannon

* 強い resonance
* 中程度 shift
* 重い saturation

## Alien Shield

* 高 shift
* 長め delay
* soft resonance

## Warp Engine

* modulation 強め
* dynamic LPF 強め

---

# Code Quality

要求:

* コメント付き
* 数学的説明あり
* DSP意図を説明
* Unity向け最適化理由を書く

---

# Output Format

以下の順で出力してください。

1. アーキテクチャ説明
2. DSP理論
3. クラス図
4. 完全なC#コード
5. Unity設定方法
6. 最適化ポイント
7. 改良案

---

# Important

音の方向性は:

* Destiny 2
* Returnal
* NieR:Automata
* DOOM Eternal

のような「未来的だが物理感のある」質感を目指してください。

単なるロボ声ではなく、
「高エネルギー物質が空間共鳴している」
ようなサウンドを作ること。
