using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;

namespace CustomCollisionDetection
{
	public readonly struct CollisionPoints
	{
		public readonly float3 Normal;
		public readonly float PenetrationDepth;
		public readonly List<float3> Points;

		public CollisionPoints(float3 normal, float penetrationDepth)
		{
			Normal = math.normalize(normal);
			PenetrationDepth = penetrationDepth;
			Points = new List<float3>();
		}
	}

	public class CollisionSystem
	{
		public struct RawContact
		{
			public float3 Point;
			public float Angle;

			public RawContact(float3 point)
			{
				Point = point;
				Angle = 0.0f;
			}

			public RawContact(float3 point, float angle)
			{
				Point = point;
				Angle = angle;
			}

			public override readonly string ToString()
			{
				return $"{Point} {Angle}";
			}
		}

		public struct RawLine
		{
			public float3 Point0;
			public float3 Point1;

			public RawLine(float3 point0, float3 point1)
			{
				Point0 = point0;
				Point1 = point1;
			}

			public readonly float3 GetVector()
			{
				return math.normalize(Point1 - Point0);
			}
		}

		public void CalculateContactPoints(
			CustomBoxCollider collider,
			CustomBoxCollider otherCollider,
			ref CollisionPoints collisionPoints)
		{
			var contactPointsA = new List<RawContact>(capacity: 4);
			var contactPointsB = new List<RawContact>(capacity: 4);

			var contactPlane = new Plane(collisionPoints.Normal, collider.FindFurthestPoint(collisionPoints.Normal));

			FindContactPoints(collider, contactPlane, contactPointsA);
			FindContactPoints(otherCollider, contactPlane, contactPointsB);

			if (contactPointsA.Count == 0)
			{
				for (int i = 0; i < contactPointsB.Count; i++)
				{
					collisionPoints.Points.Add(contactPointsB[i].Point);
				}

				return;
			}

			if (contactPointsB.Count == 0)
			{
				for (int i = 0; i < contactPointsA.Count; i++)
				{
					collisionPoints.Points.Add(contactPointsA[i].Point);
				}

				return;
			}

			//Vertex to face contact
			if (contactPointsA.Count == 1)
			{
				collisionPoints.Points.Add(contactPointsA[0].Point);
				return;
			}
			else if (contactPointsB.Count == 1)
			{
				collisionPoints.Points.Add(contactPointsB[0].Point);
				return;
			}

			CalculateAngles(contactPointsA, collisionPoints.Normal);
			CalculateAngles(contactPointsB, collisionPoints.Normal);

			contactPointsA.Sort(CompareAngle);
			contactPointsB.Sort(CompareAngle);

			var realContactPoints = new List<float3>(4);
			CheckIntersection(contactPointsA, contactPointsB, collisionPoints.Normal, realContactPoints);

			if (realContactPoints.Count == 0)
			{
				CheckIntersection(contactPointsB, contactPointsA, collisionPoints.Normal, realContactPoints);
			}

			collisionPoints.Points.Clear();
			collisionPoints.Points.AddRange(realContactPoints);
		}

		private void FindContactPoints(CustomBoxCollider collider, Plane plane, List<RawContact> contactBuffer)
		{
			for (int i = 0; i < collider.WorldVertices.Length; i++)
			{
				var worldPoint = collider.WorldVertices[i];
				var distance = plane.SignedDistanceToPoint(worldPoint);

				if (math.abs(distance) > 0.005f) { continue; }

				contactBuffer.Add(new RawContact(worldPoint, 0.0f));
			}
		}

		public static void CalculateAngles(List<RawContact> contactBuffer, float3 normal)
		{
			var origin = FindOrigin(contactBuffer);
			var refVector = contactBuffer[0].Point - origin;

			for (int i = 1; i < contactBuffer.Count; i++)
			{
				var originToPoint = contactBuffer[i].Point - origin;
				var u = math.dot(normal, math.cross(refVector, originToPoint));
				var angle = AngleInDegrees(refVector, originToPoint);

				var contact = contactBuffer[i];

				if (u <= 0.001f)
				{
					contact.Angle = angle;
				}
				else
				{
					contact.Angle = angle + 180.0f;
				}

				contactBuffer[i] = contact;
			}
		}

		private static float3 FindOrigin(List<RawContact> contactBuffer)
		{
			var origin = float3.zero;

			for (int i = 0; i < contactBuffer.Count; i++)
			{
				origin += contactBuffer[i].Point;
			}

			origin /= contactBuffer.Count;
			return origin;
		}

		public static int CompareAngle(RawContact contactA, RawContact contactB)
		{
			if (contactA.Angle < contactB.Angle)
			{
				return -1;
			}

			return 1;
		}

		private void CheckIntersection(
			List<RawContact> contactPointsA,
			List<RawContact> contactPointsB,
			float3 normal,
			List<float3> contactPointsBuffer)
		{
			for (int it = 0; it < contactPointsB.Count; it++)
			{
				//We assume that the first point is inside
				var passedCount = 1;

				if (IsInside(contactPointsB[it].Point, contactPointsA, normal))
				{
					contactPointsBuffer.Add(contactPointsB[it].Point);
				}
				else
				{
					passedCount--; // = 0
				}

				var itSecondP = it + 1;
				if (itSecondP == contactPointsB.Count)
				{
					itSecondP = 0;
				}

				if (IsInside(contactPointsB[itSecondP].Point, contactPointsA, normal))
				{
					passedCount++; // = 1 || = 2
				}
				else
				{
					passedCount--; // = 0 || = -1
				}

				//all points of current line is inside shape
				if (passedCount == 2)
				{
					continue;
				}

				//all points of current line is outside shape
				if (passedCount < 0 && !(contactPointsA.Count == 2 && contactPointsB.Count == 2))
				{
					continue;
				}

				//Check if line separate shape
				for (int jt = 0; jt < contactPointsA.Count; jt++)
				{
					int jtSecondP = jt + 1;
					if (jtSecondP == contactPointsA.Count)
					{
						jtSecondP = 0;
					}

					bool notParallel = TryGetLineIntersection(
						new RawLine(contactPointsB[it].Point, contactPointsB[itSecondP].Point),
						new RawLine(contactPointsA[jt].Point, contactPointsA[jtSecondP].Point),
						out float3 projectPoint);

					if (notParallel)
					{
						float vct1 = math.lengthsq(contactPointsB[it].Point - projectPoint);
						float vct2 = math.lengthsq(contactPointsB[itSecondP].Point - projectPoint);

						float lengthAxisB = math.lengthsq(contactPointsB[itSecondP].Point - contactPointsB[it].Point);
						float lengthAxisA = math.lengthsq(contactPointsA[jtSecondP].Point - contactPointsA[jt].Point);

						if (vct1 > lengthAxisA || vct2 > lengthAxisA)
						{
							continue;
						}

						if (vct1 > lengthAxisB || vct2 > lengthAxisB)
						{
							continue;
						}

						contactPointsBuffer.Add(projectPoint);
					}
				}
			}

			for (int it = 0; it < contactPointsA.Count; it++)
			{
				if (IsInside(contactPointsA[it].Point, contactPointsB, normal, checkStrictly: false))
				{
					contactPointsBuffer.Add(contactPointsA[it].Point);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleInDegrees(float3 from, float3 to)
		{
			float num = math.sqrt(math.lengthsq(from) * math.lengthsq(to));

			if (num < 1E-15f)
			{
				return 0.0f;
			}

			float num2 = math.clamp(math.dot(from, to) / num, -1.0f, 1.0f);
			return math.degrees(math.acos(num2));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLineIntersection(RawLine line0, RawLine line1, out float3 intersectionPoint)
		{
			float3 p13 = line0.Point0 - line1.Point0;
			float3 p43 = line1.Point1 - line1.Point0;

			if (math.lengthsq(p43) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}

			float3 p21 = line0.Point1 - line0.Point0;

			if (math.lengthsq(p21) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}

			float d1343 = math.dot(p13, p43);
			float d4321 = math.dot(p43, p21);
			float d1321 = math.dot(p13, p21);
			float d4343 = math.dot(p43, p43);
			float d2121 = math.dot(p21, p21);

			float denom = d2121 * d4343 - d4321 * d4321;
			if (math.abs(denom) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}
			float numer = d1343 * d4321 - d1321 * d4343;

			float mua = numer / denom;
			float3 pa = line0.Point0 + mua * p21;

			intersectionPoint = pa;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInside(float3 point, List<RawContact> shape, float3 normal, bool checkStrictly = true)
		{
			for (int i = 0; i < shape.Count; i++)
			{
				var j = i + 1;
				if (j == shape.Count)
				{
					j = 0;
				}

				var vector = point - shape[i].Point;
				var edgeVector = shape[j].Point - shape[i].Point;

				var direction = math.cross(math.normalize(edgeVector), normal);

				if (checkStrictly)
				{
					if (math.dot(direction, vector) < 0.0f)
					{
						return false;
					}
				}
				else
				{
					if (math.dot(direction, vector) <= 0.0f)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
