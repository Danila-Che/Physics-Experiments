using CustomPhysics;
using UnityEngine;

public class RopeCursor : MonoBehaviour
{
    [SerializeField] private ParticleAttachment m_ParticleAttachment;

    public ParticleAttachment ParticleAttachment => m_ParticleAttachment;

#if UNITY_INCLUDE_TESTS && UNITY_EDITOR

	public ParticleAttachment Test_ParticleAttachment { get => m_ParticleAttachment; set => m_ParticleAttachment = value; }

#endif
}
