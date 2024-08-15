using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
    public interface IFakeCollider
    {
        float3 Size { get; }
        Collider Collider { get; }
        float3 CalculateInverseInertiaTensor(float mass);
        bool Intersects(in FakePose selfPose, IFakeCollider otherCollider, in FakePose otherPose, out FakeContact contactPair);
	}
}
