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
		public void InitAndAddBehavior()
		{
			TestUtil.Init();
			ECS.AddBehavior<BExample0>();
			Assert.IsTrue(ECS.TypeList.Count == 1);
			Assert.IsTrue(ECS.TypeToRef[typeof(BExample0)] != null);
			Assert.IsTrue(ECS.TypeToComIDs<BExample0>().Length == 0);
		}
		[TestMethod]
		public void InitAndAddBehaviorDependent()
		{
			TestUtil.Init();
			ECS.AddBehavior<BDependent>();
			Assert.IsTrue(ECS.TypeList.Count == 2);
			Assert.IsTrue(ECS.TypeToRef[typeof(BDependent)] != null);
			Assert.IsTrue(ECS.TypeToComIDs<BDependent>().Length == 0);
			Assert.IsTrue(ECS.TypeToRef[typeof(BDependency)] != null);
			Assert.IsTrue(ECS.TypeToComIDs<BDependency>().Length == 0);
		}
		public void InitAndAddBehaviors()
		{
			TestUtil.Init();
			ECS.AddBehaviors(new Type[]{typeof(BExample0), typeof(BExample1)});
			Assert.IsTrue(ECS.TypeList.Count == 2);
			Assert.IsTrue(ECS.TypeToRef[typeof(BExample0)] != null);
			Assert.IsTrue(ECS.TypeToRef[typeof(BExample1)] != null);
			Assert.IsTrue(ECS.TypeToComIDs<BExample0>().Length == 0);
			Assert.IsTrue(ECS.TypeToComIDs<BExample1>().Length == 0);
		}
		[TestMethod]
		public void AddEntWithCom()
		{
			InitAndAddBehavior();
			ECS.AddEntWithCom<BExample0>();
			Assert.IsTrue(ECS.Coms.Count == 1);
			Assert.IsTrue(ECS.Coms[0].ComType == typeof(BExample0));
		}
		[TestMethod]
		public void AddEntWithComDependent()
		{
			InitAndAddBehaviorDependent();
			ECS.AddEntWithCom<BDependent>();
			Console.WriteLine("Assert");
			Assert.IsTrue(ECS.Coms.Count == 2);
			Console.WriteLine("Assert");
			Assert.IsTrue(ECS.Coms[0].ComType == typeof(BDependency));
			Console.WriteLine("Assert");
			Assert.IsTrue(ECS.Coms[1].ComType == typeof(BDependent));
		}
		[TestMethod]
		public void AddEntWithComs()
		{
			InitAndAddBehaviors();
			ECS.AddEntWithComs(new Type[] { typeof(BExample0), typeof(BExample1) });
			Assert.IsTrue(ECS.Coms.Count == 2);
			for (int i = 0; i < ECS.Coms.Count; i++)
			{
				Console.WriteLine("ComIX: " + i + "\tComID: " + ECS.Coms.IndicesToIds[i] + "\tComType: " + ECS.Coms[i].ComType.Name + "\tDataID: " + ECS.Coms[i].DataID);
			}
			for (int i = 0; i < ECS.Coms.Count; i++)
			{
				Assert.IsTrue(ECS.Coms[i].ComType == ((i % 2 == 0) ? typeof(BExample0) : typeof(BExample1)));
				Assert.IsTrue(ECS.Coms.IndicesToIds[i] == i);
				Assert.IsTrue(ECS.Coms[i].DataID == i/2);
			}
		}
		[TestMethod]
		public void AddEntsWithCom()
		{
			InitAndAddBehavior();
			ECS.AddEntsWithCom(10, typeof(BExample0));
			Assert.IsTrue(ECS.Coms.Count == 10);
			for (int i = 0; i < ECS.Coms.Count; i++)
				Assert.IsTrue(ECS.Coms[i].ComType == typeof(BExample0));
		}
		[TestMethod]
		public void AddEntsWithComs()
		{
			InitAndAddBehaviors();
			ECS.AddEntsWithComs(10, new Type[] { typeof(BExample0), typeof(BExample1) });
			Assert.IsTrue(ECS.Ents.Count == 10);
			Assert.IsTrue(ECS.Coms.Count == 20);
			Console.WriteLine("=== Ents ===");
			for (int i = 0; i < ECS.Ents.Count; i++)
			{
				Console.Write("EntIX: " + i + "\tEntID: " + ECS.Ents.IndicesToIds[i] + "\tComCount: " + ECS.Ents[i].ComCount);
				Console.WriteLine();
				Console.Write("\tComIDs:\t\t");
				for (int j = 0; j < ECS.Ents[i].ComCount; j++)
					Console.Write(ECS.Ents[i].ComIDs[j] + "\t\t\t");
				Console.WriteLine();
				Console.Write("\tComTypes:");
				for (int j = 0; j < ECS.Ents[i].ComCount; j++)
					Console.Write("\t" + ECS.Ents[i].ComTypes[j].Name);
				Console.WriteLine();
				Console.Write("\tDataIDs:\t");
				for (int j = 0; j < ECS.Ents[i].ComCount; j++)
					Console.Write(ECS.Ents[i].DataIDs[j] + "\t\t\t");
				Console.WriteLine();
			}
			Console.WriteLine();
			Console.WriteLine("=== Coms ===");
			for (int i = 0; i < ECS.Coms.Count; i++)
			{ 
				Console.WriteLine("ComIX: " + i + "\tComID: " + ECS.Coms.IndicesToIds[i] + "\tDataID: " + ECS.Coms[i].DataID + "\tComType: " + ECS.Coms[i].ComType.Name);
			}
			BExample0 ex0 = (BExample0)ECS.TypeToRef[typeof(BExample0)];
			ref RelaStructures.StructReArray<SExampleData> data = ref ex0.Data;
			Console.WriteLine();
			Console.WriteLine("=== Data BExample0 ===");
			for (int i = 0; i < data.Count; i++)
			{
				Console.WriteLine("DataIX: " + i + "\tDataID: " + data.IndicesToIds[i]);
			}
			BExample1 ex1 = (BExample1)ECS.TypeToRef[typeof(BExample1)];
			data = ref ex1.Data;
			Console.WriteLine();
			Console.WriteLine("=== Data BExample1 ===");
			for (int i = 0; i < data.Count; i++)
			{
				Console.WriteLine("DataIX: " + i + "\tDataID: " + data.IndicesToIds[i]);
			}
			for (int i = 0; i < ECS.Coms.Count; i++)
			{
				//Assert.IsTrue(ECS.Coms[i].ComType == ((i % 2 == 0) ? typeof(BExample0) : typeof (BExample1)));
				Assert.IsTrue(ECS.Coms[i].DataID == i / 2);
				//Assert.IsTrue(ECS.Coms[i].EntID == i / 2);
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
			BExample0 b = (BExample0)ECS.TypeToRef[typeof(BExample0)];
			Assert.IsTrue(b.Sum(ECS.Coms[0].DataID) == 15);
		}
	}
}