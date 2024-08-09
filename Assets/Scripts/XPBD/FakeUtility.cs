using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace XPBD
{
	internal static class FakeUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetAxis0(quaternion quaternion)
		{
			var x2 = quaternion.value.x * 2.0f;
			var w2 = quaternion.value.x * 2.0f;

			return new float3(
				x: (quaternion.value.w * w2) - 1.0f + quaternion.value.x * x2,
				y: (quaternion.value.z * w2) + quaternion.value.y * x2,
				z: (-quaternion.value.y * w2) + quaternion.value.z * x2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetAxis1(quaternion quaternion)
		{
			var y2 = quaternion.value.y * 2.0f;
			var w2 = quaternion.value.w * 2.0f;

			return new float3(
				x: (-quaternion.value.z * w2) + quaternion.value.x * y2,
				y: (quaternion.value.w * w2) - 1.0f + quaternion.value.y * y2,
				z: (quaternion.value.x * w2) + quaternion.value.z * y2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetAxis2(quaternion quaternion)
		{
			var z2 = quaternion.value.z * 2.0f;
			var w2 = quaternion.value.w * 2.0f;

			return new float3(
				x: (quaternion.value.y * w2) + quaternion.value.x * z2,
				y: (-quaternion.value.x * w2) + quaternion.value.y * z2,
				z: (quaternion.value.w * w2) - 1.0f + quaternion.value.z * z2);
		}
	}
}