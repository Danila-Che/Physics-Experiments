using UnityEngine;
using UnityEngine.Assertions;

namespace RopePhysics
{
	public class AttachementController : MonoBehaviour
	{
		[SerializeField] private RigidbodyPointController[] m_Points;

		private Vector3[] m_InitialVectors;

		private void OnEnable()
		{
			m_InitialVectors = GetVectors();
		}

		private void Update()
		{
			Assert.IsTrue(m_Points.Length >= 3);

			var normal = CalculateNormal(m_Points[0].Position, m_Points[1].Position, m_Points[2].Position);
			var forward = m_Points[0].Position - m_Points[1].Position;

			Debug.DrawRay(transform.position, normal, Color.red);

			transform.rotation = Quaternion.LookRotation(forward, normal);

			//transform.rotation = CalculateRotation(m_InitialVectors, GetVectors());
		}

		private Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
		{
			var side1 = b - a;
			var side2 = c - a;

			return Vector3.Cross(side1, side2).normalized;
		}

		private Quaternion CalculateRotation(Vector3[] initialVectors, Vector3[] currentVectors)
		{
			if (initialVectors.Length != currentVectors.Length)
			{
				Debug.LogError("Vectors arrays must have the same length.");
				return Quaternion.identity;
			}

			//Vector3 centroidInitial = Vector3.zero;
			//Vector3 centroidCurrent = Vector3.zero;

			//for (int i = 0; i < initialVectors.Length; i++)
			//{
			//	centroidInitial += initialVectors[i];
			//	centroidCurrent += currentVectors[i];
			//}

			//centroidInitial /= initialVectors.Length;
			//centroidCurrent /= currentVectors.Length;

			Matrix4x4 covarianceMatrix = Matrix4x4.zero;

			for (int i = 0; i < initialVectors.Length; i++)
			{
				Vector3 initialVector = initialVectors[i];// - centroidInitial;
				Vector3 currentVector = currentVectors[i];// - centroidCurrent;

				covarianceMatrix.m00 += initialVector.x * currentVector.x;
				covarianceMatrix.m01 += initialVector.x * currentVector.y;
				covarianceMatrix.m02 += initialVector.x * currentVector.z;

				covarianceMatrix.m10 += initialVector.y * currentVector.x;
				covarianceMatrix.m11 += initialVector.y * currentVector.y;
				covarianceMatrix.m12 += initialVector.y * currentVector.z;

				covarianceMatrix.m20 += initialVector.z * currentVector.x;
				covarianceMatrix.m21 += initialVector.z * currentVector.y;
				covarianceMatrix.m22 += initialVector.z * currentVector.z;
			}

			Quaternion rotation = QuaternionFromMatrix(covarianceMatrix);
			return rotation;
		}

		private Quaternion QuaternionFromMatrix(Matrix4x4 m)
		{
			Quaternion q = new Quaternion();
			q.w = Mathf.Sqrt(1.0f + m.m00 + m.m11 + m.m22) / 2.0f;
			float w4 = (4.0f * q.w);
			q.x = (m.m21 - m.m12) / w4;
			q.y = (m.m02 - m.m20) / w4;
			q.z = (m.m10 - m.m01) / w4;
			return q;
		}

		private Vector3[] GetVectors()
		{
			var vectors = new Vector3[m_Points.Length];

			for (int i = 0; i < m_Points.Length; i++)
			{
				vectors[i] = m_Points[i].Position - transform.position;
			}

			return vectors;
		}
	}
}
