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
		public int OutputStart, OutputEnd;

		public int InitCom(int comID, int entID)
		{
			int dataID = Data.Request();
			SetDefaultData(dataID);
			ECS.ComAtId(comID).DataID = dataID;
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
			OutputStart.SetBit(slot, true);
		}
		public void EndInput(int comID, int slot)
		{
			OutputEnd.SetBit(slot, true);
		}
	}
}