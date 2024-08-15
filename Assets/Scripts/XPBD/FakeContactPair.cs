using Unity.Mathematics;

namespace XPBD
{
	public readonly struct FakeContactPair
	{
		public readonly FakeBody Body0;
		public readonly FakeBody Body1;
		public readonly float3 Point;
		public readonly float3 Normal;
		public readonly float Depth;
		public readonly float3 DeltaVDirection;
		public readonly float DeltaVLength;
		public readonly float Friction;

		public FakeContactPair(
			FakeBody body0,
			FakeBody body1,
			float3 point,
			float3 normal,
			float depth,
			float3 deltaVDirection,
			float deltaVLength,
			float friction)
		{
			Body0 = body0;
			Body1 = body1;
			Point = point;
			Normal = normal;
			Depth = depth;
			DeltaVDirection = deltaVDirection;
			DeltaVLength = deltaVLength;
			Friction = friction;
		}
	}

	public readonly struct FakeContact
	{
		public readonly float3 Point;
		public readonly float3 Normal;
		public readonly float Separation;

		public FakeContact(float3 point, float3 normal, float separation)
		{
			Point = point;
			Normal = normal;
			Separation = separation;
		}
	}
}
