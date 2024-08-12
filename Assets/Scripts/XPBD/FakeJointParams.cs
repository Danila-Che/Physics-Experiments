using System;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
    [Serializable]
    public class FakeJointParams : MonoBehaviour
    {
        public float3 LocalPos0;
        public quaternion LocalRot0 = Quaternion.identity;

        public float3 LocalPos1;
        public quaternion LocalRot1 = Quaternion.identity;

        public FakeJointType Type;					
        public float Compliance = 0.0f;
        public float RotationDamping = 0.0f;
        public float PositionDamping = 0.0f;
        public bool HasSwingLimits = false;
        public float MinSwingAngle = -2.0f * Mathf.PI;
        public float MaxSwingAngle = 2.0f * Mathf.PI;
        public float SwingLimitsCompliance = 0.0f;
        public bool HasTwistLimits = false;
        public float MinTwistAngle = -2.0f * Mathf.PI;
        public float MaxTwistAngle = 2.0f * Mathf.PI;
        public float TwistLimitCompliance = 0.0f;
        public float Distance = 0.0f;
    }
}
