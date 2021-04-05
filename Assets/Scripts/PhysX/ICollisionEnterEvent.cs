
namespace PhysX {
    public interface ICollisionEnterEvent {
        bool requiresData { get; }

        void OnCollisionEnter();
        void OnCollisionEnter(PhysXCollision collision);
    }
}