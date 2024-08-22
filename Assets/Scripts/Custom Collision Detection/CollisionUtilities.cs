using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace CustomCollisionDetection
{
	internal class CollisionUtilities
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleInDegrees(float3 from, float3 to)
		{
			var num = math.sqrt(math.lengthsq(from) * math.lengthsq(to));

			if (num < 1E-15f)
			{
				return 0.0f;
			}

			var num2 = math.clamp(math.dot(from, to) / num, -1.0f, 1.0f);
			return math.degrees(math.acos(num2));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ClosetPointBetweenAxis((float3, float3) axis1, (float3, float3) axis2, out float3 projectPoint)
		{
			var axis1Vector = math.normalize(axis1.Item2 - axis1.Item1);
			var axis2Vector = math.normalize(axis2.Item2 - axis2.Item1);

			float normU = math.dot(axis1Vector, axis2Vector);

			// Axes are parallel each others
			if (math.abs(normU) == 1.0f)
			{
				projectPoint = float3.zero;
				return false;
			}

			var cn = math.normalize(math.cross(axis2Vector, axis1Vector));
			var projection = axis1Vector * math.dot(axis2.Item1 - axis1.Item1, axis1Vector);
			var rejection = axis2.Item1 - axis1.Item1 - axis1Vector * math.dot(axis2.Item1 - axis1.Item1, axis1Vector) - cn * math.dot(axis2.Item1 - axis1.Item1, cn);
			var closetApproach = axis2.Item1 - axis2Vector * math.length(rejection) / math.dot(axis2Vector, math.normalize(rejection));

			projectPoint = closetApproach;
			return true;
		}
	}
}
