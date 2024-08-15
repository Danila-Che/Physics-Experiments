using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
	public readonly struct FakePose
	{
		public static readonly FakePose k_Identity = new(float3.zero, quaternion.identity);

		private readonly float3 m_Position;
		private readonly quaternion m_Rotation;

		public FakePose(float3 position, quaternion quaternion)
		{
			m_Position = position;
			m_Rotation = quaternion;
		}

		public FakePose(in FakePose pose)
		{
			m_Position = pose.m_Position;
			m_Rotation = pose.m_Rotation;
		}

		public FakePose(Transform transform)
		{
			m_Position = transform.position;
			m_Rotation = transform.rotation;
		}

		public float3 Position => m_Position;

		public quaternion Rotation => m_Rotation;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose Transform(in FakePose pose)
		{
			var position = Transform(pose.m_Position);
			var quaternion = math.mul(m_Rotation, pose.m_Rotation);

			return new FakePose(position, quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 Transform(float3 vector)
		{
			return Rotate(vector) + m_Position;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 Rotate(float3 vector)
		{
			return math.mul(m_Rotation, vector);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 InverseTransform(float3 vector)
		{
			return InverseRotate(vector - m_Position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 InverseRotate(float3 vector)
		{
			var inverse = math.inverse(m_Rotation);

			return math.mul(inverse, vector);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose Translate(float3 deltaPosition)
		{
			return new FakePose(m_Position + deltaPosition, m_Rotation);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakePose SetRotation(quaternion rotation)
		{
			return new FakePose(m_Position, rotation);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture.NumberFormat, "({0}, {1})", m_Position.ToString(), m_Rotation.ToString());
		}
	}
}
