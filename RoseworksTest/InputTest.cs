using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using Roseworks;

namespace RoseworksTest
{

	[TestClass]
	public class InputTest
	{
		public Vector2 Output = new Vector2(4, 20);
		public (int slotFlags, int slotsToSendFlags, int valueFlags)[] StartEndTestCases =
		{
			(0, 0, 0), (1, 1, 1), (5, 6, 6)
		};

		[TestMethod]
		public void TestMouse()
		{
			TestUtil.Init();
			ECS.AddBehavior<BInputExampleMouse>();
			ECS.AddEntWithCom<BInputExampleMouse>();
			Assert.AreEqual(1, Input.State.MouseRefs.Count);
			Assert.AreEqual(1, Input.State.Data.Count);
			Input.SendMouse(Output);
			Vector2 v = ((BInputExampleMouse)ECS.State.TypeToRef[typeof(BInputExampleMouse)]).Output;
			Assert.AreEqual(Output.X, v.X);
		}
		[TestMethod]
		public void TestMove()
		{
			TestUtil.Init();
			ECS.AddBehavior<BInputExampleMove>();
			ECS.AddEntWithCom<BInputExampleMove>();
			Assert.AreEqual(1, Input.State.MoveRefs.Count);
			Assert.AreEqual(1, Input.State.Data.Count);
			Input.SendMove(Output);
			Vector2 v = ((BInputExampleMove)ECS.State.TypeToRef[typeof(BInputExampleMove)]).Output;
			Assert.AreEqual(Output.X, v.X);
		}
		[TestMethod]
		public void TestTrigger()
		{
			TestUtil.Init();
			ECS.AddBehavior<BInputExampleTrigger>();
			ECS.AddEntWithCom<BInputExampleTrigger>();
			Assert.AreEqual(1, Input.State.TriggerRefs.Count);
			Assert.AreEqual(1, Input.State.Data.Count);
			IInputTrigger com = (IInputTrigger)ECS.State.TypeToRef[typeof(BInputExampleTrigger)];
			for (int testIx = 0; testIx < StartEndTestCases.Length; testIx++)
			{

				ref (int slotFlags, int slotsToSendFlags, int valueFlags) test = ref StartEndTestCases[testIx];
				com.SlotFlags = test.slotFlags;
				for (int slotIx = 0; slotIx < TriggerSlots.Count; slotIx++)
				{

					//
					// slot 0 incorrectly sent in test 3?
					// console output looks right but
					// debug log indicates SendTrigger(0)
					// is called regardless
					//

					Console.WriteLine("slots to send flags: " + test.slotsToSendFlags + "\tslot: " + slotIx + "\tbit: " + UBit.GetBit(test.slotsToSendFlags, slotIx));
					if (UBit.GetBit(test.slotsToSendFlags, slotIx))
						Input.SendTrigger(triggerSlot: slotIx, value: UBit.GetBit(test.valueFlags, slotIx) ? 1 : 0);
				}
				ref VecI2 realOutput = ref ((BInputExampleTrigger)ECS.State.TypeToRef[typeof(BInputExampleTrigger)]).Output;
				int slotsInUse = test.slotFlags & test.slotsToSendFlags;
				int expectedStart = slotsInUse & test.valueFlags;
				int expectedEnd = slotsInUse & ~test.valueFlags;
				Console.WriteLine("Expected start: " + expectedStart + "\tReal start: " + realOutput[0]);
				Console.WriteLine("Expected end: " + expectedEnd + "\tReal end: " + realOutput[1]);
				Assert.AreEqual(expectedStart, realOutput[0]);
				Assert.AreEqual(expectedEnd, realOutput[1]);
				realOutput = new VecI2();
			}
		}
	}
}
