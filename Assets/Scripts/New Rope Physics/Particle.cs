using Unity.Mathematics;

namespace RopePhysics
{
	public struct Particle
	{
		public float3 Position;
		public float3 OldPosition;
		public bool IsSleep;
		public float InverseMass;

		public Particle(float3 position)
		{
			Position = position;
			OldPosition = position;
			IsSleep = true;
			InverseMass = 0.0f;
		}

		public Particle(float3 position, float mass)
		{
			Position = position;
			OldPosition = position;
			IsSleep = true;
			InverseMass = 1.0f / mass;
		}

		public readonly float3 Velocity => Position - OldPosition;

		public void SetMass(float mass)
		{
			if (float.IsInfinity(mass))
			{
				InverseMass = 0.0f;
			}
			else
			{
				InverseMass = 1.0f / mass;
			}
		}

		public void SetInfinityMass()
		{
			InverseMass = 0.0f;
		}
	}
}
