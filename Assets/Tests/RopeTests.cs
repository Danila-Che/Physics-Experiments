using CustomPhysics;
using NUnit.Framework;
using UnityEngine;
using static CustomPhysics.ParticleAttachment;

[TestFixture]
internal class RopeTests
{
	private GameObject m_RopeGameObject;
	private Rope m_Rope;

	private ParticleAttachment m_TopParticleAttachment;
	private ParticleAttachment m_BottomParticleAttachment;
	private RopeCursor m_RopeCursor;

	[SetUp]
	public void SetUp()
	{
		m_RopeGameObject = new GameObject();
		m_Rope = m_RopeGameObject.AddComponent<Rope>();

		m_TopParticleAttachment = TestTools.AttachTopParticleAttachment(m_Rope, AttachmentType.Dynamic);
		m_BottomParticleAttachment = TestTools.AttachBottomParticleAttachment(m_Rope, AttachmentType.Dynamic);
		m_RopeCursor = TestTools.AttachCursor(m_Rope);
	}

	[TearDown]
	public void TearDown()
	{
		m_Rope.Destroy();
	}

	[Test]
	public void Test_ParticlesLifeCycle()
	{
		m_Rope.OnStart();
		Assert.That(m_Rope.Particles.IsCreated);

		m_Rope.Destroy();
		Assert.That(m_Rope.Particles.IsCreated is false);
	}

	[Test]
	public void Test_DistanceConstraintsLifeCycle()
	{
		m_Rope.OnStart();
		Assert.That(m_Rope.Test_DistanceConstraints.IsCreated);

		m_Rope.Destroy();
		Assert.That(m_Rope.Test_DistanceConstraints.IsCreated is false);
	}

	[Test]
	public void Test_AttachmentConstraintsLifeCycle()
	{
		m_Rope.OnStart();
		Assert.That(m_Rope.AttachmentConstraints.IsCreated);

		m_Rope.Destroy();
		Assert.That(m_Rope.AttachmentConstraints.IsCreated is false);
	}

	[Test]
	public void Test_CreateRope_UniformDistributionCase_WithTwoParticleAttachments()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var expectedParticleCount = 6;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = 1.0f;
		m_Rope.OnStart();

		Assert.That(m_Rope.Particles.Length, Is.EqualTo(expectedParticleCount));

		for (int i = 0; i < expectedParticleCount; i++)
		{
			var expectedPositionFromTop = topPosition + i * m_Rope.Test_ParticleDistance * Vector3.down;
			var expectedPositionFromBottom = bottomPosition + i * m_Rope.Test_ParticleDistance * Vector3.up;

			Assert.That((Vector3)m_Rope.Particles[i].Position,
				Is.EqualTo(expectedPositionFromTop).Within(Mathf.Epsilon)
				.Or.EqualTo(expectedPositionFromBottom).Within(Mathf.Epsilon));
		}
	}

	[Test]
	public void Test_CreateRope_UniformDistributionCase_OnlyTwoParticle()
	{
		m_TopParticleAttachment.Test_Target.transform.position = Vector3.up;
		m_BottomParticleAttachment.Test_Target.transform.position = Vector3.zero;
		m_Rope.Test_ParticleDistance = 1.0f;
		m_Rope.OnStart();

		Assert.That(m_Rope.Particles.Length, Is.EqualTo(2));

		Assert.That((Vector3)m_Rope.Particles[0].Position,
				Is.EqualTo(Vector3.up).Within(Mathf.Epsilon)
				.Or.EqualTo(Vector3.zero).Within(Mathf.Epsilon));

		Assert.That((Vector3)m_Rope.Particles[^1].Position,
				Is.EqualTo(Vector3.up).Within(Mathf.Epsilon)
				.Or.EqualTo(Vector3.zero).Within(Mathf.Epsilon));
	}

	[Test]
	public void Test_CreateRope_NonUniformDistributionCase_WithTwoParticleAttachments()
	{
		var topPosition = Vector3.up * 4.5f;
		var bottomPosition = Vector3.zero;
		var expectedParticleCount = 6;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = 1.0f;
		m_Rope.OnStart();

		Assert.That(m_Rope.Particles.Length, Is.EqualTo(expectedParticleCount));

		Assert.That((Vector3)m_Rope.Particles[0].Position,
			Is.EqualTo(topPosition).Within(Mathf.Epsilon)
			.Or.EqualTo(bottomPosition).Within(Mathf.Epsilon));

		Assert.That((Vector3)m_Rope.Particles[^1].Position,
			Is.EqualTo(topPosition).Within(Mathf.Epsilon)
			.Or.EqualTo(bottomPosition).Within(Mathf.Epsilon));

		for (int i = 1; i < expectedParticleCount - 1; i++)
		{
			var expectedPositionFromTop = topPosition + i * m_Rope.Test_ParticleDistance * Vector3.down;
			var expectedPositionFromBottom = bottomPosition + i * m_Rope.Test_ParticleDistance * Vector3.up;

			Assert.That((Vector3)m_Rope.Particles[i].Position,
				Is.EqualTo(expectedPositionFromTop).Within(Mathf.Epsilon)
				.Or.EqualTo(expectedPositionFromBottom).Within(Mathf.Epsilon));
		}
	}

	[Test]
	public void Test_CreateRope_UniformDistributionCase_DistanceConstraint_WithTwoParticleAttachments()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var distance = 1.0f;
		var expectedDistanceConstraintCount = 5;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = distance;
		m_Rope.OnStart();

		Assert.That(m_Rope.Test_DistanceConstraints.Length, Is.EqualTo(expectedDistanceConstraintCount));

		for (int i = 0; i < expectedDistanceConstraintCount; i++)
		{
			Assert.That(m_Rope.Test_DistanceConstraints[i].Distance, Is.EqualTo(distance));
			Assert.That(m_Rope.Test_DistanceConstraints[i].Index0,
				Is.LessThan(m_Rope.Test_DistanceConstraints[i].Index1)
				.Or.GreaterThan(m_Rope.Test_DistanceConstraints[i].Index1));
		}
	}

	[Test]
	public void Test_CreateRope_NonUniformDistributionCase_DistanceConstraint_WithTwoParticleAttachments()
	{
		var topPosition = Vector3.up * 4.5f;
		var bottomPosition = Vector3.zero;
		var distance = 1.0f;
		var expectedDistanceConstraintCount = 5;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = 1.0f;
		m_Rope.OnStart();

		Assert.That(m_Rope.Test_DistanceConstraints.Length, Is.EqualTo(expectedDistanceConstraintCount));

		if (m_Rope.Test_DistanceConstraints[0].Distance == 1.0f)
		{
			Assert.That(m_Rope.Test_DistanceConstraints[^1].Distance, Is.EqualTo(0.5f).Within(Mathf.Epsilon));
		}
		else
		{
			Assert.That(m_Rope.Test_DistanceConstraints[^1].Distance, Is.EqualTo(1.0f).Within(Mathf.Epsilon));
		}

		if (m_Rope.Test_DistanceConstraints[^1].Distance == 1.0f)
		{
			Assert.That(m_Rope.Test_DistanceConstraints[0].Distance, Is.EqualTo(0.5f).Within(Mathf.Epsilon));
		}
		else
		{
			Assert.That(m_Rope.Test_DistanceConstraints[0].Distance, Is.EqualTo(1.0f).Within(Mathf.Epsilon));
		}

		for (int i = 1; i < expectedDistanceConstraintCount - 1; i++)
		{
			Assert.That(m_Rope.Test_DistanceConstraints[i].Distance, Is.EqualTo(distance));
		}

		for (int i = 0; i < expectedDistanceConstraintCount; i++)
		{
			Assert.That(m_Rope.Test_DistanceConstraints[i].Index0,
				Is.LessThan(m_Rope.Test_DistanceConstraints[i].Index1)
				.Or.GreaterThan(m_Rope.Test_DistanceConstraints[i].Index1));
		}
	}

	[Test]
	public void Test_CreateRope_UniformDistributionCase_OnlyOneConstraint()
	{
		m_TopParticleAttachment.Test_Target.transform.position = Vector3.up;
		m_BottomParticleAttachment.Test_Target.transform.position = Vector3.zero;
		m_Rope.Test_ParticleDistance = 1.0f;
		m_Rope.OnStart();

		Assert.That(m_Rope.Test_DistanceConstraints.Length, Is.EqualTo(1));
		Assert.That(m_Rope.Test_DistanceConstraints[0].Distance, Is.EqualTo(1.0f).Within(Mathf.Epsilon));

		if (m_Rope.Test_DistanceConstraints[0].Index0 == 0)
		{
			Assert.That(m_Rope.Test_DistanceConstraints[0].Index0, Is.EqualTo(0));
			Assert.That(m_Rope.Test_DistanceConstraints[0].Index1, Is.EqualTo(1));
		}
		else
		{
			Assert.That(m_Rope.Test_DistanceConstraints[0].Index0, Is.EqualTo(1));
			Assert.That(m_Rope.Test_DistanceConstraints[0].Index1, Is.EqualTo(0));
		}
	}

	[Test]
	public void Test_RopeCursor_Init_StartFromBottom()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var distance = 1.0f;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = distance;
		m_RopeCursor.Test_ParticleAttachment = m_TopParticleAttachment;
		m_Rope.OnStart();

		TestTools.AssertPosition(m_Rope.Particles[0].Position, bottomPosition);
		TestTools.AssertPosition(m_Rope.Particles[^1].Position, topPosition);
	}

	[Test]
	public void Test_RopeCursor_Init_StartFromTop()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var distance = 1.0f;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = distance;
		m_RopeCursor.Test_ParticleAttachment = m_BottomParticleAttachment;
		m_Rope.OnStart();

		TestTools.AssertPosition(m_Rope.Particles[0].Position, topPosition);
		TestTools.AssertPosition(m_Rope.Particles[^1].Position, bottomPosition);
	}

	[Test]
	public void Test_CreateRope_When_RopeHasCursorWithTopParitcleAttachment_RopeMustStartFromBottom()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var distance = 1.0f;

		m_RopeCursor.Test_ParticleAttachment = m_TopParticleAttachment;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = distance;
		m_Rope.OnStart();

		TestTools.AssertPosition(m_Rope.Particles[0].Position, bottomPosition);
		TestTools.AssertPosition(m_Rope.Particles[^1].Position, topPosition);
	}

	[Test]
	public void Test_CreateRope_When_RopeHasCursorWithBottomParitcleAttachment_RopeMustStartFromTop()
	{
		var topPosition = Vector3.up * 5.0f;
		var bottomPosition = Vector3.zero;
		var distance = 1.0f;

		m_RopeCursor.Test_ParticleAttachment = m_BottomParticleAttachment;

		m_TopParticleAttachment.Test_Target.transform.position = topPosition;
		m_BottomParticleAttachment.Test_Target.transform.position = bottomPosition;
		m_Rope.Test_ParticleDistance = distance;
		m_Rope.OnStart();

		TestTools.AssertPosition(m_Rope.Particles[0].Position, topPosition);
		TestTools.AssertPosition(m_Rope.Particles[^1].Position, bottomPosition);
	}

	public void Test_RopeCursor_ChangeDistanceConstraint()
	{

	}

	public void Test_RopeCursor_AddParticle()
	{

	}

	public void Test_RopeCursor_RemoveParticle()
	{

	}
}
