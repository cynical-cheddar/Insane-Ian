
namespace PhysX {
    public interface ICollisionExitEvent {
        bool requiresData { get; }

        void OnCollisionExit();
        void OnCollisionExit(PhysXCollision collision);
    }
}