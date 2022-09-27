using System.Diagnostics;
using System.Collections.Generic;
using Roseworks;

namespace Roseworks
{
	public class InputState
	{
		public int[] InputEntIDs;
		public enum EInputTypes { Trigger, Move, Mouse }
		public int InputTypeCount = System.Enum.GetValues(typeof(EInputTypes)).Length;

		public Dictionary<System.Type, int[]> ComTypeToRefsIx = new Dictionary<System.Type, int[]>();
		public Dictionary<System.Type, int> TypeToInputTypes = new Dictionary<System.Type, int>();
		public RelaStructures.StructReArray<Input.SInput> Data;
		public System.Type[] InputTypes = { typeof(IInputTrigger), typeof(IInputMove), typeof(IInputMouse) };
		public List<IInputTrigger> TriggerRefs = new List<IInputTrigger>();
		public List<IInputMove> MoveRefs = new List<IInputMove>();
		public List<IInputMouse> MouseRefs = new List<IInputMouse>();

		public void Init()
		{
			Data = new RelaStructures.StructReArray<Input.SInput>(byte.MaxValue + 1, short.MaxValue + 1, Input.ClearAction, Input.MoveAction, Input.InitAction);
		}
	}
}