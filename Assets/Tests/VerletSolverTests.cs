using CustomPhysics;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using static CustomPhysics.ParticleAttachment;

[TestFixture]
internal class VerletSolverTests
{
	private const float k_Gravity = 9.807f;

	private RopeSolver m_RopeSolver;
	private Rope m_Rope;
	private ParticleAttachment m_TopParticleAttachment;
	private ParticleAttachment m_BottomParticleAttachment;
	private RopeCursor m_RopeCursor;

	private Vector3 TopParticlePosition => Vector3.up;

	private Vector3 BottomParticlePosition => Vector3.zero;

	[SetUp]
	public void SetUp()
	{
		m_RopeSolver = new GameObject().AddComponent<RopeSolver>();
		m_RopeSolver.GravityVector = new float3(0.0f, -k_Gravity, 0.0f);
		m_RopeSolver.OnStart();

		m_Rope = CreateRopeWithTwoParticle();
	}

	[TearDown]
	public void TearDown()
	{
		m_RopeSolver.Destoy();
		m_Rope.Destroy();
	}

	[Test]
	public void Test_Rope_RegisterOneRopeContainer()
	{
		var solver = new GameObject().AddComponent<RopeSolver>();
		var rope = new GameObject().AddComponent<Rope>();

		rope.transform.parent = solver.transform;
		solver.OnStart();
		rope.OnStart();

		Assert.That(solver.Containers.Count, Is.EqualTo(1));
		Assert.That(solver.Containers[0], Is.EqualTo(rope));

		solver.Destoy();
	}

	[Test]
	[TestCase(2)]
	[TestCase(3)]
	[TestCase(4)]
	public void Test_Rope_RegisterSeveralRopeContainers(int containersCount)
	{
		var solver = new GameObject().AddComponent<RopeSolver>();
		solver.OnStart();

		for (int i = 0; i < containersCount; i++)
		{
			var rope = new GameObject().AddComponent<Rope>();
			rope.transform.parent = solver.transform;
			rope.OnStart();

			Assert.That(solver.Containers.Count, Is.EqualTo(i + 1));
			Assert.That(solver.Containers[i], Is.EqualTo(rope));
		}

		solver.Destoy();
	}

	/// <summary>
	/// y = g * dt^2 / 2
	/// </summary>
	[Test]
	[TestCase(1.0f, 1, -0.5f * k_Gravity)]
	[TestCase(1.0f, 2, -0.5f * k_Gravity)]
	[TestCase(1.0f, 3, -0.5f * k_Gravity)]
	[TestCase(1.0f, 4, -0.5f * k_Gravity)]
	[TestCase(1.0f, 5, -0.5f * k_Gravity)]
	//[TestCase(0.1f, 1000, -0.5f*k_Gravity*0.1f*0.1f)]
	public void Test_MovementUnderGravity(
		float deltaTimeIsSeconds,
		int substepCount,
		float expectedPositionAlongYAxis)
	{
		m_Rope.OnStart();

		deltaTimeIsSeconds /= substepCount;

		TestTools.AssertPosition(m_Rope.Particles[0].Position, BottomParticlePosition);
		TestTools.AssertPosition(m_Rope.Particles[1].Position, TopParticlePosition);

		for (int i = 0; i < substepCount; i++)
		{
			m_RopeSolver.Substep(deltaTimeIsSeconds);
		}

		m_RopeSolver.Containers[0].Job.Complete();

		TestTools.AssertPosition(
			m_Rope.Particles[0].Position,
			new float3(0.0f, expectedPositionAlongYAxis, 0.0f));

		TestTools.AssertPosition(
			m_Rope.Particles[1].Position,
			new float3(0.0f, 1.0f + expectedPositionAlongYAxis, 0.0f));
	}

	[Test]
	public void Test_FallingAsleepIfThereIsNoMovement()
	{
		m_Rope.OnStart();
		Step();
		m_RopeSolver.Containers[0].Job.Complete();

		Assert.That(m_Rope.Particles[0].IsSleep, Is.EqualTo(false));
		Assert.That(m_Rope.Particles[1].IsSleep, Is.EqualTo(false));

		var particles = m_Rope.Particles;

		var particle = particles[0];
		particle.OldPosition = float3.zero;
		particle.Position = float3.zero;
		particles[0] = particle;

		particle = particles[1];
		particle.OldPosition = float3.zero;
		particle.Position = float3.zero;
		particles[1] = particle;

		m_RopeSolver.EndStep();
		m_RopeSolver.Containers[0].Job.Complete();

		Assert.That(m_Rope.Particles[0].IsSleep, Is.EqualTo(true));
		Assert.That(m_Rope.Particles[1].IsSleep, Is.EqualTo(true));
	}

	[Test]
	public void Test_AttachementContstraint_WithoutMovement()
	{
		m_Rope.OnStart();

		Assert.That(m_RopeSolver.Containers.Count, Is.EqualTo(1));
		Assert.That(m_Rope.AttachmentConstraints.Length, Is.EqualTo(1));
		Assert.That(m_Rope.AttachmentConstraints[0].Index, Is.EqualTo(1));

		TestTools.AssertPosition(m_Rope.Particles[0].Position, BottomParticlePosition);
		TestTools.AssertPosition(m_Rope.Particles[1].Position, TopParticlePosition);

		Step(substepTimeInSeconds: 1.0f);

		m_RopeSolver.Containers[0].Job.Complete();

		TestTools.AssertPosition(m_Rope.Particles[1].Position, TopParticlePosition);
	}

	[Test]
	public void Test_DistanceConstraint_WhenHasStaticAttachment_CorrespondParticleIsFixed()
	{
		m_TopParticleAttachment.Test_AttachmentType = AttachmentType.Static;
		m_BottomParticleAttachment.Test_AttachmentType = AttachmentType.Dynamic;

		m_Rope.OnStart();

		Assert.That(m_Rope.DistanceConstraints.Length, Is.EqualTo(1));
		Assert.That(m_Rope.DistanceConstraints[0].IsIndex0Free, Is.False);
		Assert.That(m_Rope.DistanceConstraints[0].IsIndex1Free, Is.True);
	}

	[Test]
	public void Test_DistanceConstraint()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var expectedDistanceConstraintCount = 5;

		m_Rope.Test_StartRopeFrom = AttachmentParticle.Top;
		m_RopeSolver.DistanceConstraintsIterations = 100;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_TopParticleAttachment.Test_AttachmentType = AttachmentType.Static;

		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_BottomParticleAttachment.Test_AttachmentType = AttachmentType.Dynamic;

		m_Rope.OnStart();

		Assert.That(m_Rope.DistanceConstraints.Length, Is.EqualTo(expectedDistanceConstraintCount));

		TestTools.AssertPosition(m_Rope.Particles[0].Position, topPosition);
		TestTools.AssertPosition(m_Rope.Particles[^1].Position, bottomPosition);

		for (int i = 0; i < 10; i++)
		{
			Step(0.1f);
		}

		m_RopeSolver.Containers[0].Job.Complete();

		for (int i = 0; i < expectedDistanceConstraintCount; i++)
		{
			TestTools.AssertPosition(m_Rope.Particles[i].Position, (float3)(topPosition + Vector3.down * i));
		}
	}

	private Rope CreateRopeWithTwoParticle()
	{
		var rope = new GameObject().AddComponent<Rope>();

		m_TopParticleAttachment = TestTools.AttachTopParticleAttachment(rope, AttachmentType.Static);
		m_TopParticleAttachment.Test_Target.transform.position = TopParticlePosition;

		m_BottomParticleAttachment = TestTools.AttachBottomParticleAttachment(rope, AttachmentType.Dynamic);
		m_BottomParticleAttachment.Test_Target.transform.position = BottomParticlePosition;

		m_RopeCursor = TestTools.AttachCursor(rope);
		m_RopeCursor.Test_ParticleAttachment = m_TopParticleAttachment;

		rope.transform.parent = m_RopeSolver.transform;
		rope.Test_ParticleDistance = 1.0f;

		return rope;
	}

	private void Step(float substepTimeInSeconds = 1.0f)
	{
		m_RopeSolver.BeginStep();
		m_RopeSolver.Substep(substepTimeInSeconds);
		m_RopeSolver.EndStep();
	}
}
