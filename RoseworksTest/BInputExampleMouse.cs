using System;
using System.Collections.Generic;
using System.Text;
using RelaStructures;
using Roseworks;

namespace RoseworksTest
{
	public class BInputExampleMouse : Behavior, IInputMouse
	{
		public Type[] Dependencies { get; set; } = { };
		public bool ShouldTick { get; set; } = false;

		public StructReArray<SExampleData> Data = new StructReArray<SExampleData>(ushort.MaxValue, ushort.MaxValue, SExampleData.Clear, SExampleData.Move);
		public VecF2 Output;

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
		public void MouseInput(int comID, VecF2 mouseVec)
		{
			Output = mouseVec;
		}
	}
}