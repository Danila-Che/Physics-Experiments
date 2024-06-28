namespace CustomPhysics
{
    public struct DistanceConstraint
    {
        public readonly int Index0;
        public readonly bool IsIndex0Free;
		public readonly int Index1;
        public readonly bool IsIndex1Free;
        public float Distance;

        public DistanceConstraint(int index0, bool isIndex0Free, int index1, bool isIndex1Free, float distance)
        {
            Index0 = index0;
            IsIndex0Free = isIndex0Free;
			Index1 = index1;
            IsIndex1Free = isIndex1Free;
            Distance = distance;
        }
    }
}
