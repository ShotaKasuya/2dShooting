using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// 1乗算で動作する最初期型1次オールパスフィルタ。
    /// 伝達関数: H(z) = (a^2 - z^-2) / (1 - a^2 * z^-2)
    /// 差分方程式: y[n] = a^2 * (x[n] + y[n-2]) - x[n-2]
    /// </summary>
    public struct AllPassSection
    {
        private float aSq;
        private float x1, x2;
        private float y1, y2;

        public AllPassSection(float a)
        {
            aSq = a * a;
            x1 = x2 = y1 = y2 = 0f;
        }

        public float Process(float input)
        {
            float output = aSq * (input + y2) - x2;
            
            // 状態の遅延シフト
            x2 = x1;
            x1 = input;
            y2 = y1;
            y1 = output;

            return output;
        }
    }

    /// <summary>
    /// 4つのAllPassSectionを直列に接続したフィルタチェーン。
    /// </summary>
    public struct AllPassChain
    {
        private AllPassSection s1;
        private AllPassSection s2;
        private AllPassSection s3;
        private AllPassSection s4;

        public void Init(float a1, float a2, float a3, float a4)
        {
            s1 = new AllPassSection(a1);
            s2 = new AllPassSection(a2);
            s3 = new AllPassSection(a3);
            s4 = new AllPassSection(a4);
        }

        public float Process(float input)
        {
            float out1 = s1.Process(input);
            float out2 = s2.Process(out1);
            float out3 = s3.Process(out2);
            return s4.Process(out3);
        }
    }

    /// <summary>
    /// 軽量近似版シングルサイドバンド (SSB) 変調型周波数シフタ。
    /// IIRオールパスフィルタによるヒルベルト変換（90度位相差分割）を使用します。
    /// ピッチシフタとは異なり、全周波数を固定Hz分だけ一定値加算・減算するため、
    /// 倍音関係が崩れ、宇宙的・SF的な不協和共鳴音を作り出せます。
    /// </summary>
    public struct FrequencyShifter
    {
        private AllPassChain allpassL1; // 左チャンネル I（同相）パス
        private AllPassChain allpassL2; // 左チャンネル Q（直交）パス
        private AllPassChain allpassR1; // 右チャンネル Iパス
        private AllPassChain allpassR2; // 右チャンネル Qパス

        private float oscPhase;
        private float lastInputL;
        private float lastInputR;

        /// <summary>
        /// Olli Niemitalo の広帯域 90度位相差設計に基づく係数で初期化する。
        /// </summary>
        public void Init()
        {
            // フィルタ1（Iパス、さらにz^-1遅延を追加適用する）の極係数
            allpassL1.Init(0.6923878f, 0.9360654f, 0.9882295f, 0.9987488f);
            allpassR1.Init(0.6923878f, 0.9360654f, 0.9882295f, 0.9987488f);

            // フィルタ2（Qパス、z^-1遅延なし）の極係数
            allpassL2.Init(0.4021921f, 0.8561711f, 0.9722910f, 0.9952885f);
            allpassR2.Init(0.4021921f, 0.8561711f, 0.9722910f, 0.9952885f);

            oscPhase = 0f;
            lastInputL = 0f;
            lastInputR = 0f;
        }

        /// <summary>
        /// ステレオ信号に対して周波数シフトを適用する。
        /// </summary>
        public void ProcessStereo(ref float left, ref float right, float shiftHz, float sampleRate)
        {
            // シフト周波数がほぼ0なら処理をバイパス
            if (shiftHz <= 0.05f) return;

            // 発振器の位相の更新
            float phaseInc = 2f * Mathf.PI * shiftHz / sampleRate;
            oscPhase += phaseInc;
            if (oscPhase >= 2f * Mathf.PI)
            {
                oscPhase -= 2f * Mathf.PI;
            }

            float cos = Mathf.Cos(oscPhase);
            float sin = Mathf.Sin(oscPhase);

            // ヒルベルト変換器の適用
            // 左チャンネル:
            // I パスは z^-1 の遅延を持つ
            float inL1 = lastInputL;
            lastInputL = left;
            float outL1 = allpassL1.Process(inL1);
            // Q パスは遅延なし
            float outL2 = allpassL2.Process(left);

            // 右チャンネル:
            float inR1 = lastInputR;
            lastInputR = right;
            float outR1 = allpassR1.Process(inR1);
            float outR2 = allpassR2.Process(right);

            // SSB変調（コサインとサインによる複素乗算）
            // 周波数アップシフト: y(t) = I(t)*cos(w*t) - Q(t)*sin(w*t)
            left = outL1 * cos - outL2 * sin;
            right = outR1 * cos - outR2 * sin;
        }
    }
}
