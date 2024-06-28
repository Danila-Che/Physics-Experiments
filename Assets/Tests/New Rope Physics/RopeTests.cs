using NUnit.Framework;
using RopePhysics;
using Unity.Mathematics;

[TestFixture]
public class RopeTests
{
	private Rope m_Rope;

	[TearDown]
	public void TearDown()
	{
		m_Rope?.Dispose();
	}

	[Test]
	public void Test_GivenRopeWithSpanDistance_WhenAssignedTargetAndSourcePoint_ThenCreateParticlesOnEdgePoint()
	{
		var target = float3.zero;
		var source = math.up();

		var targetAttachment = new AttachmentConstraintAlias();
		var sourceAttachment = new AttachmentConstraintAlias();

		targetAttachment.SetPosition(target);
		sourceAttachment.SetPosition(source);

		m_Rope = new Rope(spanDistance: 1.0f, targetAttachment, sourceAttachment);
		m_Rope.CreateRope();

		Assert.That(m_Rope.Particles.Length, Is.EqualTo(2));
		Assert.That(m_Rope.Particles[0].Position, Is.EqualTo(target));
		Assert.That(m_Rope.Particles[1].Position, Is.EqualTo(source));
	}

	[Test]
	public void Test_GivenRopeWithSpanDistance_WhenAssignedTargetAndSourcePoint_ThenCreateChainOfParticles()
	{
		var target = float3.zero;
		var source = math.up();

		var targetAttachment = new AttachmentConstraintAlias();
		var sourceAttachment = new AttachmentConstraintAlias();

		targetAttachment.SetPosition(target);
		sourceAttachment.SetPosition(source);

		m_Rope = new Rope(spanDistance: 0.5f, targetAttachment, sourceAttachment);
		m_Rope.CreateRope();

		Assert.That(m_Rope.Particles.Length, Is.EqualTo(3));
		Assert.That(m_Rope.Particles[0].Position, Is.EqualTo(target));
		Assert.That(m_Rope.Particles[1].Position, Is.EqualTo(new float3(0.0f, 0.5f, 0.0f)));
		Assert.That(m_Rope.Particles[2].Position, Is.EqualTo(source));
	}
}
