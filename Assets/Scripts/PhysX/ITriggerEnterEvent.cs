
namespace PhysX {
    public interface ITriggerEnterEvent {
        bool requiresData { get; }

        void TriggerEnter();
        void TriggerEnter(PhysXCollider collider);
    }
}