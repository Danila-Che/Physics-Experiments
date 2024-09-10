using Unity.Entities;

namespace ECSPhysics
{
	public readonly struct RigidBody : IComponentData
	{
		public readonly bool IsKinematic;

		public RigidBody(bool isKinematic)
		{
			IsKinematic = isKinematic;
		}
	}
}
