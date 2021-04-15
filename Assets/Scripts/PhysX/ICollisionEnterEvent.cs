
namespace PhysX {
    public interface ICollisionEnterEvent {
        bool requiresData { get; }

        void CollisionEnter();
        void CollisionEnter(PhysXCollision collision);
    }
}