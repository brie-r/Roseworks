using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using Roseworks;
using static System.Net.Mime.MediaTypeNames;

namespace RoseworksTest
{

	[TestClass]
	public class InputTest
	{
		public Vector2 Output = new Vector2(4, 20);
		public (
			int slotFlags,
			int slotsToSendFlags,
			int valueFlags
		)[] TriggerTestCases =
		{
			(0b000, 0b000, 0b000), // start 000 end 000
			(0b001, 0b001, 0b001), // start 001 end 000
			(0b101, 0b110, 0b111), // start 100 end 000
			(0b101, 0b111, 0b110), // start 100 end 001
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
		}
		[TestMethod]
		public void TestTrigger0()
		{
			TestTrigger();
			ref (int slotFlags, int slotsToSendFlags, int valueFlags) test = ref TriggerTestCases[0];
			for (int slotIx = 0; slotIx < TriggerSlots.Count; slotIx++)
			{
				TestTriggerDebugPerSlot(ref test, slotIx);
				if (UBit.GetBit(test.slotsToSendFlags, slotIx))
					Input.SendTrigger(triggerSlot: slotIx, value: test.valueFlags.GetBit(slotIx) ? 1 : 0);
			}
		}
		[TestMethod]
		public void TestTrigger1()
		{
			TestTrigger();
			ref (int slotFlags, int slotsToSendFlags, int valueFlags) test = ref TriggerTestCases[1];
			for (int slotIx = 0; slotIx < TriggerSlots.Count; slotIx++)
			{
				TestTriggerDebugPerSlot(ref test, slotIx);
				if (UBit.GetBit(test.slotsToSendFlags, slotIx))
					Input.SendTrigger(triggerSlot: slotIx, value: test.valueFlags.GetBit(slotIx) ? 1 : 0);
			}
		}
		[TestMethod]
		public void TestTrigger2()
		{
			TestTrigger();
			ref (int slotFlags, int slotsToSendFlags, int valueFlags) test = ref TriggerTestCases[2];
			for (int slotIx = 0; slotIx < TriggerSlots.Count; slotIx++)
			{
				TestTriggerDebugPerSlot(ref test, slotIx);
				if (UBit.GetBit(test.slotsToSendFlags, slotIx))
					Input.SendTrigger(triggerSlot: slotIx, value: test.valueFlags.GetBit(slotIx) ? 1 : 0);
			}
		}
		[TestMethod]
		public void TestTrigger3()
		{
			TestTrigger();
			ref (int slotFlags, int slotsToSendFlags, int valueFlags) test = ref TriggerTestCases[3];
			for (int slotIx = 0; slotIx < TriggerSlots.Count; slotIx++)
			{
				TestTriggerDebugPerSlot(ref test, slotIx);
				if (UBit.GetBit(test.slotsToSendFlags, slotIx))
					Input.SendTrigger(triggerSlot: slotIx, value: test.valueFlags.GetBit(slotIx) ? 1 : 0);
			}
		}
		public void TestTriggerDebugPerSlot(ref (int slotFlags, int slotsToSendFlags, int valueFlags) test, int slotIx)
		{
			Console.WriteLine("slots to send flags: " + Convert.ToString(test.slotsToSendFlags, 2).PadLeft(3, '0') + "\tslot: " + slotIx + "\tbit: " + UBit.GetBit(test.slotsToSendFlags, slotIx));
		}
		public void TestTriggerDebug(ref (int slotFlags, int slotsToSendFlags, int valueFlags) test)
		{
			ref int outputStart = ref ((BInputExampleTrigger)ECS.State.TypeToRef[typeof(BInputExampleTrigger)]).OutputStart;
			ref int outputEnd = ref ((BInputExampleTrigger)ECS.State.TypeToRef[typeof(BInputExampleTrigger)]).OutputEnd;
			int slotsInUse = test.slotFlags & test.slotsToSendFlags;
			int expectedStart = slotsInUse & test.valueFlags;
			int expectedEnd = slotsInUse & ~test.valueFlags;
			Console.WriteLine("Expected start: " + Convert.ToString(expectedStart, 2).PadLeft(3, '0') + "\tReal start: " + Convert.ToString(outputStart, 2).PadLeft(3, '0'));
			Console.WriteLine("Expected end: " + Convert.ToString(expectedEnd, 2).PadLeft(3, '0') + "\tReal end: " + Convert.ToString(outputEnd, 2).PadLeft(3, '0'));
			Assert.AreEqual(expectedStart, outputStart);
			Assert.AreEqual(expectedEnd, outputEnd);
		}
	}
}
