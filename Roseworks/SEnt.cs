using System.Collections;
using System.Collections.Generic;
using RelaStructures;
namespace Roseworks
{
	public struct SEnt
	{
		public int ComCount;
		public System.Type[] ComTypes;
		public int[] ComIDs;
		public int[] DataIDs;
		public static void Init(ref SEnt obj)
		{
			obj.ComTypes = new System.Type[ECS.MaxComsPerEnt];
			obj.ComIDs = new int[ECS.MaxComsPerEnt];
			obj.DataIDs = new int[ECS.MaxComsPerEnt];
		}
		public static void Clear(ref SEnt obj)
		{
			obj.ComCount = 0;
			System.Array.Clear(obj.ComTypes, 0, obj.ComCount);
			System.Array.Clear(obj.ComIDs, 0, obj.ComCount);
			System.Array.Clear(obj.DataIDs, 0, obj.ComCount);
		}
		public static void Move(ref SEnt from, ref SEnt to)
		{
			to.ComCount = from.ComCount;
			System.Array.Copy(from.ComIDs, to.ComIDs, ECS.MaxComsPerEnt);
			System.Array.Copy(from.ComTypes, to.ComTypes, ECS.MaxComsPerEnt);
			System.Array.Copy(from.DataIDs, to.DataIDs, ECS.MaxComsPerEnt);
		}
	}
}