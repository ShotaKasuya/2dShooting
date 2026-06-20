using UnityEngine;

namespace _Scripts.Behaviour
{
    /// <summary>
    /// ダメージを受けることが可能なオブジェクト用インターフェース
    /// </summary>
    public interface IDamage
    {
        public void ApplyDamage(DamageData data);
    }

    /// <summary>
    /// ダメージに関する情報を保持する構造体。
    /// ノックバックや将来的な拡張（属性、クリティカル等）に対応可能。
    /// </summary>
    public readonly ref struct DamageData
    {
        public readonly int Amount; // ダメージ量
        public readonly float KnockbackForce; // ノックバックの強さと方向
        public readonly Vector2 HitPosition; // ヒットした座標

        public DamageData(int amount, float knockbackForce, Vector2 hitPosition)
        {
            Amount = amount;
            KnockbackForce = knockbackForce;
            HitPosition = hitPosition;
        }
    }
}