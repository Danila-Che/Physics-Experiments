using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
	public class FakeSolver
	{
		private readonly float3 m_GravitationalAcceleration = Physics.gravity;
		private readonly int m_SubstepIteractionsNumber = 1;
		private readonly int m_SolvePositionIteractionsNumber = 1;
		private readonly int m_SolverCollisionIteractionNumber = 1;

		private readonly List<FakeBody> m_Bodies;
		private readonly List<IActor> m_Actors;
		private readonly List<IConstrainable> m_Constrainables;
		private readonly List<float3> m_Forces;
		private readonly List<FakeJoint> m_Joints;

		private readonly FakeCollisionSystem m_FakeCollisionSystem;

		public FakeSolver(
			float3 gravitationalAcceleration,
			int substepIteractionsNumber,
			int solvePositionIteractionsNumber,
			int solverCollisionIteractionNumber,
			FakeCollisionSystem fakeCollisionSystem)
		{
			m_GravitationalAcceleration = gravitationalAcceleration;
			m_SubstepIteractionsNumber = substepIteractionsNumber;
			m_SolvePositionIteractionsNumber = solvePositionIteractionsNumber;
			m_SolverCollisionIteractionNumber = solverCollisionIteractionNumber;

			m_Bodies = new List<FakeBody>();
			m_Actors = new List<IActor>();
			m_Constrainables = new List<IConstrainable>();
			m_Forces = new List<float3>();
			m_Joints = new List<FakeJoint>();
			m_FakeCollisionSystem = fakeCollisionSystem;
		}

		public void AddActor(IActor actor)
		{
			if (m_Actors.Contains(actor)) return;

			m_Actors.Add(actor);
		}

		public void RemoveActor(IActor actor)
		{
			m_Actors.Remove(actor);
		}

		public void AddConstrainable(IConstrainable constrainable)
		{
			if (m_Constrainables.Contains(constrainable)) return;

			m_Constrainables.Add(constrainable);
		}

		public void RemoveConstrainable(IConstrainable constrainable)
		{
			m_Constrainables.Remove(constrainable);
		}

		public void AddBody(FakeBody body)
		{
			if (m_Bodies.Contains(body)) { return; }

			m_Bodies.Add(body);
			m_Forces.Add(float3.zero);
		}

		public void RemoveBody(FakeBody body)
		{
			// TODO remove linear search
			var index = m_Bodies.IndexOf(body);

			if (index == -1)
			{
				return;
			}

			m_Bodies.RemoveAt(index);
			m_Forces.RemoveAt(index);
		}

		public void AddJoint(FakeJoint joint)
		{
			if (m_Joints.Contains(joint) is false)
			{
				m_Joints.Add(joint);
			}
		}

		public void RemoveJoint(FakeJoint joint)
		{
			m_Joints.Remove(joint);
		}

		public void AddForce(FakeBody body, float3 force)
		{
			// TODO remove linear search
			var index = m_Bodies.IndexOf(body);

			if (index == -1)
			{
				return;
			}

			m_Forces[index] += force;
		}

		public void Step(float deltaTime)
		{
			var substepDeltaTime = deltaTime / m_SubstepIteractionsNumber;

			for (int iteration = 0; iteration < m_SubstepIteractionsNumber; iteration++)
			{
				for (int i = 0; i < m_Bodies.Count; i++)
				{
					if (m_Bodies[i].IsKinematic) { continue; }

					m_Bodies[i].BeginStep();
				}

				for (int i = 0; i < m_Actors.Count; i++)
				{
					m_Actors[i].BeginStep();
				}

				var contacts = m_FakeCollisionSystem.Contacts;
				for (int i = 0; i < contacts.Count; i++)
				{
					var contact = contacts[i];
					var limit = contact.Friction * substepDeltaTime;

					limit = contact.Body0?.CalculateFirctionForceLimit(
						limit,
						contact.Normal,
						contact.Point,
						contact.DeltaVDirection,
						contact.DeltaVLength) ?? limit;

					limit = contact.Body1?.CalculateFirctionForceLimit(
						limit,
						-contact.Normal,
						contact.Point,
						-contact.DeltaVDirection,
						contact.DeltaVLength) ?? limit;

					contact.Body0?.ApplyCorrection(limit * contact.DeltaVDirection, contact.Point, true);
					contact.Body1?.ApplyCorrection(-limit * contact.DeltaVDirection, contact.Point, true);
				}

				for (int i = 0; i < m_Bodies.Count; i++)
				{
					var body = m_Bodies[i];

					if (body.IsKinematic) { continue; }

					body.ApplyAcceleration(substepDeltaTime, m_GravitationalAcceleration);
					body.ApplyDrag(substepDeltaTime);
					body.Step(substepDeltaTime);
				}

				for (int i = 0; i < m_Actors.Count; i++)
				{
					var actor = m_Actors[i];

					actor.ApplyAcceleration(substepDeltaTime, m_GravitationalAcceleration);
					actor.ApplyDrag(substepDeltaTime);
					actor.Step(substepDeltaTime);
				}

				m_FakeCollisionSystem.SolveCollision(m_Bodies, substepDeltaTime);

				for (int i = 0; i < m_Joints.Count; i++)
				{
					m_Joints[i].SolvePosition(substepDeltaTime);
				}

				for (int i = 0; i < m_Constrainables.Count; i++)
				{
					m_Constrainables[i].SolveConstraints(substepDeltaTime);
				}

				m_FakeCollisionSystem.SolveCollision(m_Bodies, substepDeltaTime);
				m_FakeCollisionSystem.ClearContacts();

				for (int i = 0; i < m_Bodies.Count; i++)
				{
					if (m_Bodies[i].IsKinematic) { continue; }

					m_Bodies[i].EndStep(substepDeltaTime);
				}

				for (int i = 0; i < m_Actors.Count; i++)
				{
					m_Actors[i].EndStep(substepDeltaTime);
				}

				for (int i = 0; i < m_Joints.Count; i++)
				{
					m_Joints[i].SolveVelocity(substepDeltaTime);
				}
			}

			for (var i = 0; i < m_Forces.Count; i++)
			{
				m_Forces[i] = Vector3.zero;
			}
		}
	}
}
