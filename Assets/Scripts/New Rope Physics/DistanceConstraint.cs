namespace RopePhysics
{
	public struct DistanceConstraint
	{
		public float Distance;
		public int Index0;
		public int Index1;

		public DistanceConstraint(float distance)
		{
			Distance = distance;
			Index0 = 0;
			Index1 = 0;
		}

		public DistanceConstraint(float distance, int index0, int index1)
		{
			Distance = distance;
			Index0 = index0;
			Index1 = index1;
		}
	}
}
