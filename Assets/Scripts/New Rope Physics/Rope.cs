using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	public class Rope : IDisposable
	{
		private readonly float m_SpanDistance;
		private readonly NativeList<Particle> m_Particles;

		private NativeList<AttachmentConstraint> m_StaticAttachementConstraints;
		private NativeList<DistanceConstraint> m_DistanceConstraints;

		private readonly AttachmentConstraintAlias m_TargetAttachmentConstraint;
		private AttachmentConstraint m_TargetParticleAttachmentConstraint;

		private readonly AttachmentConstraintAlias m_SourceAttachmentConstraint;
		private AttachmentConstraint m_SourceParticleAttachmentConstraint;

		private JobHandle m_Job;

		public Rope() : this(1.0f) { }

		public Rope(float spanDistance, AttachmentConstraintAlias targetAttachmentConstraint, AttachmentConstraintAlias sourceAttachmentConstraint) : this(spanDistance)
		{
			m_TargetAttachmentConstraint = targetAttachmentConstraint;

			m_TargetParticleAttachmentConstraint = (m_TargetAttachmentConstraint == null) switch
			{
				true => new AttachmentConstraint(isExists: false),
				false => new AttachmentConstraint(m_TargetAttachmentConstraint.Attachment, isExists: true),
			};

			m_SourceAttachmentConstraint = sourceAttachmentConstraint;

			m_SourceParticleAttachmentConstraint = (m_SourceAttachmentConstraint == null) switch
			{
				true => new AttachmentConstraint(isExists: false),
				false => new AttachmentConstraint(m_SourceAttachmentConstraint.Attachment, isExists: true),
			};
		}

		public Rope(float spanDistance)
		{
			m_SpanDistance = spanDistance;
			m_Particles = new NativeList<Particle>(Allocator.Persistent);
			m_StaticAttachementConstraints = new NativeList<AttachmentConstraint>(Allocator.Persistent);
			m_DistanceConstraints = new NativeList<DistanceConstraint>(Allocator.Persistent);
		}

		public NativeList<Particle> Particles => m_Particles;

		public NativeList<DistanceConstraint> DistanceConstraints => m_DistanceConstraints;

		public AttachmentConstraint TargetParticleAttachmentConstraint => m_TargetParticleAttachmentConstraint;

		public AttachmentConstraint SourceParticleAttachmentConstraint => m_SourceParticleAttachmentConstraint;

		public bool HasAttachment => m_TargetParticleAttachmentConstraint.IsExists && m_SourceParticleAttachmentConstraint.IsExists;
		public bool AllAttachmentIsStatic =>
			m_TargetParticleAttachmentConstraint.Attachment is Attachment.Static
			&& m_SourceParticleAttachmentConstraint.Attachment is Attachment.Static;

		public JobHandle Job { get => m_Job; set => m_Job = value; }

		public void Dispose()
		{
			if (m_Particles.IsCreated)
			{
				m_Particles.Dispose();
			}

			if (m_StaticAttachementConstraints.IsCreated)
			{
				m_StaticAttachementConstraints.Dispose();
			}

			if (m_DistanceConstraints.IsCreated)
			{
				m_DistanceConstraints.Dispose();
			}
		}

		public void Update()
		{
			m_TargetParticleAttachmentConstraint.SetPosition(m_TargetAttachmentConstraint.Position);
			m_SourceParticleAttachmentConstraint.SetPosition(m_SourceAttachmentConstraint.Position);
		}

		public void CreateRope()
		{
			CreateRope(m_SourceAttachmentConstraint.Position, m_TargetAttachmentConstraint.Position);
		}

		public void CreateRope(float3 source, float3 target)
		{
			var vector = source - target;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = 1 + (int)math.ceil(magnitude / m_SpanDistance);

			m_Particles.SetCapacity(particleCount);
			m_DistanceConstraints.SetCapacity(particleCount - 1);

			for (int i = 0; i < particleCount - 1; i++)
			{
				m_Particles.AddNoResize(new Particle(target + m_SpanDistance * i * normal));
			}

			m_Particles.AddNoResize(new Particle(source));

			for (int i = 0; i < particleCount - 1; i++)
			{
				var distance = math.distance(m_Particles[i].Position, m_Particles[i + 1].Position);

				m_DistanceConstraints.AddNoResize(new DistanceConstraint(distance));
			}
		}
	}
}
