
namespace PhysX {
    public interface ICollisionExitEvent {
        bool requiresData { get; }

        void CollisionExit();
        void OnCollisionExit(PhysXCollision collision);
    }
}