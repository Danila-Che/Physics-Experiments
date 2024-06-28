using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
	public class SolverController : MonoBehaviour
	{
		[SerializeField] private float3 m_Gravity = new(0.0f, -9.807f, 0.0f);
		[SerializeField] private bool m_NeedDistanceConstraint = true;
		[Min(1)]
		[SerializeField] private int m_DistanceConstraintsIterations = 1;
		[Min(0.0f)]
		[SerializeField] private float m_ParticleColliderRadius = 0.1f;
		[Min(0)]
		[SerializeField] private int m_ColliderBufferSize = 1;

		private Solver m_Solver;

		private void OnEnable()
		{
			m_Solver = new Solver(m_Gravity, m_DistanceConstraintsIterations, m_NeedDistanceConstraint, m_ParticleColliderRadius, m_ColliderBufferSize);

			foreach (var ropeController in GetComponentsInChildren<RopeController>())
			{
				ropeController.RopeWasInitialized += OnRopeWasInitialized;
			}
		}

		private void OnDisable()
		{
			m_Solver = null;

			foreach (var ropeController in GetComponentsInChildren<RopeController>())
			{
				ropeController.RopeWasInitialized -= OnRopeWasInitialized;
			}
		}

		private void FixedUpdate()
		{
			m_Solver.BeginStep();
			m_Solver.Step(Time.fixedDeltaTime);
			m_Solver.EndStep();
		}

		private void OnRopeWasInitialized(object sender, Rope rope)
		{
			m_Solver.Register(rope);
		}
	}
}
