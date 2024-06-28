using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics
{
	using static ParticleAttachment;

	public class Rope : MonoBehaviour, IContainer
	{
		public struct ParticleAlias
		{
			public int Index;
		}

		[Min(0.01f)]
		[SerializeField] private float m_ParticleDistance = 1.0f;

		private TopParticleAttachment m_TopParticleAttachment;
		private BottomParticleAttachment m_BottomParticleAttachment;

		private JobHandle m_Job;
		private NativeList<Particle> m_Particles;
		private NativeList<DistanceConstraint> m_DistanceConstraints;
		private NativeArray<AttachmentConstraint> m_AttachmentConstraints;
		/// <summary>
		/// Key for ParticleAttachment
		/// Value for index of AttachmentConstraint in m_AttachmentConstraints
		/// </summary>
		private Dictionary<ParticleAttachment, int> m_ParticleAttachmentMap;

		private AttachmentParticle m_StartRopeFrom = AttachmentParticle.Bottom;

		public JobHandle Job { get => m_Job; set => m_Job = value; }

		public NativeList<Particle> Particles => m_Particles;

		public NativeList<DistanceConstraint> DistanceConstraints => m_DistanceConstraints;

		public NativeArray<AttachmentConstraint> AttachmentConstraints => m_AttachmentConstraints;

#if UNITY_INCLUDE_TESTS && UNITY_EDITOR

		public float Test_ParticleDistance { get => m_ParticleDistance; set => m_ParticleDistance = value; }
		public NativeList<DistanceConstraint> Test_DistanceConstraints { get => m_DistanceConstraints; set => m_DistanceConstraints = value; }
		public AttachmentParticle Test_StartRopeFrom { get => m_StartRopeFrom; set => m_StartRopeFrom = value; }

		public void OnStart() => Start();
		public void Destroy() => OnDestroy();

#endif

		private void Start()
		{
			m_TopParticleAttachment = GetComponent<TopParticleAttachment>();
			m_BottomParticleAttachment = GetComponent<BottomParticleAttachment>();

			m_ParticleAttachmentMap = new Dictionary<ParticleAttachment, int>();
			m_Particles = new NativeList<Particle>(Allocator.Persistent);
			m_DistanceConstraints = new NativeList<DistanceConstraint>(Allocator.Persistent);
			m_AttachmentConstraints = CreateAttachmentConstraints();

			CreateRope();
			//RegisterInSolver();
		}

		private void OnDestroy()
		{
			m_ParticleAttachmentMap = null;

			if (m_Particles.IsCreated)
			{
				m_Particles.Dispose();
			}

			if (m_DistanceConstraints.IsCreated)
			{
				m_DistanceConstraints.Dispose();
			}

			if (m_AttachmentConstraints.IsCreated)
			{
				m_AttachmentConstraints.Dispose();
			}
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_Particles.IsCreated)
			{
				Gizmos.color = Color.green;

				m_Job.Complete();

				for (int i = 0; i < m_Particles.Length; i++)
				{
					Gizmos.DrawSphere(m_Particles[i].Position, 0.1f);
				}
			}
		}

#endif

		public void UpdateContainer()
		{
			//foreach (var pair in m_ParticleAttachmentMap)
			//{
			//	var constraint = m_AttachmentConstraints[pair.Value];

			//	constraint.Position = pair.Key.Taget.transform.position;

			//	m_AttachmentConstraints[pair.Value] = constraint;
			//}
		}

		private NativeArray<AttachmentConstraint> CreateAttachmentConstraints()
		{
			if (m_TopParticleAttachment.Type is AttachmentType.Static)
			{
				return new NativeArray<AttachmentConstraint>(1, Allocator.Persistent);
			}
			else
			{
				return new NativeArray<AttachmentConstraint>(0, Allocator.Persistent);
			}
		}

		private void CreateRope()
		{
			var topPosition = m_TopParticleAttachment.Taget.transform.position;
			var bottomPosition = m_BottomParticleAttachment.Taget.transform.position;

			if (TryGetComponent<RopeCursor>(out var cursor))
			{
				if (m_TopParticleAttachment == cursor.ParticleAttachment)
				{
					CreateRope(bottomPosition, topPosition);
				}
				else
				{
					CreateRope(topPosition, bottomPosition);
				}
			}
		}

		private void CreateRope(Vector3 startParticlePosition, Vector3 finishParticlePosition)
		{
			var vector = finishParticlePosition - startParticlePosition;
			var normal = vector.normalized;
			var particleCount = (vector.magnitude % m_ParticleDistance == 0.0f) switch
			{
				true => 1 + (int)(vector.magnitude / m_ParticleDistance),
				false => 2 + (int)(vector.magnitude / m_ParticleDistance),
			};
			var constraintCount = particleCount - 1;

			m_Particles.SetCapacity(particleCount);
			m_DistanceConstraints.SetCapacity(constraintCount);

			for (int i = 0; i < particleCount - 1; i++)
			{
				var position = startParticlePosition + i * m_ParticleDistance * normal;

				m_Particles.AddNoResize(new Particle(position));
			}

			m_Particles.AddNoResize(new Particle(finishParticlePosition));

			for (int i = 0; i < constraintCount; i++)
			{
				var isIndex0Free = true;
				var isIndex1Free = true;

				for (int j = 0; j < m_AttachmentConstraints.Length; j++)
				{
					if (m_AttachmentConstraints[j].Index == i)
					{
						isIndex0Free &= false;
					}

					if (m_AttachmentConstraints[j].Index == i + 1)
					{
						isIndex1Free &= false;
					}
				}

				m_DistanceConstraints.AddNoResize(CreateDistanceConstraint(i, isIndex0Free, isIndex1Free));
			}
		}

		private void InitAttachmentConstraint()
		{
			if (TryGetComponent<RopeCursor>(out var cursor))
			{
				if (m_TopParticleAttachment == cursor.ParticleAttachment)
				{
					m_AttachmentConstraints[0] = new AttachmentConstraint(m_Particles.Length - 1);
				}
				else if (m_BottomParticleAttachment == cursor.ParticleAttachment)
				{
					m_AttachmentConstraints[0] = new AttachmentConstraint(0);
				}
			}
		}

		//     private IEnumerator<float3> IterateParticlePosition()
		//     {
		//var position0 = m_ParticleAttachments[0].Taget.transform.position;
		//var position1 = m_ParticleAttachments[1].Taget.transform.position;

		//yield return float3.zero;
		//     }

		//private void RegisterInSolver()
		//{
		//	var solver = GetComponentInParent<RopeSolver>();

		//	if (solver != null)
		//	{
		//		solver.RegisterContainer(this);
		//	}
		//}

		private DistanceConstraint CreateDistanceConstraint(int particleIndex, bool isTopParticleFree, bool isBottomParticleFree)
		{
			var distance = math.length(m_Particles[particleIndex].Position - m_Particles[particleIndex + 1].Position);

			return new DistanceConstraint(particleIndex, isTopParticleFree, particleIndex + 1, isBottomParticleFree, distance);
		}
	}
}
