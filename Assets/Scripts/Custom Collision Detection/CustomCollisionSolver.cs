using System;
using UnityEngine;

namespace CustomCollisionDetection
{
	public class CustomCollisionSolver : MonoBehaviour
	{
		[SerializeField] private int m_MaxDetectionCount = 1;

		private CustomBoxCollider[] m_Colliders;

		private void OnEnable()
		{
			m_Colliders = GetComponentsInChildren<CustomBoxCollider>();

			Array.ForEach(m_Colliders, collider => collider.Init());
		}

		private void FixedUpdate()
		{
			Array.ForEach(m_Colliders, collider =>
			{
				collider.ResolveAsseleration(Physics.gravity, Time.deltaTime);
				collider.ResolveCollision(m_MaxDetectionCount);
			});
		}
	}
}
