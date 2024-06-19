using System;
using System.Runtime.CompilerServices;
using CustomPhysics;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

internal static class TestTools
{
    public static ParticleAttachment CreateParticleAttachment(
        Rope rope,
        AttachmentParticle attachmentParticle,
        ParticleAttachment.AttachmentType attachmentType)
    {
        var gameObject = new GameObject().AddComponent<Rigidbody>();
        var particleAttachment = rope.gameObject.AddComponent<ParticleAttachment>(); 
        particleAttachment.Test_Target = gameObject;
        particleAttachment.Test_AttachmentParticle = attachmentParticle;
        particleAttachment.Test_AttachmentType = attachmentType;

        return particleAttachment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertPosition(float3 actual, float3 expected)
    {
        Assert.That(
            math.length(actual - expected),
            Is.LessThan(1E-5f), $"Expected {expected}{Environment.NewLine}But was: {actual}");
    }
}
