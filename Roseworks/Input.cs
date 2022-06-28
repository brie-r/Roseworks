using System.Diagnostics;
using System.Collections.Generic;

#pragma warning disable 0414
namespace Roseworks
{
	public static class Input
	{
		public static int[] InputEntIDs;

		// never initialized
		public static Dictionary<System.Type, List<int>> TypeToRefsIndex = new Dictionary<System.Type, List<int>>();
		public static Dictionary<System.Type, int> TypeToInputTypes = new Dictionary<System.Type, int>();
		public static RelaStructures.StructReArray<SData> Data = new RelaStructures.StructReArray<SData>(byte.MaxValue + 1, short.MaxValue + 1, ClearAction, MoveAction);
		[System.Flags] public enum EInputTypes { None = 0, StartEnd = 1 << 0, Move = 1 << 1, Mouse = 1 << 2 }
		public static int InputTypeCount = System.Enum.GetValues(typeof(EInputTypes)).Length - 1;
		public static System.Type[] InputTypes = { typeof(IStartEnd), typeof(IMove), typeof(IMouse) };
		public static Dictionary<System.Type, int> InputTypeToFlags = new Dictionary<System.Type, int>();
		public static List<IStartEnd> StartEndRefs = new List<IStartEnd>();
		public static List<IMove> MoveRefs = new List<IMove>();
		public static List<IMouse> MouseRefs = new List<IMouse>();
		public struct SData
		{
			public int ComID;
			public System.Type ComType;
			public int InputTypes;
			public int RefIndex;
			public int PlayerID;
		}
		private static void ClearAction(ref SData obj)
		{
			obj.ComID = -1;
			obj.ComType = null;
			obj.InputTypes = 0;
			obj.RefIndex = -1;
			obj.PlayerID = -1;
		}
		private static void MoveAction(ref SData from, ref SData to)
		{
			to.ComID = from.ComID;
			to.ComType = from.ComType;
			to.InputTypes = from.InputTypes;
			to.RefIndex = from.RefIndex;
			to.PlayerID = from.PlayerID;
		}

		private static StackTrace st = new StackTrace();
		public static bool DebugPrint = false;

		// store ents that receive input
		// send inputs to their behaviors if ReceivesInput == true
		// store behavior property structs instead of specific types (dimensions, constraints, whatever)
		// simulted input?

		public static int AddInputBehavior(Behavior b)
		{
			int inputTypes = 0;
			int index = -1;
			if (b is IMouse)
			{
				inputTypes |= (int) EInputTypes.Mouse;
				MouseRefs.Add(b as IMouse);
				TypeToRefsIndex.TryAdd(b.GetType(), new List<int>());
				TypeToRefsIndex[b.GetType()].Add(MouseRefs.Count - 1);
				index = MouseRefs.Count - 1;
			}
			if (b is IMove)
			{
				inputTypes |= (int)EInputTypes.Move;
				MoveRefs.Add(b as IMove);
				TypeToRefsIndex.TryAdd(b.GetType(), new List<int>());
				TypeToRefsIndex[b.GetType()].Add(MoveRefs.Count - 1);
				index = MoveRefs.Count - 1;
			}
			if (b is IStartEnd)
			{
				inputTypes |= (int)EInputTypes.StartEnd;
				StartEndRefs.Add(b as IStartEnd);
				TypeToRefsIndex.TryAdd(b.GetType(), new List<int>());
				TypeToRefsIndex[b.GetType()].Add(StartEndRefs.Count - 1);
				index = StartEndRefs.Count - 1;
			}

			TypeToInputTypes[b.GetType()] = inputTypes;
			return index;
		}
		public static void AddInputCom<ComType, InputType>(int comID)
		{
			int dataID = Data.Request();
			ref SData data = ref Data.AtId(dataID);
			data.ComID = comID;
			data.ComType = typeof(ComType);
			data.InputTypes |= InputTypeToFlags[typeof(InputType)];
			data.RefIndex = AddInputBehavior(ECS.TypeToRef[typeof(ComType)]);
		}
		public static void SendMove(float context)
		{
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if (Data[i].ComType == typeof(IMove))
				{
					comID = Data[i].ComID;
					MoveRefs[Data[i].RefIndex].MoveInput(comID, context);
				}
			}
		}
		public static void SendMove(VecF2 context)
		{
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if(Data[i].ComType == typeof(IMove))
				{
					comID = Data[i].ComID;
					MoveRefs[Data[i].RefIndex].MoveInput(comID, context);
				}
			}
		}
		public static void SendMove(VecF3 context)
		{
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if (Data[i].ComType == typeof(IMove))
				{
					comID = Data[i].ComID;
					MoveRefs[Data[i].RefIndex].MoveInput(comID, context);
				}
			}
		}
		public static void SendMouse(VecF2 context)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				int comID = Data[i].ComID;
				MouseRefs[Data[i].RefIndex].MouseInput(comID, context);
			}
		}
		public static void SendStartEnd(int fireSlot, int value)
		{
			int comID;
			int refIndex;
			for (int i = 0; i < Data.Count; i++)
			{
				if (Data[i].ComType == typeof(IMouse))
				{
					comID = Data[i].ComID;
					refIndex = Data[i].RefIndex;

					bool shouldFire = ((StartEndRefs[refIndex].SlotFlags & (1 << fireSlot)) != 0);
					if (shouldFire)
					{
						if (DebugPrint) Logger.WriteLine("InputHandler.HandleFire()");
						if (value != 0)
							StartEndRefs[refIndex].StartInput(comID, fireSlot, value);
						else
							StartEndRefs[refIndex].EndInput(comID, fireSlot);
					}
					if (DebugPrint)
						Logger.WriteLine(
							System.Reflection.MethodBase.GetCurrentMethod().Name + " " +
							fireSlot + " " + (value != 0 ? "down" : "up") + " " + value);
					if (DebugPrint) Logger.WriteLine("InputHandler.HandleFire( " + fireSlot + " " + (value!=0?"down":"up") + " " + value + " )");
				}
			}
		}
	}

	public interface IInput
	{
	}
	public interface IMove: IInput
	{
		void MoveInput(int comID, float moveX);
		void MoveInput(int comID, VecF2 moveVec);
		void MoveInput(int comID, float moveX, float moveY);
		void MoveInput(int comID, VecF3 moveVec);
		void MoveInput(int comID, float moveX, float moveY, float moveZ);

	}
	public interface IMouse : IInput
	{
		void MouseInput(int comID, VecF2 mouseVec);
	}
	public interface IStartEnd : IInput
	{
		void StartInput(int comID, int slot, float value = 1);
		// Slot -1 fires when any fire input changes values
		void EndInput(int comID, int slot);
		public int SlotFlags { get; set; }
	}
}