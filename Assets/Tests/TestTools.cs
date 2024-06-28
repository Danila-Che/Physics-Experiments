using CustomPhysics;
using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static CustomPhysics.ParticleAttachment;

internal static class TestTools
{
	public static TopParticleAttachment AttachTopParticleAttachment(Rope rope, AttachmentType attachmentType)
	{
		var gameObject = new GameObject().AddComponent<Rigidbody>();
		var particleAttachment = rope.gameObject.AddComponent<TopParticleAttachment>();

		particleAttachment.Test_Target = gameObject;
		particleAttachment.Test_AttachmentType = attachmentType;

		return particleAttachment;
	}

	public static BottomParticleAttachment AttachBottomParticleAttachment(Rope rope, AttachmentType attachmentType)
	{
		var gameObject = new GameObject().AddComponent<Rigidbody>();
		var particleAttachment = rope.gameObject.AddComponent<BottomParticleAttachment>();

		particleAttachment.Test_Target = gameObject;
		particleAttachment.Test_AttachmentType = attachmentType;

		return particleAttachment;
	}

	public static RopeCursor AttachCursor(Rope rope)
	{
		var ropeCursor = rope.gameObject.AddComponent<RopeCursor>();

		return ropeCursor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AssertPosition(float3 actual, float3 expected)
	{
		Assert.That(
			math.length(actual - expected),
			Is.LessThan(1E-5f), $"Expected {expected}{Environment.NewLine}But was: {actual}");
	}
}
