using UnityEngine;

namespace XPBD
{
    public abstract class BaseFakeCollider
    {
        public enum Type
        {
            Box,
            Sphere
        }

        protected float m_InverseMass;
        protected Vector3 m_InverseInertiaTensor;  // may just inverse inertia
        protected Vector3 m_Drag;

        public Vector3 Drag => m_Drag;
        public float InverseMass => m_InverseMass;
        public Vector3 InverseInertiaTensor => m_InverseInertiaTensor;

        public abstract float Volume { get; }
        public abstract Vector3 BBSize { get; }

        public abstract Vector3 CalcAABBExtents(Quaternion rotation);
        public abstract bool Intersect(Pose atPose, BaseFakeCollider withCollider, Pose otherPose, out Vector3 point, out Vector3 normal, out float shift);
        public abstract bool IntersectWithFloor(Pose atPose, float floorLevel, out Vector3 point, out Vector3 normal, out float shift);
    }
}
