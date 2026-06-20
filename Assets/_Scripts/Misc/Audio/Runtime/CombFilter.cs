using Unity.Collections;
using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// フィードバック型コームフィルタ (Feedback Comb Filter)
    /// ショートフィードバックディレイとダンピングLPFにより、プラズマや金属的な共鳴効果を作ります。
    /// </summary>
    public struct CombFilter
    {
        private NativeArray<float> delayBufferL;
        private NativeArray<float> delayBufferR;
        private int bufferLength;
        private int writeIndex;
        private float lastFilteredL;
        private float lastFilteredR;

        /// <summary>
        /// フィルタを初期化し、ディレイライン用のメモリを確保する。
        /// </summary>
        public void Init(float sampleRate, float maxDelayMs = 50f)
        {
            // 最大ディレイ時間に対応するサンプル数（少し余裕を持たせる）
            int maxDelaySamples = Mathf.CeilToInt(sampleRate * maxDelayMs * 0.001f);
            
            // リングバッファのインデックス処理をビットワイズANDで高速化するため、
            // バッファサイズを2のべき乗に丸める。
            bufferLength = Mathf.NextPowerOfTwo(maxDelaySamples);
            if (bufferLength < 4) bufferLength = 4;

            delayBufferL = new NativeArray<float>(bufferLength, Allocator.Persistent);
            delayBufferR = new NativeArray<float>(bufferLength, Allocator.Persistent);
            writeIndex = 0;
            lastFilteredL = 0f;
            lastFilteredR = 0f;
        }

        /// <summary>
        /// 確保したアンマネージメモリを解放する。メモリリークを防ぐために必須。
        /// </summary>
        public void Dispose()
        {
            if (delayBufferL.IsCreated) delayBufferL.Dispose();
            if (delayBufferR.IsCreated) delayBufferR.Dispose();
        }

        /// <summary>
        /// ステレオサンプルに対してコームフィルタとダンピングLPFを適用する。
        /// </summary>
        public void ProcessStereo(ref float left, ref float right, float delayMs, float feedback, float damping, float sampleRate)
        {
            if (!delayBufferL.IsCreated) return;

            // ディレイ時間をサンプル数に変換
            float delaySamples = sampleRate * delayMs * 0.001f;
            
            // ディレイ時間はバッファサイズ未満に制限
            if (delaySamples >= bufferLength) delaySamples = bufferLength - 1;
            if (delaySamples < 0f) delaySamples = 0f;

            // 小数ディレイをサポートするための読み込み位置
            float readPosition = (float)writeIndex - delaySamples;
            if (readPosition < 0f) readPosition += bufferLength;

            int idx0 = Mathf.FloorToInt(readPosition) & (bufferLength - 1);
            int idx1 = (idx0 + 1) & (bufferLength - 1);
            float frac = readPosition - Mathf.Floor(readPosition);

            // 線形補間による分数ディレイサンプルの取得（音程変化時のプチプチ音を抑制）
            float delayedL = Mathf.Lerp(delayBufferL[idx0], delayBufferL[idx1], frac);
            float delayedR = Mathf.Lerp(delayBufferR[idx0], delayBufferR[idx1], frac);

            // フィードバック経路の1ポール・ダンピング低域通過フィルタ (Damping LPF)
            // w[n] = (1 - damp) * y[n - delay] + damp * w[n-1]
            float filteredL = delayedL * (1f - damping) + lastFilteredL * damping;
            float filteredR = delayedR * (1f - damping) + lastFilteredR * damping;
            
            lastFilteredL = filteredL;
            lastFilteredR = filteredR;

            // コームフィルタの差分方程式:
            // y[n] = x[n] + feedback * w[n]
            float outL = left + feedback * filteredL;
            float outR = right + feedback * filteredR;

            // バッファに書き込み
            delayBufferL[writeIndex] = outL;
            delayBufferR[writeIndex] = outR;

            // インデックスの更新（2のべき乗バッファのため、剰余演算をビットANDで高速化）
            writeIndex = (writeIndex + 1) & (bufferLength - 1);

            left = outL;
            right = outR;
        }
    }
}
