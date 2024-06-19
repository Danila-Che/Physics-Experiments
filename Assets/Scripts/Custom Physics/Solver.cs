using System;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics
{
    public class Solver : MonoBehaviour
    {
        private enum Evaluation
        {
            Sequential,
            Parallel,
        }

        [Serializable]
        private struct Constraint
        {
            public Evaluation Evaluation;
            public int Iterations;
        }

        [SerializeField] private float3 m_Gravity;
        [SerializeField] private Constraint m_Distance;
        [SerializeField] private Constraint m_Bending;

        public JobHandle BeginStep(float stepTimeInSeconds)
        {
            return new JobHandle();
        }

        public JobHandle Substep(float substepTimeInSeconds)
        {
            return new JobHandle();
        }

        public void EndStep(float substepTimeInSeconds)
        {

        }

        public void Interpolate(float stepTimeInSeconds, float unsimulatedTimeInSeconds)
        {

        }
    }
}
