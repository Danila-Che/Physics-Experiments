using Unity.Mathematics;
using UnityEngine;

namespace CustomCollisionDetection
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
	public class CustomBoxCollider : MonoBehaviour
    {
        private const int k_BoxVerticesCount = 8;

        private Rigidbody m_Rigidbody;
        private BoxCollider m_BoxCollider;
        private float3 m_HalfSize;
		private float3[] m_Vertices;
        private float3[] m_WorldVertices;

		private float3 m_Position;
		private quaternion m_Rotation;
		private Bounds m_Bounds;

        public float3 Position => m_Rigidbody.position;

		public float3[] WorldVertices => m_WorldVertices;

		public Bounds Bounds => m_Bounds;

		public bool IsIntersects { get; set; } = false;

        public bool IsCollide { get; set; } = false;

		public void Init()
        {
            m_BoxCollider = GetComponent<BoxCollider>();
            m_Rigidbody = GetComponent<Rigidbody>();

			Debug.Log($"> {m_Rigidbody.inertiaTensor}");

            m_Rigidbody.useGravity = false;
            m_Rigidbody.isKinematic = true;
            m_BoxCollider.isTrigger = true;

            m_HalfSize = 0.5f * (float3)m_BoxCollider.size * (float3)transform.localScale;

            m_Vertices = new float3[k_BoxVerticesCount];
            m_WorldVertices = new float3[k_BoxVerticesCount];

			m_Vertices[0] = new float3(-m_HalfSize.x, -m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[1] = new float3(-m_HalfSize.x, -m_HalfSize.y, m_HalfSize.z);
			m_Vertices[2] = new float3(-m_HalfSize.x, m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[3] = new float3(-m_HalfSize.x, m_HalfSize.y, m_HalfSize.z);
			m_Vertices[4] = new float3(m_HalfSize.x, -m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[5] = new float3(m_HalfSize.x, -m_HalfSize.y, m_HalfSize.z);
			m_Vertices[6] = new float3(m_HalfSize.x, m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[7] = new float3(m_HalfSize.x, m_HalfSize.y, m_HalfSize.z);
		}

		public void OnBeginStep()
		{
			m_Position = (float3)m_Rigidbody.position;
			m_Rotation = (quaternion)m_Rigidbody.rotation;
			m_Bounds = m_BoxCollider.bounds;

			UpdateVertices();
		}

		public float3 FindFurthestPoint(float3 direction)
        {
			var result = m_WorldVertices[0];
			var maxDistance = math.dot(result, direction);

			for (int i = 1; i < m_WorldVertices.Length; i++)
			{
				var distance = math.dot(m_WorldVertices[i], direction);

				if (distance > maxDistance)
				{
					maxDistance = distance;
					result = m_WorldVertices[i];
				}
			}

			return result;
		}

		public bool TryCalculatePenetration(CustomBoxCollider otherCollider, out CollisionPoints collisionPoints)
		{
			var hasPenetration = Physics.ComputePenetration(
				m_BoxCollider,
				m_Rigidbody.position,
				m_Rigidbody.rotation,
				otherCollider.m_BoxCollider,
				otherCollider.m_Rigidbody.position,
				otherCollider.m_Rigidbody.rotation,
				out var direction,
				out var distance);

			if (hasPenetration)
			{
				collisionPoints = new CollisionPoints(-direction, distance);

				return true;
			}

			collisionPoints = default;
			return false;
		}

		private void UpdateVertices()
        {
			for (int i = 0; i < k_BoxVerticesCount; i++)
			{
				m_WorldVertices[i] = m_Position + math.mul(m_Rotation, m_Vertices[i]);
			}
		}
    }
}
