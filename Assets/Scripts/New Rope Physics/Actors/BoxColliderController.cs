using UnityEngine;

namespace RopePhysics
{
	public class BoxColliderController : MonoBehaviour, IPrimaryActorController
	{
		[SerializeField] private Vector3 m_Size;

		private BoxCollider m_BoxCollider;

		public void InitWithSolver(Solver solver)
		{
			m_BoxCollider = new BoxCollider(solver.NextParticleId, transform.position, m_Size);
			solver.Register(m_BoxCollider);
		}

		private void Update()
		{
			var center = m_BoxCollider.GetCenter();
			var rotation = m_BoxCollider.GetRotation(center);

			transform.SetPositionAndRotation(center, rotation);
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_BoxCollider == null)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(transform.position, m_Size);
			}
			else
			{
				var length = m_BoxCollider.ParticlesCopy.Length;
				for (int i = 0; i < length; i++)
				{
					Gizmos.color = Color.Lerp(Color.black, Color.white, (float)i / (float)(length - 1));
					Gizmos.DrawCube(m_BoxCollider.ParticlesCopy[i].Position, 0.1f * Vector3.one);
				}

				Gizmos.color = Color.green;

				for (int i = 0; i < m_BoxCollider.DistanceConstraints.Length; i++)
				{
					var constraint = m_BoxCollider.DistanceConstraints[i];

					Gizmos.DrawLine(
						m_BoxCollider.ParticlesCopy[constraint.Index0 - m_BoxCollider.StartIndexOfParticles].Position,
						m_BoxCollider.ParticlesCopy[constraint.Index1 - m_BoxCollider.StartIndexOfParticles].Position);
				}
			}
		}

#endif

		public void ActualiaseFromSolver(Solver solver)
		{
			m_BoxCollider.UpdateWith(solver.Particles, solver.Constraints);
		}

		public void ActualiaseToSolver(Solver solver) { }

		public void Dispose()
		{
			m_BoxCollider?.Dispose();
		}
	}
}
