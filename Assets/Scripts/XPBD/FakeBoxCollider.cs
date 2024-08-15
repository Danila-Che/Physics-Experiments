using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
    public class FakeBoxCollider : IFakeCollider
    {
        private readonly float3 m_Size;
        private readonly float3 m_HalfSize;
		private readonly BoxCollider m_Collider;
		private readonly float3[] m_Vertices;
		private Collider[] m_ColliderHitBuffer;

		public FakeBoxCollider(BoxCollider boxCollider) : this(boxCollider.size)
		{
			m_Collider = boxCollider;
		}

		public FakeBoxCollider(BoxCollider boxCollider, int colliderBufferSize) : this(boxCollider.size)
		{
			m_Collider = boxCollider;
			m_ColliderHitBuffer = new Collider[colliderBufferSize];
		}

		private FakeBoxCollider(float3 size)
        {
            m_Size = size;
            m_HalfSize = 0.5f * size;

			m_Vertices = new float3[8];
			m_Vertices[0] = new float3(-m_HalfSize.x, -m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[1] = new float3(-m_HalfSize.x, -m_HalfSize.y, m_HalfSize.z);
			m_Vertices[2] = new float3(-m_HalfSize.x, m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[3] = new float3(-m_HalfSize.x, m_HalfSize.y, m_HalfSize.z);
			m_Vertices[4] = new float3(m_HalfSize.x, -m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[5] = new float3(m_HalfSize.x, -m_HalfSize.y, m_HalfSize.z);
			m_Vertices[6] = new float3(m_HalfSize.x, m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[7] = new float3(m_HalfSize.x, m_HalfSize.y, m_HalfSize.z);
		}

        public float3 Size => m_Size;

		public Collider Collider => m_Collider;

        public float3 CalculateInverseInertiaTensor(float mass)
        {
			var inertiaTensor = new float3()
			{
				x = (mass / 12.0f) * (m_Size.y * m_Size.y + m_Size.z * m_Size.z),
				y = (mass / 12.0f) * (m_Size.x * m_Size.x + m_Size.z * m_Size.z),
				z = (mass / 12.0f) * (m_Size.x * m_Size.x + m_Size.y * m_Size.y),
			};

			return new float3()
			{
				x = 1.0f / inertiaTensor.x,
				y = 1.0f / inertiaTensor.y,
				z = 1.0f / inertiaTensor.z,
			};
		}

		public bool Intersects(in FakePose selfPose, IFakeCollider otherCollider, in FakePose otherPose, out FakeContact contactPair)
		{
			//if (m_Collider.bounds.Intersects(otherCollider.Collider.bounds) is false)
			//{
			//	contactPair = default;
			//	return false;
			//}

			var isOverlaped = Physics.ComputePenetration(
				m_Collider,
				selfPose.Position,
				selfPose.Rotation,
				otherCollider.Collider,
				otherPose.Position,
				otherPose.Rotation,
				out var direction,
				out var distance);

			//Debug.Log(Physics.BoxCast(
			//		selfPose.Position,
			//		m_HalfSize,
			//		Vector3.down,
			//		out RaycastHit hitInfo,
			//		selfPose.Rotation,
			//		1.0f));

			if (isOverlaped)
			{
				_ = Physics.BoxCast(
					selfPose.Position + (float3)direction,
					m_HalfSize,
					-direction,
					out RaycastHit hitInfo,
					selfPose.Rotation,
					1.0f);

				//var point = m_Collider.ClosestPoint(otherPose.Position);
				//var contacts = GetContactPoints(in selfPose, this, in otherPose, otherCollider as FakeBoxCollider, direction, distance);

				//foreach (var contact in contacts)
				//{
				//	Debug.Log($"> {contacts.Count} {contact}");
				//}

				var point = hitInfo.point;
				contactPair = new FakeContact(point, direction, distance);
			}
			else
			{
				contactPair = default;
			}

			return isOverlaped;
		}

		public static List<float3> GetContactPoints(in FakePose selfPose, FakeBoxCollider selfBox, in FakePose otherPose, FakeBoxCollider otherBox, float3 normal, float penetrationDepth)
		{
			var contactPoints = new List<float3>();

			foreach (var vertex in selfBox.m_Vertices)
			{
				var worldVertex = selfPose.Position + math.mul(selfPose.Rotation, vertex);

				Debug.Log($">> {worldVertex} {math.dot(worldVertex - otherPose.Position, normal)} {penetrationDepth}");

				if (math.dot(worldVertex - otherPose.Position, normal) < penetrationDepth)
				{
					contactPoints.Add(worldVertex);
				}
			}

			foreach (var vertex in otherBox.m_Vertices)
			{
				var worldVertex = otherPose.Position + math.mul(otherPose.Rotation, vertex);

				Debug.Log($">> {worldVertex} {math.dot(worldVertex - selfPose.Position, normal)} {penetrationDepth}");

				if (math.dot(worldVertex - selfPose.Position, normal) < penetrationDepth)
				{
					contactPoints.Add(worldVertex);
				}
			}

			return contactPoints;
		}

		//public FakeRawContactPair[] GetContacts(in FakePose pose)
		//{
		//	var count = Physics.OverlapBoxNonAlloc(
		//		pose.Position,
		//		m_HalfSize,
		//		m_ColliderHitBuffer,
		//		pose.Rotation);

		//	var result = new FakeRawContactPair[count - 1];
		//	var index = 0;

		//	for (int i = 0; i < count; i++)
		//	{
		//		var otherCollider = m_ColliderHitBuffer[i];

		//		if (otherCollider == m_Collider) { continue; }

		//		var otherPosition = otherCollider.attachedRigidbody.position;
		//		var otherRotation = otherCollider.attachedRigidbody.rotation;

		//		_ = Physics.ComputePenetration(
		//			m_Collider,
		//			pose.Position,
		//			pose.Rotation,
		//			otherCollider,
		//			otherPosition,
		//			otherRotation,
		//			out var direction,
		//			out var distance);

		//		var point = m_Collider.ClosestPoint(otherPosition);

		//		Debug.DrawRay(otherPosition, direction * distance, Color.red);
		//		Debug.Log($"> P {point} N {direction} D {distance}");

		//		result[index++] = new FakeRawContactPair(point, direction, distance);
		//	}

		//	return result;
		//}

		//private void AddContact(FakeBody body0, FakeBody body1, float3 point, float3 normal, float depth)
		//{
		//	var pointVelocity0 = body0.GetVelocityAt(point);
		//	var pointVelocity1 = body1?.GetVelocityAt(point) ?? float3.zero;
		//	var projection0 = math.dot(pointVelocity0, normal);
		//	var projection1 = math.dot(pointVelocity1, normal);

		//	// simplified (pointVel0 - proj0 * normal) - (pointVel1 - proj1 * normal)
		//	// delta of velocities' parts tangential to normal
		//	var deltaV = pointVelocity0 - pointVelocity1 - (projection0 - projection1) * normal;
		//	var deltaVLength = math.length(deltaV);
		//	var deltaVDirection = float3.zero;

		//	if (deltaVLength != 0.0f)
		//	{
		//		deltaVDirection = deltaV / deltaVLength;
		//	}

		//	var friction = -100.0f;
		//	m_Contacts.Add(new FakeContactPair(
		//		body0,
		//		body1,
		//		point,
		//		normal,
		//		math.max(0.0f, depth),
		//		deltaVDirection,
		//		deltaVLength,
		//		friction));
		//}
	}
}
