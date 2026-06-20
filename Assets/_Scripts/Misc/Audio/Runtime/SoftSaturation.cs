using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// ソフトサチュレーション (Soft Saturation)
    /// tanh（双曲線正接関数）の高速代数近似を使用して、オーディオ信号に歪みを与え、音圧と存在感を高めます。
    /// 分岐 (branch) を排除し、モバイルなど低スペック機器でも超軽量かつ高音質に動作します。
    /// 近似式: y = x / (1 + |x|)
    /// </summary>
    public struct SoftSaturation
    {
        /// <summary>
        /// サンプルにソフトサチュレーションを適用する。
        /// </summary>
        /// <param name="input">入力サンプル値</param>
        /// <param name="drive">サチュレーションの強さ (1.0以上)</param>
        /// <returns>処理後のサンプル値</returns>
        public float Process(float input, float drive)
        {
            // 入力をドライブ値で増幅する
            float x = input * drive;
            
            // tanhの軽量近似式 y = x / (1 + |x|) を適用
            // 分岐が発生しないため、Burst/SIMD最適化時に最適。
            return x / (1.0f + Mathf.Abs(x));
        }
    }
}
