using System.Collections;
using System.Collections.Generic;
namespace Roseworks
{ 
	public struct SCom
	{
		public int DataID;
		public int EntID;
		public System.Type ComType;
		public bool Active;
		public static void Clear(ref SCom obj)
		{
			obj.EntID = 0;
			obj.DataID = 0;
			obj.ComType = null;
			obj.Active = false;
		}
		public static void Move(ref SCom from, ref SCom to)
		{
			to.EntID = from.EntID;
			to.DataID = from.DataID;
			to.ComType = from.ComType;
			to.Active = from.Active;
		}
}
}