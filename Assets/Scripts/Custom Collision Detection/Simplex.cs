using Unity.Mathematics;

namespace CustomCollisionDetection
{
	internal struct Simplex
	{
		private readonly float3[] m_Points;
		private int m_Size;

		private Simplex(int maxSize)
		{
			m_Points = new float3[maxSize];
			m_Size = 0;
		}

		public static Simplex Default => new(4);

		public readonly float3 this[int index] => m_Points[index];

		public readonly int Size => m_Size;

		public void Set(params float3[] source)
		{
			m_Size = source.Length;

			for (int i = 0; i < m_Size; i++)
			{
				m_Points[i] = source[i];
			}
		}

		public void PushFront(float3 point)
		{
			m_Points[3] = m_Points[2];
			m_Points[2] = m_Points[1];
			m_Points[1] = m_Points[0];
			m_Points[0] = point;

			m_Size = math.min(m_Size + 1, 4);
		}
	}
}
