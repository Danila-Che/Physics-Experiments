using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace XPBD
{
	internal readonly struct FakePose
	{
		private readonly float3 m_Position;
		private readonly quaternion m_Quaternion;

		public FakePose(float3 position, quaternion quaternion)
		{
			m_Position = position;
			m_Quaternion = quaternion;
		}

		public FakePose(in FakePose pose)
		{
			m_Position = pose.m_Position;
			m_Quaternion = pose.m_Quaternion;
		}

		public static FakePose Default => new(float3.zero, quaternion.identity);

		public float3 Position => m_Position;

		public quaternion Quaternion => m_Quaternion;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose Transform(in FakePose pose)
		{
			var position = Transform(pose.m_Position);
			var quaternion = math.mul(m_Quaternion, pose.m_Quaternion);

			return new FakePose(position, quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose Clone()
		{
			return new FakePose(m_Position, m_Quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 Transform(float3 vector)
		{
			return Rotate(vector) + m_Position;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 Rotate(float3 vector)
		{
			return math.mul(m_Quaternion, vector);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 InverseTransform(float3 vector)
		{
			return InverseRotate(vector - m_Position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 InverseRotate(float3 vector)
		{
			var inverse = math.conjugate(m_Quaternion);

			return math.mul(inverse, vector);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose Translate(float3 deltaPosition)
		{
			return new FakePose(m_Position + deltaPosition, m_Quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose SetRotation(quaternion rotation)
		{
			return new FakePose(m_Position, rotation).Translate(float3.zero);
		}
	}
}
