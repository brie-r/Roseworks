using System;
using System.Numerics;
using RelaStructures;
using Roseworks;

namespace RoseworksTest
{
	public class BInputExampleMove : Behavior, IInputMove
	{
		public Type[] Dependencies { get; set; } = { };
		public bool ShouldTick { get; set; } = false;

		public StructReArray<SExampleData> Data = new StructReArray<SExampleData>(ushort.MaxValue, ushort.MaxValue, SExampleData.Clear, SExampleData.Move);
		public Vector2 Output;

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
		public void MoveInput(int comID, Vector2 moveVec)
		{
			Output = moveVec;
		}

		public void MoveInput(int comID, float moveX)
		{
			throw new NotImplementedException();
		}

		public void MoveInput(int comID, float moveX, float moveY)
		{
			throw new NotImplementedException();
		}

		public void MoveInput(int comID, Vector3 moveVec)
		{
			throw new NotImplementedException();
		}

		public void MoveInput(int comID, float moveX, float moveY, float moveZ)
		{
			throw new NotImplementedException();
		}
	}
}