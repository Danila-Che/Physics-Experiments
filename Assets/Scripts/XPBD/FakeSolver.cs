using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
	internal class FakeSolver
	{
		private readonly float3 m_GravitationalAcceleration = Physics.gravity;
		private readonly int m_SubstepIteractionsNumber = 1;
		private readonly int m_SolvePositionIteractionsNumber = 1;

		private readonly List<FakeBody> m_Bodies;
		private readonly List<float3> m_Forces;
		private readonly List<FakeJoint> m_Joints;

        public FakeSolver(float3 gravitationalAcceleration, int substepIteractionsNumber, int solvePositionIteractionsNumber)
        {
            m_GravitationalAcceleration = gravitationalAcceleration;
            m_SubstepIteractionsNumber = substepIteractionsNumber;
            m_SolvePositionIteractionsNumber = solvePositionIteractionsNumber;

			m_Bodies = new List<FakeBody>();
			m_Forces = new List<float3>();
			m_Joints = new List<FakeJoint>();
        }

		public void AddBody(FakeBody body)
        {
            if (m_Bodies.Contains(body))
                return;
            
            m_Bodies.Add(body);
            m_Forces.Add(float3.zero);
        }

		public void AddJoint(FakeJoint joint)
        {
            if (m_Joints.Contains(joint) is false)
			{
                m_Joints.Add(joint);
			}
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

		private void Step(float detlaTime)
		{
			var substepDeltaTime = detlaTime / m_SubstepIteractionsNumber;

			for (int i = 0; i < m_SubstepIteractionsNumber; i++)
			{
				for (int j = 0; j < m_Bodies.Count; j++)
				{
					m_Bodies[i].BeginStep();
				}

				// resolve collision contacts

				for (int j = 0; j < m_Bodies.Count; j++)
				{
					m_Bodies[i].BeginStep();
					m_Bodies[i].ApplyAcceleration(substepDeltaTime, m_GravitationalAcceleration);
					m_Bodies[i].ApplyDrag(substepDeltaTime);
					m_Bodies[i].Step(substepDeltaTime);
					m_Bodies[i].EndStep(substepDeltaTime);
				}

				for (var j = 0; j < m_Bodies.Count; j++)
				{
					m_Forces[j] = Vector3.zero;
				}
			}
		}
	}
}
