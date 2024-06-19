using Unity.Mathematics;

namespace CustomPhysics
{
    public struct AttachmentConstraint
    {
        public readonly int Index;
        public float3 Position;

        public AttachmentConstraint(int index)
        {
            Index = index;
            Position = float3.zero;
        }
    }
}
