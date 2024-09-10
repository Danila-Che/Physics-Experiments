using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics
{
	public readonly struct Pose : IComponentData
	{
		public static readonly Pose k_Identity = new(float3.zero, quaternion.identity);

		public readonly float3 Position;
		public readonly quaternion Rotation;

		public Pose(float3 position, quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}
	}
}
