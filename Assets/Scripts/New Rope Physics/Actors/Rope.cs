using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	public class Rope : IActor
	{
		private readonly float m_SpanDistance;
		private int m_ParticleCount;
		private int m_StartIndexOfParticles;
		private NativeArray<DistanceConstraint> m_DistanceConstraints;
		private NativeArray<Particle> m_Particles;
		private NativeArray<Particle> m_ParitclesCopy;

		private AttachmentConstraint m_TargetParticleAttachmentConstraint;
		private AttachmentConstraint m_SourceParticleAttachmentConstraint;

		private readonly IAttachmentConstraintController m_TargetAttachmentConstraintController;
		private readonly IAttachmentConstraintController m_SourceAttachmentConstraintController;

		private JobHandle m_Job;

		public Rope() : this(1.0f) { }

		public Rope(float spanDistance, AttachmentConstraintAlias targetAttachmentConstraint, AttachmentConstraintAlias sourceAttachmentConstraint)
		{
			m_TargetParticleAttachmentConstraint = new AttachmentConstraint(isExists: false);
			m_SourceParticleAttachmentConstraint = new AttachmentConstraint(isExists: false);
		}

		public Rope(
			float spanDistance,
			IAttachmentConstraintController targetAttachmentConstraintController,
			IAttachmentConstraintController sourceAttachmentConstraintController)
			: this(spanDistance)
		{
			m_TargetAttachmentConstraintController = targetAttachmentConstraintController;
			m_SourceAttachmentConstraintController = sourceAttachmentConstraintController;

			m_TargetParticleAttachmentConstraint = new AttachmentConstraint(
				targetAttachmentConstraintController.Attachment,
				isExists: true);

			m_SourceParticleAttachmentConstraint = new AttachmentConstraint(
				sourceAttachmentConstraintController.Attachment,
				isExists: true);
		}

		public Rope(float spanDistance)
		{
			m_SpanDistance = spanDistance;
		}

		public NativeArray<Particle> Particles => m_Particles;

		public NativeArray<DistanceConstraint> DistanceConstraints => m_DistanceConstraints;

		public NativeArray<Particle> ParitclesCopy => m_ParitclesCopy;

		public AttachmentConstraint TargetParticleAttachmentConstraint => m_TargetParticleAttachmentConstraint;

		public AttachmentConstraint SourceParticleAttachmentConstraint => m_SourceParticleAttachmentConstraint;

		public bool HasAttachment => m_TargetParticleAttachmentConstraint.IsExists && m_SourceParticleAttachmentConstraint.IsExists;
		public bool AllAttachmentIsStatic =>
			m_TargetParticleAttachmentConstraint.Attachment is Attachment.Static
			&& m_SourceParticleAttachmentConstraint.Attachment is Attachment.Static;

		public JobHandle Job { get => m_Job; set => m_Job = value; }

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

		public void UpdatePositions()
		{
			m_TargetParticleAttachmentConstraint.Position = Particles[0].Position;
			m_SourceParticleAttachmentConstraint.Position = Particles[^1].Position;

			m_TargetAttachmentConstraintController.Resolve(ref m_TargetParticleAttachmentConstraint);
			m_SourceAttachmentConstraintController.Resolve(ref m_SourceParticleAttachmentConstraint);
		}

		public void CreateRope()
		{
			CreateRope(m_SourceAttachmentConstraintController.Position, m_TargetAttachmentConstraintController.Position);
		}

		public void CreateRope(Solver solver, int startParticleIndex, int finishParticleIndex)
		{
			var source = solver.Particles[startParticleIndex].Position;
			var target = solver.Particles[finishParticleIndex].Position;

			var vector = source - target;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = 1 + (int)math.ceil(magnitude / m_SpanDistance) - 2;

			m_ParticleCount = particleCount;

			m_Particles = new NativeArray<Particle>(particleCount, Allocator.Persistent);
			m_ParitclesCopy = new NativeArray<Particle>(particleCount, Allocator.Persistent);
			m_DistanceConstraints = new NativeArray<DistanceConstraint>(particleCount + 1, Allocator.Persistent);

			for (int i = 0; i < m_Particles.Length; i++)
			{
				m_Particles[i] = new Particle(target + m_SpanDistance * (i + 1) * normal, mass: 1.0f);
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

			UnityEngine.Debug.Log($"> {i} {from} {to} {distance}");

			m_DistanceConstraints[i] = new DistanceConstraint(distance, from, to);
		}

		public void CreateRope(float3 source, float3 target)
		{
			//var vector = source - target;
			//var normal = math.normalize(vector);
			//var magnitude = math.length(vector);
			//var particleCount = 1 + (int)math.ceil(magnitude / m_SpanDistance);

			//m_Particles.SetCapacity(particleCount);
			//m_DistanceConstraints.SetCapacity(particleCount - 1);

			//for (int i = 0; i < particleCount - 1; i++)
			//{
			//	m_Particles.AddNoResize(new Particle(target + m_SpanDistance * i * normal, mass: 1.0f));
			//}

			//m_Particles.AddNoResize(new Particle(source, mass: 1.0f));

			//for (int i = 0; i < particleCount - 1; i++)
			//{
			//	var distance = math.distance(m_Particles[i].Position, m_Particles[i + 1].Position);

			//	m_DistanceConstraints.AddNoResize(new DistanceConstraint(distance));
			//}
		}

		public void Notify(int startIndexOfParticles, int startIndexOfConstraint)
		{
			m_StartIndexOfParticles = startIndexOfParticles;
		}

		public void UpdateWith(Solver solver)
		{
			UnityEngine.Debug.Log($"> {m_StartIndexOfParticles}");

			for (int i = 0; i < m_ParitclesCopy.Length; i++)
			{
				m_ParitclesCopy[i] = solver.Particles[m_StartIndexOfParticles + i];
			}
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
