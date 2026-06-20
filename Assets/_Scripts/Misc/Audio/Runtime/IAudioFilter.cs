namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// 音声信号処理を行うフィルタの共通インターフェース。
    /// リアルタイム処理においてGC Alloc 0を実現するため、バッファを直接書き換える。
    /// </summary>
    public interface IAudioFilter
    {
        /// <summary>
        /// 音声バッファを処理し、インプレースで効果を適用する。
        /// </summary>
        /// <param name="buffer">処理対象のオーディオデータバッファ</param>
        /// <param name="channels">チャンネル数（モノラル: 1, ステレオ: 2）</param>
        /// <param name="sampleRate">サンプリングレート（例: 48000）</param>
        void Process(float[] buffer, int channels, int sampleRate);
    }
}
