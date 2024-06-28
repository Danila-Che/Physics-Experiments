using Unity.Mathematics;

namespace RopePhysics
{
	public struct Particle
	{
		public float3 Position;
		public float3 OldPosition;
		public bool IsSleep;

		public Particle(float3 position)
		{
			Position = position;
			OldPosition = position;
			IsSleep = true;
		}
	}
}
