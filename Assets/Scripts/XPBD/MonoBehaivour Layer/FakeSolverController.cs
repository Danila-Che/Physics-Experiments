using Unity.Mathematics;
using UnityEngine;
using XPBD;

namespace FakeXPBDMonoBehaviour
{
	internal enum ExecutionOrder
	{
		Solver,
		Collider,
		Body,
		Joint
	}

	[DefaultExecutionOrder((int)ExecutionOrder.Solver)]
    public class FakeSolverController : MonoBehaviour
    {
		[SerializeField] private float3 m_GravitationalAcceleration = Physics.gravity;
		[Min(1)]
		[SerializeField] private int m_SubstepIteractionsNumber = 1;
		[Min(1)]
		[SerializeField] private int m_SolvePositionIteractionsNumber = 1;
		[Min(1)]
		[SerializeField] private int m_SolverCollisionIteractionNumber = 1;

		private FakeCollisionSystem m_FakeCollisionSystem;
		private FakeSolver m_Solver;

		private void OnEnable()
		{
			m_FakeCollisionSystem = new FakeCollisionSystem();

			m_Solver = new FakeSolver(
				m_GravitationalAcceleration,
				m_SubstepIteractionsNumber,
				m_SolvePositionIteractionsNumber,
				m_SolverCollisionIteractionNumber,
				m_FakeCollisionSystem);
		}

		private void OnDisable()
		{
			m_FakeCollisionSystem?.Dispose();
			m_FakeCollisionSystem = null;
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_FakeCollisionSystem == null) { return; }

			for (int i = 0; i < m_FakeCollisionSystem.Contacts.Count; i++)
			{
				var contact = m_FakeCollisionSystem.Contacts[i];

				Gizmos.color = Color.green;
				Gizmos.DrawSphere(contact.Point, 0.1f);

				Gizmos.color = Color.blue;
				Gizmos.DrawLine(contact.Point, contact.Point + contact.DeltaVDirection);

				Gizmos.color = Color.red;
				Gizmos.DrawLine(contact.Point, contact.Point + contact.Normal);
			}
		}

#endif

		private void FixedUpdate()
		{
			m_Solver.Step(Time.fixedDeltaTime);
		}

		public void RegisterActor(IActor actor)
		{
			m_Solver.AddActor(actor);
		}

		public void UnregisterActor(IActor actor)
		{
			m_Solver.RemoveActor(actor);
		}

		public void RegisterConstrainable(IConstrainable constrainable)
		{
			m_Solver.AddConstrainable(constrainable);
		}

		public void UnregisterConstrainable(IConstrainable constrainable)
		{
			m_Solver.RemoveConstrainable(constrainable);
		}

		public void RegisterBody(FakeBody body)
		{
			m_Solver.AddBody(body);
		}

		public void UnregisterBody(FakeBody body)
		{
			m_Solver.RemoveBody(body);
		}

		public void RegisterJoint(FakeJoint joint)
		{
			m_Solver.AddJoint(joint);
		}

		public void UnregisterJoint(FakeJoint joint)
		{
			m_Solver.RemoveJoint(joint);
		}

		public void RegisterModifier(int bodyId, FakeBody body) { }

		public void UnregisterModifier(int bodyId) { }
	}
}
