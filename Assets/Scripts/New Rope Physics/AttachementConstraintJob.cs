using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace RopePhysics
{
	/// <summary>
	/// Apply attachment contstraint on particles.
	/// The static constraint sets the mass of the particle equal to infinity.
	/// From target to source
	/// </summary>
	[BurstCompile]
	public struct AttachementConstraintJob : IJob
	{
		public NativeList<Particle> Particles;
		[ReadOnly]
		public AttachmentConstraint TargetParticleAttachmentConstraint;
		[ReadOnly]
		public AttachmentConstraint SourceParticleAttachmentConstraint;

		public void Execute()
		{
			ResolveAttachment(0, TargetParticleAttachmentConstraint);
			ResolveAttachment(Particles.Length - 1, SourceParticleAttachmentConstraint);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResolveAttachment(int i, AttachmentConstraint attachmentConstraint)
		{
			var particle = Particles[i];

			if (attachmentConstraint.Attachment is Attachment.Static)
			{
				particle.Position = attachmentConstraint.Position;
				particle.SetInfinityMass();
			}

			Particles[i] = particle;
		}
	}
}
