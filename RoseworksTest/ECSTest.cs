using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Roseworks;

namespace RoseworksTest
{
	[TestClass]
	public class ECSTest
	{
		[TestMethod]
		public void Init()
		{
			TestUtil.Init();
			for (int i = 0; i < ECS.State.TypeList.Count; i++)
				Console.WriteLine(ECS.State.TypeList[i]);
			Assert.IsTrue(ECS.State.TypeList.Count == 0);
			TestUtil.Init();
			ECS.AddBehavior<BExample0>();
			Console.WriteLine();
			for (int i = 0; i < ECS.State.TypeList.Count; i++)
				Console.WriteLine(ECS.State.TypeList[i]);
			Assert.IsTrue(ECS.State.TypeList.Count == 1);
			TestUtil.Init();
			Console.WriteLine();
			for (int i = 0; i < ECS.State.TypeList.Count; i++)
				Console.WriteLine(ECS.State.TypeList[i]);
			Assert.IsTrue(ECS.State.TypeList.Count == 0);
		}
		[TestMethod]
		public void InitAndAddBehavior()
		{
			TestUtil.Init();
			ECS.AddBehavior<BExample0>();
			Console.WriteLine(ECS.State.TypeList.Count);
			Assert.IsTrue(ECS.State.TypeList.Count == 1);
			Console.WriteLine(ECS.State.TypeToRef[typeof(BExample0)]);
			Assert.IsTrue(ECS.State.TypeToRef[typeof(BExample0)] != null);
			Console.WriteLine(ECS.TypeToComIDs<BExample0>().Length);
			Assert.IsTrue(ECS.TypeToComIDs<BExample0>().Length == 0);
		}
		[TestMethod]
		public void InitAndAddBehaviorDependent()
		{
			TestUtil.Init();
			ECS.AddBehavior<BDependent>();

			for (int i = 0; i < ECS.State.TypeList.Count; i++)
				Console.WriteLine(ECS.State.TypeList[i]);
			Assert.AreEqual(2, ECS.State.TypeList.Count);

			Console.WriteLine(ECS.State.TypeToRef[typeof(BDependent)] + " != null");
			Assert.AreNotEqual(null, ECS.State.TypeToRef[typeof(BDependent)]);

			Console.WriteLine("ECS.TypeToComIDs<BDependent>().Length == 0? " + ECS.TypeToComIDs<BDependent>().Length);
			Assert.AreEqual(0, ECS.TypeToComIDs<BDependent>().Length);

			Console.WriteLine(ECS.State.TypeToRef[typeof(BDependency)] + " != null");
			Assert.AreNotEqual(null, ECS.State.TypeToRef[typeof(BDependency)]);

			Console.WriteLine("ECS.TypeToComIDs<BDependency>().Length == 0? " + ECS.TypeToComIDs<BDependency>().Length);
			Assert.AreEqual(0, ECS.TypeToComIDs<BDependency>().Length);
		}
		public void InitAndAddBehaviors()
		{
			TestUtil.Init();
			ECS.AddBehaviors(new Type[]{typeof(BExample0), typeof(BExample1)});
			Assert.IsTrue(ECS.State.TypeList.Count == 2);
			Assert.IsTrue(ECS.State.TypeToRef[typeof(BExample0)] != null);
			Assert.IsTrue(ECS.State.TypeToRef[typeof(BExample1)] != null);
			Assert.IsTrue(ECS.TypeToComIDs<BExample0>().Length == 0);
			Assert.IsTrue(ECS.TypeToComIDs<BExample1>().Length == 0);
		}
		[TestMethod]
		public void AddEntWithCom()
		{
			InitAndAddBehavior();
			ECS.AddEntWithCom<BExample0>();
			Assert.IsTrue(ECS.State.Coms.Count == 1);
			Assert.IsTrue(ECS.State.Coms[0].ComType == typeof(BExample0));
		}
		[TestMethod]
		public void AddEntWithComDependent()
		{
			InitAndAddBehaviorDependent();
			ECS.AddEntWithCom<BDependent>();
			Console.WriteLine();
			Console.WriteLine("Coms: ");
			for (int i = 0; i < ECS.State.Coms.Count; i++)
				Console.WriteLine(ECS.State.Coms[i].ComType);
			Console.WriteLine("ECS.State.Coms.Count == 2? " + ECS.State.Coms.Count);
			Assert.IsTrue(ECS.State.Coms.Count == 2);
			Console.WriteLine("ECS.State.Coms[0].ComType == BDependency? " + ECS.State.Coms[0].ComType + " == " + typeof(BDependency));
			Assert.IsTrue(ECS.State.Coms[0].ComType == typeof(BDependency));
			Console.WriteLine("ECS.State.Coms[1].ComType == BDependent? " + ECS.State.Coms[1].ComType + " == " + typeof(BDependent));
			Assert.IsTrue(ECS.State.Coms[1].ComType == typeof(BDependent));
		}
		[TestMethod]
		public void AddEntWithComs()
		{
			InitAndAddBehaviors();
			ECS.AddEntWithComs(new Type[] { typeof(BExample0), typeof(BExample1) });
			Assert.IsTrue(ECS.State.Coms.Count == 2);
			for (int i = 0; i < ECS.State.Coms.Count; i++)
			{
				Console.WriteLine("ComIX: " + i + "\tComID: " + ECS.State.Coms.IndicesToIds[i] + "\tComType: " + ECS.State.Coms[i].ComType.Name + "\tDataID: " + ECS.State.Coms[i].DataID);
			}
			for (int i = 0; i < ECS.State.Coms.Count; i++)
			{
				Assert.IsTrue(ECS.State.Coms[i].ComType == ((i % 2 == 0) ? typeof(BExample0) : typeof(BExample1)));
				Assert.IsTrue(ECS.State.Coms.IndicesToIds[i] == i);
				Assert.IsTrue(ECS.State.Coms[i].DataID == i/2);
			}
		}
		[TestMethod]
		public void AddEntsWithCom()
		{
			InitAndAddBehavior();
			ECS.AddEntsWithCom(10, typeof(BExample0));
			Console.WriteLine(ECS.State.Coms.Count);
			Assert.IsTrue(ECS.State.Coms.Count == 10);
			for (int i = 0; i < ECS.State.Coms.Count; i++)
				Assert.IsTrue(ECS.State.Coms[i].ComType == typeof(BExample0));
		}
		[TestMethod]
		public void AddEntsWithComs()
		{
			InitAndAddBehaviors();
			ECS.AddEntsWithComs(10, new Type[] { typeof(BExample0), typeof(BExample1) });
			Assert.IsTrue(ECS.State.Ents.Count == 10);
			Assert.IsTrue(ECS.State.Coms.Count == 20);
			Console.WriteLine("=== Ents ===");
			for (int i = 0; i < ECS.State.Ents.Count; i++)
			{
				Console.Write("EntIX: " + i + "\tEntID: " + ECS.State.Ents.IndicesToIds[i] + "\tComCount: " + ECS.State.Ents[i].ComCount);
				Console.WriteLine();
				Console.Write("\tComIDs:\t\t");
				for (int j = 0; j < ECS.State.Ents[i].ComCount; j++)
					Console.Write(ECS.State.Ents[i].ComIDs[j] + "\t\t\t");
				Console.WriteLine();
				Console.Write("\tComTypes:");
				for (int j = 0; j < ECS.State.Ents[i].ComCount; j++)
					Console.Write("\t" + ECS.State.Ents[i].ComTypes[j].Name);
				Console.WriteLine();
				Console.Write("\tDataIDs:\t");
				for (int j = 0; j < ECS.State.Ents[i].ComCount; j++)
					Console.Write(ECS.State.Ents[i].DataIDs[j] + "\t\t\t");
				Console.WriteLine();
			}
			Console.WriteLine();
			Console.WriteLine("=== Coms ===");
			for (int i = 0; i < ECS.State.Coms.Count; i++)
			{ 
				Console.WriteLine("ComIX: " + i + "\tComID: " + ECS.State.Coms.IndicesToIds[i] + "\tDataID: " + ECS.State.Coms[i].DataID + "\tComType: " + ECS.State.Coms[i].ComType.Name);
			}
			BExample0 ex0 = (BExample0)ECS.State.TypeToRef[typeof(BExample0)];
			ref RelaStructures.StructReArray<SExampleData> data = ref ex0.Data;
			Console.WriteLine();
			Console.WriteLine("=== Data BExample0 ===");
			for (int i = 0; i < data.Count; i++)
			{
				Console.WriteLine("DataIX: " + i + "\tDataID: " + data.IndicesToIds[i]);
			}
			BExample1 ex1 = (BExample1)ECS.State.TypeToRef[typeof(BExample1)];
			data = ref ex1.Data;
			Console.WriteLine();
			Console.WriteLine("=== Data BExample1 ===");
			for (int i = 0; i < data.Count; i++)
			{
				Console.WriteLine("DataIX: " + i + "\tDataID: " + data.IndicesToIds[i]);
			}
			for (int i = 0; i < ECS.State.Coms.Count; i++)
			{
				//Assert.IsTrue(ECS.State.Coms[i].ComType == ((i % 2 == 0) ? typeof(BExample0) : typeof (BExample1)));
				Assert.IsTrue(ECS.State.Coms[i].DataID == i / 2);
				//Assert.IsTrue(ECS.State.Coms[i].EntID == i / 2);
			}
		}
		[TestMethod]
		public void FindTypeInEnt()
		{
			InitAndAddBehaviors();
			int[] entIDs = ECS.AddEnts(5);
			int[] comIDs = ECS.AddComsToEnts(new Type[] { typeof(BExample0), typeof(BExample1) }, entIDs);
			// comID -> dataID
			ECS.FindTypeInEnt<BExample1>(comIDs[0], true, false);
			ECS.FindTypeInEnt<BExample1>(entIDs[0], false, true);
		}

		[TestMethod]
		public void RunSum()
		{
			AddEntWithCom();
			ECS.AddEntWithCom<BExample0>();
			BExample0 b = (BExample0)ECS.State.TypeToRef[typeof(BExample0)];
			Assert.IsTrue(b.Sum(ECS.State.Coms[0].DataID) == 15);
		}
	}
}