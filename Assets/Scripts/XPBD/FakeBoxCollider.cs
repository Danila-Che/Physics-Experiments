using UnityEngine;

namespace XPBD
{
    public class FakeBoxCollider : BaseFakeCollider
    {
        public override float Volume => throw new System.NotImplementedException();

        public override Vector3 BBSize => throw new System.NotImplementedException();

        public override Vector3 CalcAABBExtents(Quaternion rotation)
        {
            throw new System.NotImplementedException();
        }

        public override bool Intersect(Pose atPose, BaseFakeCollider withCollider, Pose otherPose, out Vector3 point, out Vector3 normal, out float shift)
        {
            throw new System.NotImplementedException();
        }

        public override bool IntersectWithFloor(Pose atPose, float floorLevel, out Vector3 point, out Vector3 normal, out float shift)
        {
            throw new System.NotImplementedException();
        }
    }
}
