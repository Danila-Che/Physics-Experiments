using NUnit.Framework;
using RopePhysics;
using Unity.Mathematics;

[TestFixture]
public class SolverTests
{
	private const float k_Gravity = 9.807f;

	private Rope m_Rope;

	[TearDown]
	public void TearDown()
	{
		m_Rope?.Dispose();
	}

	[Test]
	public void Test_GivenSolver_WhenRegisterRope_ThenSolverHasOneRope()
	{
		var solver = new Solver();
		m_Rope = new Rope();

		solver.Register(m_Rope);

		Assert.That(solver.Ropes.Count, Is.EqualTo(1));
		Assert.That(solver.Ropes[0], Is.EqualTo(m_Rope));
	}

	[Test]
	public void Test_GivenSolverWithGravityAndRegisteredRope_WhenSolverStep_ThenParticlesFallByGravitationWithFirstStepOfVerlet()
	{
		var solver = new Solver(
			new float3(0.0f, -k_Gravity, 0.0f),
			distanceConstraintsIterations: 0,
			needDistanceConstraint: false);

		var target = float3.zero;
		var source = math.up();
		var deltaTime = 1.0f;

		var targetAttachment = new AttachmentConstraintAlias(Attachment.Dynamic);
		var sourceAttachment = new AttachmentConstraintAlias(Attachment.Dynamic);

		targetAttachment.SetPosition(target);
		sourceAttachment.SetPosition(source);

		m_Rope = new Rope(1.0f, targetAttachment, sourceAttachment);
		m_Rope.CreateRope();

		solver.Register(m_Rope);
		solver.BeginStep();
		solver.Step(deltaTime);
		solver.EndStep();

		var offset = new float3(0.0f, -0.5f * deltaTime * deltaTime * k_Gravity, 0.0f);

		m_Rope.Job.Complete();

		Assert.That(m_Rope.Particles[0].Position, Is.EqualTo(target + offset));
		Assert.That(m_Rope.Particles[1].Position, Is.EqualTo(source + offset));
	}

	[Test]
	public void Test_GivenSolverWithGravityAndRegisteredRope_WhenSolverStep_ThenParticlesFallByGravitationWithSecondStepOfVerlet()
	{
		var solver = new Solver(
			new float3(0.0f, -k_Gravity, 0.0f),
			distanceConstraintsIterations: 0,
			needDistanceConstraint: false);

		var target = float3.zero;
		var source = math.up();

		m_Rope = new Rope(1.0f);
		m_Rope.CreateRope(source, target);

		solver.Register(m_Rope);

		var deltaTime = 1.0f;

		solver.Step(0.5f * deltaTime);
		solver.Step(0.5f * deltaTime);

		var offset = new float3(0.0f, -0.5f * deltaTime * deltaTime * k_Gravity, 0.0f);

		m_Rope.Job.Complete();

		Assert.That(m_Rope.Particles[0].Position, Is.EqualTo(target + offset));
		Assert.That(m_Rope.Particles[1].Position, Is.EqualTo(source + offset));
	}

	[Test]
	public void Test_GivenSolverWithRegisteredRope_WhenSolverEndStepAndParticlesNeverMove_ThenParticlesSleep()
	{
		var solver = new Solver(
			new float3(0.0f, -k_Gravity, 0.0f),
			distanceConstraintsIterations: 0,
			needDistanceConstraint: false);

		var target = float3.zero;
		var source = math.up();

		m_Rope = new Rope(1.0f);
		m_Rope.CreateRope(source, target);

		solver.Register(m_Rope);

		solver.Step(1.0f);
		solver.EndStep();

		m_Rope.Job.Complete();

		Assert.That(m_Rope.Particles[0].IsSleep, Is.False);
		Assert.That(m_Rope.Particles[1].IsSleep, Is.False);

		var particles = m_Rope.Particles;

		var particle = particles[0];
		particle.OldPosition = particle.Position;
		particles[0] = particle;

		particle = particles[1];
		particle.OldPosition = particle.Position;
		particles[1] = particle;

		solver.EndStep();

		m_Rope.Job.Complete();

		Assert.That(m_Rope.Particles[0].IsSleep, Is.True);
		Assert.That(m_Rope.Particles[1].IsSleep, Is.True);
	}

	[Test]
	public void Test_GivenSolverWithRegisteredRopeThatHasStaticAttachment_WhenSolverStepWithDistanceConstraint_ThenChainOfParticlesStayOnTheirPosition()
	{
		var solver = new Solver(
			new float3(0.0f, -k_Gravity, 0.0f),
			distanceConstraintsIterations: 1,
			needDistanceConstraint: true);
		var targetAttachmentConstraint = new AttachmentConstraintAlias(Attachment.Static);
		var sourceAttachmentConstraint = new AttachmentConstraintAlias(Attachment.Static);

		var target = float3.zero;
		var source = math.up();

		m_Rope = new Rope(
			spanDistance: 0.25f,
			targetAttachmentConstraint: targetAttachmentConstraint,
			sourceAttachmentConstraint: sourceAttachmentConstraint);

		m_Rope.CreateRope(source, target);

		solver.Register(m_Rope);
		targetAttachmentConstraint.SetPosition(target);
		sourceAttachmentConstraint.SetPosition(source);

		solver.BeginStep();
		solver.Step(1.0f);
		solver.EndStep();

		m_Rope.Job.Complete();

		Assert.That(m_Rope.Particles[0].Position, Is.EqualTo(new float3(0.0f, 0.0f, 0.0f)));
		Assert.That(m_Rope.Particles[1].Position, Is.EqualTo(new float3(0.0f, 0.25f, 0.0f)));
		Assert.That(m_Rope.Particles[2].Position, Is.EqualTo(new float3(0.0f, 0.5f, 0.0f)));
		Assert.That(m_Rope.Particles[3].Position, Is.EqualTo(new float3(0.0f, 0.75f, 0.0f)));
		Assert.That(m_Rope.Particles[4].Position, Is.EqualTo(new float3(0.0f, 1.0f, 0.0f)));
	}
}
