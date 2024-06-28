using NUnit.Framework;
using RopePhysics;
using Unity.Mathematics;

[TestFixture]
public class ConstraintTests
{
	private const float k_Gravity = 9.807f;

	private Rope m_Rope;

	[TearDown]
	public void TearDown()
	{
		m_Rope?.Dispose();
	}

	[Test]
	public void Test_GivenRopeWithTargetAndSourceAttachmentConstraint_WhenRopeWasCreated_ThenAttachmentConstraintsExist()
	{
		var targetAttachment = new AttachmentConstraintAlias();
		var sourceAttachment = new AttachmentConstraintAlias();

		targetAttachment.SetPosition(float3.zero);
		sourceAttachment.SetPosition(math.up());

		m_Rope = new Rope(1.0f, targetAttachment, sourceAttachment);

		Assert.That(m_Rope.TargetParticleAttachmentConstraint.IsExists, Is.True);
		Assert.That(m_Rope.SourceParticleAttachmentConstraint.IsExists, Is.True);
	}

	[Test]
	public void Test_GivenSolverAndRopeWithTwoStaticAttachmentConstraint_WhenSolverStep_ThenAttachedParticleFixedByTheirPosition()
	{
		var target = float3.zero;
		var source = math.up();

		var targetAttachment = new AttachmentConstraintAlias();
		var sourceAttachment = new AttachmentConstraintAlias();

		targetAttachment.SetPosition(target);
		sourceAttachment.SetPosition(source);

		var solver = new Solver(new float3(0.0f, -k_Gravity, 0.0f));
		m_Rope = new Rope(1.0f, targetAttachment, sourceAttachment);
		m_Rope.CreateRope();

		solver.Register(m_Rope);
		solver.BeginStep();
		solver.Step(1.0f);
		solver.EndStep();

		m_Rope.Job.Complete();

		Assert.That(m_Rope.Particles.Length, Is.EqualTo(2));
		Assert.That(m_Rope.Particles[0].Position, Is.EqualTo(target));
		Assert.That(m_Rope.Particles[1].Position, Is.EqualTo(source));
	}
}
