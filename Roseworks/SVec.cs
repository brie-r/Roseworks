using System;
using System.Numerics;

namespace Roseworks
{
	public static class Extensions
	{
		public static ref float At(this ref Vector2 vec, int index)
		{
			if (index == 0) return ref vec.X;
			else if (index == 1) return ref vec.Y;
			else throw new IndexOutOfRangeException("Vector2 indexer range: 0-1");
		}
		public static ref float At(this ref Vector3 vec, int index)
		{
			if (index == 0) return ref vec.X;
			else if (index == 1) return ref vec.Y;
			else if (index == 2) return ref vec.Z;
			else throw new IndexOutOfRangeException("Vector3 indexer range: 0-2");
		}
	}
	public struct VecI2
	{
		public int X, Y;
		public VecI2(int x, int y)
		{
			X = x;
			Y = y;
		}
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
		public VecI3(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
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
