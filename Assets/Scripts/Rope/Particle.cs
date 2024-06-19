using Unity.Mathematics;
using UnityEngine;

namespace Verlet
{
    public struct Particle
    {
        public float3 PreviousPosition;
        public float3 Position;
    }
}
