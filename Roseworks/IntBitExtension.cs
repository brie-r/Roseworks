using System;
using System.Collections.Generic;
using System.Text;

namespace Roseworks
{
	public static class IntBitExtension
	{
		public static bool GetBit(this int flagInt, int index)
		{
			return ((flagInt & (1 << index)) != 0);
		}
		public static void SetBit(this ref int flagInt, int index, bool value)
		{
			if (value)
				flagInt |= 1 << index;
			else
				flagInt &= ~(1 << index);
		}
	}
}
