using System;
using System.Collections.Generic;
using System.Text;

namespace RoseworksTest
{
	public struct SExampleData
	{
		public int A;
		public int B;
		public static void Clear(ref SExampleData obj)
		{
			obj.B = 0;
			obj.A = 0;
		}
		public static void Move(ref SExampleData from, ref SExampleData to)
		{
			to.B = from.B;
			to.A = from.A;
		}
	}
}
