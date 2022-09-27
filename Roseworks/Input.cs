using System.Diagnostics;
using System.Numerics;

#pragma warning disable 0414
namespace Roseworks
{
	public static class Input
	{
		private static StackTrace st = new StackTrace();
		public static bool DebugPrint = true;
		public static InputState State;

		public static void Init()
		{
			State = new InputState();
			State.Init();
		}
		public struct SInput
		{
			public int ComID;
			public System.Type ComType;
			public int InputTypeFlags;
			public int[] RefIx;
			public int PlayerID;
		}
		public static void ClearAction(ref SInput obj)
		{
			obj.ComID = -1;
			obj.ComType = null;
			obj.InputTypeFlags = 0;
			obj.PlayerID = -1;
			System.Array.Fill(obj.RefIx, -1);
		}
		public static void MoveAction(ref SInput from, ref SInput to)
		{
			to.ComID = from.ComID;
			to.ComType = from.ComType;
			to.InputTypeFlags = from.InputTypeFlags;
			to.RefIx = from.RefIx;
			to.PlayerID = from.PlayerID;
			System.Array.Copy(from.RefIx, to.RefIx, from.RefIx.Length);
		}
		public static void InitAction(ref SInput obj)
		{
			obj.RefIx = new int[State.InputTypeCount];
			System.Array.Fill(obj.RefIx, -1);
		}

		public static void AddInputBehavior(Behavior b)
		{
			int inputTypes = 0;
			int currentInputTypeCount;
			for (int i = 0; i < State.InputTypeCount; i++)
			{
				if (State.InputTypes[i].IsAssignableFrom(b.GetType()) == true)
				{
					inputTypes |= 1 >> i;
					currentInputTypeCount = 0;
					if (b is IInputTrigger)
					{
						State.TriggerRefs.Add(b as IInputTrigger);
						currentInputTypeCount = State.TriggerRefs.Count;
					}
					if (b is IInputMove)
					{
						State.MoveRefs.Add(b as IInputMove);
						currentInputTypeCount = State.MoveRefs.Count;
					}
					if (b is IInputMouse)
					{
						State.MouseRefs.Add(b as IInputMouse);
						currentInputTypeCount = State.MouseRefs.Count;
					}
					State.ComTypeToRefsIx.TryAdd(b.GetType(), new int[State.InputTypeCount]);
					State.ComTypeToRefsIx[b.GetType()][i] = currentInputTypeCount - 1;
				}
			}
			State.TypeToInputTypes[b.GetType()] = inputTypes;
		}
		public static void AddInputCom(System.Type comType, int comID)
		{
			int dataID = State.Data.Request();
			ref SInput data = ref State.Data.AtId(dataID);
			data.ComID = comID;
			data.ComType = comType;
			for (int i = 0; i < State.InputTypeCount; i++)
			{
				if (State.InputTypes[i].IsAssignableFrom(comType))
				{
					data.InputTypeFlags.SetBit(i, true);
					data.RefIx = State.ComTypeToRefsIx[comType];
				}
			}
		}
		public static void SendMove(float context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < State.Data.Count; i++)
			{
				if (State.Data[i].InputTypeFlags.GetBit((int)InputState.EInputTypes.Move))
				{
					refIx = State.Data[i].RefIx[(int)InputState.EInputTypes.Move];
					comID = State.Data[i].ComID;
					State.MoveRefs[refIx].MoveInput(comID, context);
				}
			}
		}
		public static void SendMove(Vector2 context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < State.Data.Count; i++)
			{
				if (State.Data[i].InputTypeFlags.GetBit((int)InputState.EInputTypes.Move))
				{
					refIx = State.Data[i].RefIx[(int)InputState.EInputTypes.Move];
					comID = State.Data[i].ComID;
					State.MoveRefs[refIx].MoveInput(comID, context);
				}
			}
		}
		public static void SendMove(Vector3 context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < State.Data.Count; i++)
			{
				if (State.Data[i].InputTypeFlags.GetBit((int)InputState.EInputTypes.Move))
				{
					refIx = State.Data[i].RefIx[(int)InputState.EInputTypes.Move];
					comID = State.Data[i].ComID;
					State.MoveRefs[refIx].MoveInput(comID, context);
				}
			}
		}
		public static void SendMouse(Vector2 context)
		{
			int refIx;
			int comID;
			for (int i = 0; i < State.Data.Count; i++)
			{
				if (State.Data[i].InputTypeFlags.GetBit((int)InputState.EInputTypes.Mouse))
				{
					refIx = State.Data[i].RefIx[(int)InputState.EInputTypes.Mouse];
					comID = State.Data[i].ComID;
					State.MouseRefs[refIx].MouseInput(comID, context);
				}
			}
		}
		public static void SendTrigger(int triggerSlot, int value)
		{
			int refIx;
			int comId;
			for (int i = 0; i < State.Data.Count; i++)
			{
				ref SInput data = ref State.Data[i];
				refIx = data.RefIx[(int)InputState.EInputTypes.Trigger];
				comId = data.ComID;
				/*
				Logger.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "\r\n\ttriggerSlot: " + triggerSlot + "\r\n\tvalue: " + value + "\r\n\tdata index: " + i + "\r\n\trefIx: " + refIx + "\r\n\tcomId: " + comId + "\r\n\tClass implements IInputTrigger? " + data.InputTypeFlags.GetBit((int)InputState.EInputTypes.Trigger) + "\r\n\tClass implementing IInputTrigger accepts slot " + triggerSlot + "? " + State.TriggerRefs[refIx].SlotFlags.GetBit(triggerSlot));
				*/
				if (
					refIx >= 0 &&
					comId >= 0 &&
					data.InputTypeFlags.GetBit((int)InputState.EInputTypes.Trigger) &&
					State.TriggerRefs[refIx].SlotFlags.GetBit(triggerSlot))
				{
					if (value != 0)
						State.TriggerRefs[refIx].StartInput(comId, triggerSlot, value);
					else
						State.TriggerRefs[refIx].EndInput(comId, triggerSlot);
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
		void MoveInput(int comID, Vector2 moveVec);
		void MoveInput(int comID, float moveX, float moveY);
		void MoveInput(int comID, Vector3 moveVec);
		void MoveInput(int comID, float moveX, float moveY, float moveZ);

	}
	public interface IInputMouse : IInput
	{
		void MouseInput(int comID, Vector2 mouseVec);
	}
	public interface IInputTrigger : IInput
	{
		void StartInput(int comID, int slot, float value = 1);
		// Slot -1 fires when any fire input changes values
		void EndInput(int comID, int slot);
		public int SlotFlags { get; set; }
	}
}