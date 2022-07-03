using System;
using System.Collections.Generic;
using System.Text;

namespace Roseworks
{
	public static class UBit
	{
		public static bool HasFlag(int a, int b)
		{
			return ((a & b) != 0);
		}
		public static bool GetBit(int a, int i)
		{
			return ((a & (1 << i)) != 0);
		}
	}
}
