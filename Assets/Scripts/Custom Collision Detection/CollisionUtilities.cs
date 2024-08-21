using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace CustomCollisionDetection
{
	public class CollisionUtilities
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckLine(ref Simplex points, ref float3 direction)
		{
			var a = points[0];
			var b = points[1];

			var ab = b - a;
			var ao = -a;

			if (HasSameDirection(ab, ao))
			{
				direction = math.cross(math.cross(ab, ao), ab);
			}
			else
			{
				points.Set(a);
				direction = ao;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckTriangle(ref Simplex points, ref float3 direction)
		{
			var a = points[0];
			var b = points[1];
			var c = points[2];

			var ab = b - a;
			var ac = c - a;
			var ao = -a;

			var abc = math.cross(ab, ac);

			if (HasSameDirection(math.cross(abc, ac), ao))
			{
				if (HasSameDirection(ac, ao))
				{
					points.Set(a, c);
					direction = math.cross(math.cross(ac, ao), ac);
				}
				else
				{
					points.Set(a, b);

					return CheckLine(ref points, ref direction);
				}
			}
			else
			{
				if (HasSameDirection(math.cross(ab, abc), ao))
				{
					points.Set(a, b);

					return CheckLine(ref points, ref direction);
				}
				else
				{
					if (HasSameDirection(abc, ao))
					{
						direction = abc;
					}
					else
					{
						points.Set(a, c, b);
						direction = -abc;
					}
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckTetrahedron(ref Simplex points, ref float3 direction)
		{
			var a = points[0];
			var b = points[1];
			var c = points[2];
			var d = points[3];

			var ab = b - a;
			var ac = c - a;
			var ad = d - a;
			var ao = -a;

			var abc = math.cross(ab, ac);
			var acd = math.cross(ac, ad);
			var adb = math.cross(ad, ab);

			if (HasSameDirection(abc, ao))
			{
				points.Set(a, b, c);

				return CheckTriangle(ref points, ref direction);
			}

			if (HasSameDirection(acd, ao))
			{
				points.Set(a, c, d);

				return CheckTriangle(ref points, ref direction);
			}

			if (HasSameDirection(adb, ao))
			{
				points.Set(a, d, b);

				return CheckTriangle(ref points, ref direction);
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool HasSameDirection(float3 direction, float3 ao)
		{
			return math.dot(direction, ao) > 0.0f;
		}
	}
}
