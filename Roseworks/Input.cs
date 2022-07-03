using System.Diagnostics;
using System.Collections.Generic;
using Roseworks;


#pragma warning disable 0414
namespace Roseworks
{
	public static class Input
	{
		public static int[] InputEntIDs;

		[System.Flags] public enum EInputTypes { None = 0, Trigger = 1 << 0, Move = 1 << 1, Mouse = 1 << 2 }
		public enum EInputTypesNoFlag {Trigger, Move, Mouse}
		public static int InputTypeCount = System.Enum.GetValues(typeof(EInputTypes)).Length - 1;

		public static Dictionary<System.Type, int[]> ComTypeToRefsIx = new Dictionary<System.Type, int[]>();
		public static Dictionary<System.Type, int> TypeToInputTypes = new Dictionary<System.Type, int>();
		public static RelaStructures.StructReArray<SData> Data = new RelaStructures.StructReArray<SData>(byte.MaxValue + 1, short.MaxValue + 1, ClearAction, MoveAction, InitAction);
		public static System.Type[] InputTypes = { typeof(IInputTrigger), typeof(IInputMove), typeof(IInputMouse) };
		public static List<IInputTrigger> TriggerRefs = new List<IInputTrigger>();
		public static List<IInputMove> MoveRefs = new List<IInputMove>();
		public static List<IInputMouse> MouseRefs = new List<IInputMouse>();
		public struct SData
		{
			public int ComID;
			public System.Type ComType;
			public int InputTypeFlags;
			public int[] RefIx;
			public int PlayerID;
		}
		private static void ClearAction(ref SData obj)
		{
			obj.ComID = -1;
			obj.ComType = null;
			obj.InputTypeFlags = 0;
			obj.PlayerID = -1;
			System.Array.Fill(obj.RefIx, -1);
		}
		private static void MoveAction(ref SData from, ref SData to)
		{
			to.ComID = from.ComID;
			to.ComType = from.ComType;
			to.InputTypeFlags = from.InputTypeFlags;
			to.RefIx = from.RefIx;
			to.PlayerID = from.PlayerID;
			System.Array.Copy(from.RefIx, to.RefIx, from.RefIx.Length);
		}
		private static void InitAction(ref SData obj)
		{
			obj.RefIx = new int[InputTypeCount];
			System.Array.Fill(obj.RefIx, -1);
		}

		private static StackTrace st = new StackTrace();
		public static bool DebugPrint = true;

		// store ents that receive input
		// send inputs to their behaviors if ReceivesInput == true
		// store behavior property structs instead of specific types (dimensions, constraints, whatever)
		// simulted input?

		public static void AddInputBehavior(Behavior b)
		{
			int inputTypes = 0;
			int currentInputTypeCount = 0;
			for (int i = 0; i < InputTypeCount; i++)
			{
				if (InputTypes[i].IsAssignableFrom(b.GetType()) == true)
				{
					inputTypes |= 1 >> i;
					currentInputTypeCount = 0;
					if (b is IInputTrigger)
					{
						TriggerRefs.Add(b as IInputTrigger);
						currentInputTypeCount = TriggerRefs.Count;
					}
					if (b is IInputMove)
					{
						MoveRefs.Add(b as IInputMove);
						currentInputTypeCount = MoveRefs.Count;
					}
					if (b is IInputMouse)
					{
						MouseRefs.Add(b as IInputMouse);
						currentInputTypeCount = MouseRefs.Count;
					}
					ComTypeToRefsIx.TryAdd(b.GetType(), new int[InputTypeCount]);
					ComTypeToRefsIx[b.GetType()][i] = currentInputTypeCount - 1;
				}
			}
			TypeToInputTypes[b.GetType()] = inputTypes;
		}
		public static void AddInputCom(System.Type comType, int comID)
		{
			int dataID = Data.Request();
			ref SData data = ref Data.AtId(dataID);
			data.ComID = comID;
			data.ComType = comType;
			for (int i = 0; i < InputTypeCount; i++)
			{
				Logger.WriteLine("AddInputCom...\tComType: " + comType + "\t" + InputTypes[i] + "\t" + comType + "\t" + InputTypes[i].IsAssignableFrom(comType));
				if (InputTypes[i].IsAssignableFrom(comType))
				{
					data.InputTypeFlags |= 1 << i;
					for (int printIx = 0; printIx < ComTypeToRefsIx[comType].Length; printIx++)
					{
						// ComTypeToRefsIx set wrong
						Logger.Write("" + ComTypeToRefsIx[comType][printIx] + " ");
					}
					Logger.WriteLine("");
					data.RefIx = ComTypeToRefsIx[comType];
				}
			}
		}
		public static void SendMove(float context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if (UBit.HasFlag(Data[i].InputTypeFlags, (int)EInputTypes.Move))
				{
					refIx = Data[i].RefIx[(int)EInputTypesNoFlag.Move];
					comID = Data[i].ComID;
					MoveRefs[refIx].MoveInput(comID, context);
				}
			}
		}
		public static void SendMove(VecF2 context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if (UBit.HasFlag(Data[i].InputTypeFlags, (int)EInputTypes.Move))
				{
					// Data.RefIx not being set correctly for all
					refIx = Data[i].RefIx[(int)EInputTypesNoFlag.Move];
					Logger.WriteLine("Flag.Move: " + (int)EInputTypesNoFlag.Move + "\tRefIx: " + refIx + "\tMoveRefs.Count: " + MoveRefs.Count);
					comID = Data[i].ComID;
					MoveRefs[refIx].MoveInput(comID, context);
				}
			}
		}
		public static void SendMove(VecF3 context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if (UBit.HasFlag(Data[i].InputTypeFlags, (int)EInputTypes.Move))
				{
					refIx = Data[i].RefIx[(int)EInputTypesNoFlag.Move];
					comID = Data[i].ComID;
					MoveRefs[refIx].MoveInput(comID, context);
				}
			}
		}
		public static void SendMouse(VecF2 context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				if (UBit.HasFlag(Data[i].InputTypeFlags, (int)EInputTypes.Mouse))
				{
					refIx = Data[i].RefIx[(int)EInputTypesNoFlag.Mouse];
					comID = Data[i].ComID;
					MouseRefs[refIx].MouseInput(comID, context);
				}
			}
		}
		public static void SendTrigger(int triggerSlot, int value)
		{
			int refIx;
			int comID;
			for (int i = 0; i < Data.Count; i++)
			{
				refIx = Data[i].RefIx[(int)EInputTypesNoFlag.Trigger];
				comID = Data[i].ComID;
				if (DebugPrint)
					Logger.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "(slot " + triggerSlot + " " + (value != 0 ? "down" : "up") + ")");
				if (
					refIx >= 0 &&
					comID >= 0 &&
					UBit.HasFlag(Data[i].InputTypeFlags, (int)EInputTypes.Trigger) &&
					UBit.GetBit(TriggerRefs[refIx].SlotFlags, triggerSlot))
				{
					if (DebugPrint) Logger.WriteLine("Input sent");
					if (value != 0)
						TriggerRefs[refIx].StartInput(comID, triggerSlot, value);
					else
						TriggerRefs[refIx].EndInput(comID, triggerSlot);
				}
				else
				{
					if (DebugPrint) Logger.WriteLine("Input not sent");
				}

			}
		}
	}

	public interface IInput
	{
	}
	public interface IInputMove: IInput
	{
		void MoveInput(int comID, float moveX);
		void MoveInput(int comID, VecF2 moveVec);
		void MoveInput(int comID, float moveX, float moveY);
		void MoveInput(int comID, VecF3 moveVec);
		void MoveInput(int comID, float moveX, float moveY, float moveZ);

	}
	public interface IInputMouse : IInput
	{
		void MouseInput(int comID, VecF2 mouseVec);
	}
	public interface IInputTrigger : IInput
	{
		void StartInput(int comID, int slot, float value = 1);
		// Slot -1 fires when any fire input changes values
		void EndInput(int comID, int slot);
		public int SlotFlags { get; set; }
	}
}