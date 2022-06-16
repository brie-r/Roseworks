using System;
using System.Collections.Generic;
using System.Text;

namespace Roseworks
{
	public struct VecF2
	{
		public float X, Y;
		public float this[int i]
		{
			get
			{
				if (i == 0) return X;
				else if (i == 1) return Y;
				else throw new IndexOutOfRangeException("VecF2 indexer range: 0-1");
			}
			set
			{
				if (i == 0) X = value;
				else if (i == 1) Y = value;
				else throw new IndexOutOfRangeException("VecF2 indexer range: 0-1");
			}
		}
	}
	public struct VecF3
	{
		public float X, Y, Z;
		public float this[int i]
		{
			get
			{
				if (i == 0) return X;
				else if (i == 1) return Y;
				else if (i == 2) return Z;
				else throw new IndexOutOfRangeException("VecF3 indexer range: 0-2");
			}
			set
			{
				if (i == 0) X = value;
				else if (i == 1) Y = value;
				else if (i == 2) Z = value;
				else throw new IndexOutOfRangeException("VecF3 indexer range: 0-2");
			}
		}
	}
	public struct VecI2
	{
		public int X, Y;
		public int this[int i]
		{
			get
			{
				if (i == 0) return X;
				else if (i == 1) return Y;
				else throw new IndexOutOfRangeException("VecI2 indexer range: 0-1");
			}
			set
			{
				if (i == 0) X = value;
				else if (i == 1) Y = value;
				else throw new IndexOutOfRangeException("VecI2 indexer range: 0-1");
			}
		}
	}
	public struct VecI3
	{
		public int X, Y, Z;
		public int this[int i]
		{
			get
			{
				if (i == 0) return X;
				else if (i == 1) return Y;
				else if (i == 2) return Z;
				else throw new IndexOutOfRangeException("VecI3 indexer range: 0-2");
			}
			set
			{
				if (i == 0) X = value;
				else if (i == 1) Y = value;
				else if (i == 2) Z = value;
				else throw new IndexOutOfRangeException("VecI3 indexer range: 0-2");
			}
		}
	}
}
