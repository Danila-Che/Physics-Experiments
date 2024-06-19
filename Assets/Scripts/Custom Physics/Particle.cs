using Unity.Mathematics;

namespace CustomPhysics
{
    public struct Particle
    {
        public float3 OldPosition;
        public float3 Position;
        public bool IsSleep;

        public Particle(float3 position)
        {
            OldPosition = position;
            Position = position;
            IsSleep = true;
        }
    }
}
