using System;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
	public class FakeSolver : MonoBehaviour
	{
		[SerializeField] private float3 m_GravitationalAcceleration = Physics.gravity;
		[Min(1)]
		[SerializeField] private int m_SubstepIteractionsNumber = 1;
		[Min(1)]
		[SerializeField] private int m_SolvePositionIteractionsNumber = 1;

		private FakeBody[] m_Bodies;

		private void OnEnable()
		{
			m_Bodies = GetComponentsInChildren<FakeBody>();

			Array.ForEach(m_Bodies, body => body.Init());
		}

		private void FixedUpdate()
		{
			var substepDeltaTime = Time.deltaTime / m_SubstepIteractionsNumber;

			for (int i = 0; i < m_SubstepIteractionsNumber; i++)
			{
				Array.ForEach(m_Bodies, body =>
				{
					body.Step(substepDeltaTime, m_GravitationalAcceleration);
					body.EndStep(substepDeltaTime);
					body.UpdatePresentation();
				});
			}
		}
	}
}
