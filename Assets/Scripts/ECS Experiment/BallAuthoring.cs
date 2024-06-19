using Unity.Entities;
using UnityEngine;

public class BallAuthoring : MonoBehaviour {
    [SerializeField] private float m_MoveSpeed = 10.0f;

    public class BallAuthoringBaker : Baker<BallAuthoring> {
        public override void Bake(BallAuthoring authoring) {
            var ballAuthoring = GetEntity(TransformUsageFlags.None);
            AddComponent(ballAuthoring, new BallComponent {
                MoveSpeed = authoring.m_MoveSpeed,
            });
        }
    }
}
