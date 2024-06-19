using CustomPhysics;
using UnityEngine;

[RequireComponent(typeof(Rope))]
public class RopeCursor : MonoBehaviour, IRopeDirection
{
    [SerializeField] private AttachmentParticle m_AttachmentParticle;

    private Rope m_Rope;

    public AttachmentParticle StartRopeFrom => m_AttachmentParticle switch
    {
        AttachmentParticle.Top => AttachmentParticle.Bottom,
        AttachmentParticle.Bottom => AttachmentParticle.Top,
        _ => throw new System.NotImplementedException(),
    };

#if UNITY_INCLUDE_TESTS && UNITY_EDITOR

    public AttachmentParticle Test_AttachmentParticle { get => m_AttachmentParticle; set => m_AttachmentParticle = value; }
    public Rope Test_Rope { get => m_Rope; set => m_Rope = value; }


    public void OnStart() => Start();

#endif

    private void Start()
    {
        m_Rope = GetComponent<Rope>();
    }
}
