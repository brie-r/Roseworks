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
			obj.ComTypes = new System.Type[ECS.State.MaxComsPerEnt];
			obj.ComIDs = new int[ECS.State.MaxComsPerEnt];
			obj.DataIDs = new int[ECS.State.MaxComsPerEnt];
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
			System.Array.Copy(from.ComIDs, to.ComIDs, ECS.State.MaxComsPerEnt);
			System.Array.Copy(from.ComTypes, to.ComTypes, ECS.State.MaxComsPerEnt);
			System.Array.Copy(from.DataIDs, to.DataIDs, ECS.State.MaxComsPerEnt);
		}
	}
}