using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
	public class SolverController : MonoBehaviour
	{
		[SerializeField] private bool m_UseCustomGravity = false;
		[SerializeField] private float3 m_Gravity = new(0.0f, -9.807f, 0.0f);
		[SerializeField] private bool m_NeedDistanceConstraint = true;
		[SerializeField] private bool m_NeedCollisionDetection = true;
		[SerializeField] private bool m_NeedSimulateUnityPhysics = false;
		[Min(1)]
		[SerializeField] private int m_DistanceConstraintsIterations = 1;
		[Min(0.0f)]
		[SerializeField] private float m_ParticleColliderRadius = 0.1f;
		[Min(0)]
		[SerializeField] private int m_ColliderBufferSize = 1;

		private Solver m_Solver;
		private List<IActorController> m_ActorController;
		private SimulationMode m_SimulationMode;

		private void OnEnable()
		{
			m_SimulationMode = Physics.simulationMode;

			if (m_NeedSimulateUnityPhysics)
			{
				Physics.simulationMode = SimulationMode.Script;
			}

			var gravity = m_UseCustomGravity ? m_Gravity : (float3)Physics.gravity;

			m_Solver = new Solver(
				gravity,
				m_DistanceConstraintsIterations,
				m_NeedDistanceConstraint,
				m_NeedCollisionDetection,
				m_ParticleColliderRadius,
				m_ColliderBufferSize);

			var primaryActorController = GetComponentsInChildren<IPrimaryActorController>();
			var secondaryActorController = GetComponentsInChildren<ISecondaryActorController>();

			foreach (var actor in primaryActorController)
			{
				actor.InitWithSolver(m_Solver);
			}

			foreach (var actor in secondaryActorController)
			{
				actor.InitWithSolver(m_Solver);
			}

			m_ActorController = new List<IActorController>(primaryActorController.Length + secondaryActorController.Length);
			m_ActorController.AddRange(primaryActorController);
			m_ActorController.AddRange(secondaryActorController);
		}

		private void OnDisable()
		{
			Physics.simulationMode = m_SimulationMode;

			m_Solver.Dispose();
			m_Solver = null;

			foreach (var actor in m_ActorController)
			{
				actor.Dispose();
			}
		}

		private void FixedUpdate()
		{
			m_Solver.BeginStep();
			m_Solver.Step(Time.fixedDeltaTime);

			if (m_NeedCollisionDetection)
			{
				m_Solver.AdjustCollisions();
			}

			m_Solver.EndStep();

			foreach (var actor in m_ActorController)
			{
				actor.ActualiaseFromSolver(m_Solver);
			}

			if (m_NeedSimulateUnityPhysics)
			{
				Physics.Simulate(Time.fixedDeltaTime);

				foreach (var actor in m_ActorController)
				{
					actor.ActualiaseToSolver(m_Solver);
				}
			}
		}
	}
}
