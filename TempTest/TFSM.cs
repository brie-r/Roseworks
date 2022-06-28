using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Roseworks;

namespace TestFSM
{
	public class TestParams
	{
	
	}
	public class TFSM : MonoBehaviour
	{
		public void TestInst(TestParams t)
		{
			ECS.InitScene(this);
			int entID = ECS.AddEnt();
			FSM.AddState(typeof(SimpleFSM), entID);
		}
	}
	public class SimpleFSM : Behavior
	{
		public bool ShouldTick { get; set; } = false;
		public Type[] Dependencies { get; set; } = { };
		public enum States : byte { Test0, Test1, Test2 };

		public int InitCom(int comID, int entID)
		{
			return FSM.AddState(comType: typeof(States), entID);
		}
	}
}
