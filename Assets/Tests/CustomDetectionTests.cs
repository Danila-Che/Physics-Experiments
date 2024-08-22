using CustomCollisionDetection;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Contact = CustomCollisionDetection.CollisionSystem.RawContact;
using Line = CustomCollisionDetection.CollisionSystem.RawLine;

[TestFixture]
public class CustomDetectionTests : MonoBehaviour
{
	[Test]
	public void Test_CheckInsideShape()
	{
		var normal = math.up();
		var shape = new List<Contact>()
		{
			new(new float3(-1.0f, 0.0f, -1.0f)),
			new(new float3(-1.0f, 0.0f, 1.0f)),
			new(new float3(1.0f, 0.0f, -1.0f)),
			new(new float3(1.0f, 0.0f, 1.0f)),
		};

		CollisionSystem.CalculateAngles(shape, normal);
		shape.Sort(CollisionSystem.CompareAngle);

		var point = float3.zero;

		Assert.IsTrue(CollisionSystem.IsInside(point, shape, normal));
	}

	[Test]
	public void Test_CheckOutsideShape()
	{
		var shape = new List<Contact>()
		{
			new(new float3(-1.0f, 0.0f, -1.0f)),
			new(new float3(-1.0f, 0.0f, 1.0f)),
			new(new float3(1.0f, 0.0f, -1.0f)),
			new(new float3(1.0f, 0.0f, 1.0f)),
		};

		shape.Sort(CollisionSystem.CompareAngle);

		var point = 2.0f * math.right();
		var normal = math.up();

		Assert.IsFalse(CollisionSystem.IsInside(point, shape, normal));
	}

	[Test]
	public void Test_Parallel_Case0()
	{
		var line0 = new Line(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
		var line1 = new Line(new float3(1.0f, 0.0f, -1.0f), new float3(1.0f, 0.0f, 1.0f));

		Assert.IsFalse(CollisionSystem.TryGetLineIntersection(line0, line1, out _));
	}

	[Test]
	public void Test_Parallel_Case1()
	{
		var line0 = new Line(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
		var line1 = new Line(new float3(1.0f, 0.0f, 1.0f), new float3(1.0f, 0.0f, -1.0f));

		Assert.IsFalse(CollisionSystem.TryGetLineIntersection(line0, line1, out _));
	}

	[Test]
	public void Test_Intersect_Case0()
	{
		var line0 = new Line(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
		var line1 = new Line(new float3(-1.0f, 0.0f, 1.0f), new float3(1.0f, 0.0f, 1.0f));

		Assert.IsTrue(CollisionSystem.TryGetLineIntersection(line0, line1, out var projectPoint));
		Assert.That(projectPoint, Is.EqualTo(new float3(-1.0f, 0.0f, 1.0f)));
	}
}
