
namespace PhysX {
    public interface ITriggerEnterEvent {
        bool requiresData { get; }

        void OnTriggerEnter();
        void OnTriggerEnter(PhysXCollider collider);
    }
}