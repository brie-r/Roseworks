using System;
using System.Collections.Generic;
using System.Text;
using RelaStructures;
using Roseworks;

namespace RoseworksTest
{
	public class BInputExampleTrigger : Behavior, IInputTrigger
	{
		public Type[] Dependencies { get; set; } = { };
		public bool ShouldTick { get; set; } = false;
		public int SlotFlags { get; set; }

		public StructReArray<SExampleData> Data = new StructReArray<SExampleData>(ushort.MaxValue, ushort.MaxValue, SExampleData.Clear, SExampleData.Move);
		public VecI2 Output;

		public int InitCom(int comID, int entID)
		{
			int dataID = Data.Request();
			SetDefaultData(dataID);
			ECS.Coms.AtId(comID).DataID = dataID;
			return dataID;
		}
		public void SetDefaultData(int dataID)
		{
			ref SExampleData data = ref Data.AtId(dataID);
			data.A = 6;
			data.B = 9;
		}
		public int Sum(int dataID)
		{
			ref SExampleData data = ref Data.AtId(dataID);
			return data.A + data.B;
		}
		public void StartInput(int comID, int slot, float value = 1)
		{
			Output[0] |= 1 << slot;
		}
		public void EndInput(int comID, int slot)
		{
			Output[1] |= 1 << slot;
		}
	}
}