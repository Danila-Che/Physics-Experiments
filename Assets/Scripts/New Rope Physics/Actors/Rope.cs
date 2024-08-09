using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	public class Rope : IActor
	{
		private readonly float m_SpanDistance;
		private readonly float m_MassOfParticle;
		private int m_StartIndexOfParticles;
		private NativeArray<DistanceConstraint> m_DistanceConstraints;
		private NativeArray<Particle> m_Particles;
		private NativeArray<Particle> m_ParitclesCopy;

		private int m_EdgeParticle0Index;
		private int m_EdgeParticle1Index;
		private Particle m_EdgeParticle0;
		private Particle m_EdgeParticle1;

		private AttachmentConstraint m_TargetParticleAttachmentConstraint;
		private AttachmentConstraint m_SourceParticleAttachmentConstraint;

		private readonly IAttachmentConstraintController m_TargetAttachmentConstraintController;
		private readonly IAttachmentConstraintController m_SourceAttachmentConstraintController;

		private JobHandle m_Job;

		public Rope() : this(spanDistance: 1.0f, massOfParticle: 1.0f) { }

		public Rope(float spanDistance) : this(spanDistance, massOfParticle: 1.0f) { }

		public Rope(float spanDistance, AttachmentConstraintAlias targetAttachmentConstraint, AttachmentConstraintAlias sourceAttachmentConstraint)
		{
			m_TargetParticleAttachmentConstraint = new AttachmentConstraint(isExists: false);
			m_SourceParticleAttachmentConstraint = new AttachmentConstraint(isExists: false);
		}

		public Rope(float spanDistance, float massOfParticle)
		{
			m_SpanDistance = spanDistance;
			m_MassOfParticle = massOfParticle;
		}

		public NativeArray<Particle> Particles => m_Particles;

		public NativeArray<DistanceConstraint> DistanceConstraints => m_DistanceConstraints;

		public NativeArray<Particle> ParitclesCopy => m_ParitclesCopy;

		public AttachmentConstraint TargetParticleAttachmentConstraint => m_TargetParticleAttachmentConstraint;

		public AttachmentConstraint SourceParticleAttachmentConstraint => m_SourceParticleAttachmentConstraint;

		public JobHandle Job { get => m_Job; set => m_Job = value; }

		public float3 EdgeParticle0Position => m_EdgeParticle0.Position;

		public float3 EdgeParticle1Position => m_EdgeParticle1.Position;

		public void Dispose()
		{
			m_Job.Complete();

			if (m_Particles.IsCreated)
			{
				m_Particles.Dispose();
			}

			if (m_ParitclesCopy.IsCreated)
			{
				m_ParitclesCopy.Dispose();
			}

			if (m_DistanceConstraints.IsCreated)
			{
				m_DistanceConstraints.Dispose();
			}
		}

		public void ActualisePositions()
		{
			m_TargetAttachmentConstraintController.ActualisePosition();
			m_SourceAttachmentConstraintController.ActualisePosition();

			m_TargetParticleAttachmentConstraint.Position = m_TargetAttachmentConstraintController.Position;
			m_SourceParticleAttachmentConstraint.Position = m_SourceAttachmentConstraintController.Position;
		}

		public void CreateRope()
		{
			CreateRope(m_SourceAttachmentConstraintController.Position, m_TargetAttachmentConstraintController.Position);
		}

		public void CreateRope(Solver solver, int startParticleIndex, int finishParticleIndex)
		{
			m_EdgeParticle0Index = finishParticleIndex;
			m_EdgeParticle1Index = startParticleIndex;

			var source = solver.Particles[startParticleIndex].Position;
			var target = solver.Particles[finishParticleIndex].Position;

			var vector = source - target;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = 1 + (int)math.ceil(magnitude / m_SpanDistance) - 2;

			m_Particles = new NativeArray<Particle>(particleCount, Allocator.Persistent);
			m_ParitclesCopy = new NativeArray<Particle>(particleCount + 2, Allocator.Persistent);
			m_DistanceConstraints = new NativeArray<DistanceConstraint>(particleCount + 1, Allocator.Persistent);

			for (int i = 0; i < m_Particles.Length; i++)
			{
				m_Particles[i] = new Particle(target + m_SpanDistance * (i + 1) * normal, m_MassOfParticle);
			}

			var offset = solver.NextParticleId;

			AddConstraint(0, startParticleIndex, offset + m_Particles.Length - 1, source, m_Particles[^1].Position);
			AddConstraint(1, finishParticleIndex, offset, target, m_Particles[0].Position);

			for (int i = 2, p = 0; i < m_DistanceConstraints.Length; i++, p++)
			{
				AddConstraint(i, offset + p, offset + p + 1, m_Particles[p].Position, m_Particles[p + 1].Position);
			}
		}

		private void AddConstraint(int i, int from, int to, float3 fromPositoin, float3 toPosition)
		{
			var distance = math.distance(fromPositoin, toPosition);

			m_DistanceConstraints[i] = new DistanceConstraint(distance, from, to);
		}

		public void CreateRope(float3 source, float3 target) { }

		public void Notify(int startIndexOfParticles, int startIndexOfConstraint)
		{
			m_StartIndexOfParticles = startIndexOfParticles;
		}

		public void UpdateWith(Solver solver)
		{
			m_EdgeParticle0 = solver.Particles[m_EdgeParticle0Index];
			m_EdgeParticle1 = solver.Particles[m_EdgeParticle1Index];

			m_ParitclesCopy[0] = m_EdgeParticle0;

			for (int i = 0; i < m_ParitclesCopy.Length - 2; i++)
			{
				m_ParitclesCopy[1 + i] = solver.Particles[m_StartIndexOfParticles + i];
			}

			m_ParitclesCopy[^1] = m_EdgeParticle1;
		}

		public void UpdateWith(NativeList<Particle> particles, NativeList<DistanceConstraint> distanceConstraints)
		{
			for (int i = 0; i < m_ParitclesCopy.Length; i++)
			{
				m_ParitclesCopy[i] = particles[m_StartIndexOfParticles + i];
			}
		}
	}
}
