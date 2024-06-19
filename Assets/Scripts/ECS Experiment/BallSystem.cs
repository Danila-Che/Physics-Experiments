using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine;

public partial struct BallSystem : ISystem {
    private InputComponent m_InputComponent;

    private void OnUpdate(ref SystemState state) {
        var entityManager = state.EntityManager;

        if (SystemAPI.TryGetSingleton(out m_InputComponent) is false) {
            return;
        }

        using var entities = entityManager.GetAllEntities(Allocator.Temp);

        foreach (var entity in entities) {
            if (entityManager.HasComponent<BallComponent>(entity)) {
                var ballComponent = entityManager.GetComponentData<BallComponent>(entity);
                var physicsVelocity = SystemAPI.GetComponentRW<PhysicsVelocity>(entity);

                var x = m_InputComponent.Movement.x * ballComponent.MoveSpeed * SystemAPI.Time.DeltaTime;
                var z = m_InputComponent.Movement.y * ballComponent.MoveSpeed * SystemAPI.Time.DeltaTime;
                physicsVelocity.ValueRW.Linear += new float3(x, 0.0f, z);
            }
        }
    }
}
