namespace _Scripts.Interface
{
    public interface IEnemyEntity
    {
        bool IsRemaining { get; }
        void Activate();
        void DisActivate();
    }
}