using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class InputSystem : SystemBase {
    private Controls m_Controls;
    
    protected override void OnCreate() {
        if (SystemAPI.TryGetSingleton(out InputComponent input) is false) {
            EntityManager.CreateEntity(typeof(InputComponent));
        }

        m_Controls = new Controls();
        m_Controls.Enable();
    }

    protected override void OnUpdate() {
        var moveVector = m_Controls.Contols.Movement.ReadValue<Vector2>();

        SystemAPI.SetSingleton(new InputComponent {
            Movement = moveVector,
        });
    }
}