using UnityEngine;

namespace CustomPhysics
{
    public class ParticleAttachment : MonoBehaviour
    {
        public enum AttachmentType
        {
            Static,
            Dynamic
        }

        [SerializeField] private Rigidbody m_Target;
        [SerializeField] private AttachmentParticle m_AttachmentParticle;
        [SerializeField] private AttachmentType m_AttachmentType;

        public AttachmentParticle AttachmentParticle => m_AttachmentParticle;

        public AttachmentType Type => m_AttachmentType;

        public Rigidbody Taget => m_Target;
        

#if UNITY_INCLUDE_TESTS && UNITY_EDITOR

        public Rigidbody Test_Target { get => m_Target; set => m_Target = value; }
        public AttachmentParticle Test_AttachmentParticle { get => m_AttachmentParticle; set => m_AttachmentParticle = value; }
        public AttachmentType Test_AttachmentType { get => m_AttachmentType; set => m_AttachmentType = value; }

#endif
    }
}
