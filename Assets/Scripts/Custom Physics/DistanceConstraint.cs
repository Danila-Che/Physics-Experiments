namespace CustomPhysics
{
    public struct DistanceConstraint
    {
        public readonly int Index0;
        public readonly int Index1;
        public float Distance;

        public DistanceConstraint(int index0, int index1, float distance)
        {
            Index0 = index0;
            Index1 = index1;
            Distance = distance;
        }
    }
}
