// using System.Collections;
using System.Collections.Generic;
using RelaStructures;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Roseworks
{
	public static class ECS
	{
		public static bool DebugPrint = true;
		private static IInstantiator Inst;

		public static int MaxComsPerEnt = 256;
		private static System.Type[] MandatoryBehaviors = { };

		public static List<System.Type> TypeList = new List<System.Type>();
		public static Dictionary<System.Type, Behavior> TypeToRef =
			new Dictionary<System.Type, Behavior>();

		public static StructReArray<SEnt> Ents = new StructReArray<SEnt>(byte.MaxValue, byte.MaxValue, SEnt.Clear, SEnt.Move, SEnt.Init);
		public static StructReArray<SCom> Coms = new StructReArray<SCom>(ushort.MaxValue, ushort.MaxValue, SCom.Clear, SCom.Move);

		public static void AddBehavior<T>()
		{
			AddBehaviorDependencies(new System.Type[] { typeof(T) });
		}
		public static void AddBehaviors(System.Type[] behaviors)
		{
			// TODO: make sure not already added
			Logger.WriteLine("Adding Behaviors...");
			// check for behaviors
			if (behaviors != null && behaviors.Length > 0)
				AddBehaviorDependencies(behaviors);
			else
				if (DebugPrint) Logger.WriteLine("No behaviors to load");
		}
		public static void AddBehaviors(string[] behaviorNames)
		{
			// TODO: make sure not already added
			Logger.WriteLine("Adding Behaviors...");

			// check for behaviors
			if (behaviorNames != null && behaviorNames.Length > 0)
			{
				System.Type behavior;
				// add all behaviors in list
				System.Type[] behaviorsToAdd = new System.Type[behaviorNames.Length];
				for (int i = 0; i < behaviorsToAdd.Length; i++)
				{
					behavior = System.Type.GetType(behaviorNames[i]);
					if (behavior == null)
						Logger.WriteLine("BehaviorEntManager: type named " + behaviorNames[i] + " not found");
					behaviorsToAdd[i] = behavior;
					if (DebugPrint) Logger.WriteLine("Planning to add: " + behaviorsToAdd[i]);
				}
				AddBehaviorDependencies(behaviorsToAdd);
			}
			else
				if (DebugPrint) Logger.WriteLine("No behaviors to load");
		}
		/// <summary>
		/// Recursive, depth first. Adds and initializes dependencies first, then dependents.
		/// </summary>
		/// <param name="dependencies"></param>
		private static void AddBehaviorDependencies(System.Type[] dependencies)
		{
			if (dependencies != null && dependencies.Length > 0)
			{
				if (DebugPrint) Logger.WriteLine("Adding " + dependencies.Length + " behaviors");
				for (int i = 0; i < dependencies.Length; i++)
				{
					if (DebugPrint) Logger.WriteLine("Add: " + dependencies[i]);
					
					if (TypeList.Contains(dependencies[i]) == false)
					{
						Behavior b = InstantiateBehavior(dependencies[i], i);

						if (b == null && DebugPrint) Logger.WriteLine("Behavior not properly instantiated: " + dependencies[i]);

						// set up dependencies, initialize
						if (b.Dependencies != null && b.Dependencies.Length > 0)
							AddBehaviorDependencies(b.Dependencies);
						bool hasInput = b is IInput;
						Logger.WriteLine(b.GetType() + " does" + (hasInput?" ":" NOT ") + "receive input");
						if (hasInput)
							Input.AddInputBehavior(b);
						b.Init();
					}
				}
			}
		}
		public static int AddEnt()
		{
			int entID = Ents.Request();
			return entID;
		}
		public static int[] AddEnts(int count)
		{
			int[] entIDs = new int[count];
			for (int i = 0; i < count; i++)
			{
				entIDs[i] = Ents.Request();
			}
			return entIDs;
		}
		public static int AddEntWithCom(System.Type comType)
		{
			int entID = Ents.Request();
			if (comType != null)
				AddComToEnt(comType, entID);
			return entID;
		}
		public static int AddEntWithCom<T>()
		{
			int entID = Ents.Request();
			AddComToEnt(typeof(T), entID);
			return entID;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="comTypes"></param>
		/// <param name="receivesInput"></param>
		/// <returns>EntID</returns>
		public static int AddEntWithComs(System.Type[] comTypes = null)
		{
			if (comTypes == null || comTypes.Length == 0)
				return -1;
			int entID = Ents.Request();
			AddComsToEnt(comTypes, entID);
			return entID;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <param name="comType"></param>
		/// <param name="receivesInput"></param>
		/// <returns>EntIDs</returns>
		public static int[] AddEntsWithCom(int count, System.Type comType)
		{
			if (count == 0)
				return null;
			int[] entIDs = AddEnts(count);
			AddComToEnts(comType, entIDs);
			return entIDs;
		}
	public static int[] AddEntsWithComs(int count, System.Type[] comTypes = null)
		{
			if (count == 0 || comTypes == null)
				return null;
			int[] entIDs = AddEnts(count);
			AddComsToEnts(comTypes, entIDs);
			return entIDs;
		}
		public static int AddComToEnt<T>(int entID = -1)
		{
			return AddComToEnt(typeof(T), entID);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="comType"></param>
		/// <param name="entID"></param>
		/// <returns>ComID</returns>
		public static int AddComToEnt(System.Type comType, int entID = -1)
		{
			if (comType == null)
				return -1;

			Behavior b = TypeToRef[comType];
			if (entID == -1)
				entID = Ents.Request();

			ref SEnt ent = ref Ents.AtId(entID);

			if (ent.ComCount == MaxComsPerEnt)
			{
				Logger.WriteLine(nameof(ECS) + "." + nameof(AddComToEnt) + "(): max coms per ent exceeded");
				return -1;
			}

			// set up dependencies, initialize
			if (b.Dependencies != null && b.Dependencies.Length > 0)
			{
				if (DebugPrint) Logger.WriteLine("Calling AddComsToEnt from AddComToEnt (dependencies)");
				AddComsToEnt(b.Dependencies);
			}

			int comID = Coms.Request();
			int dataID = TypeToRef[comType].InitCom(comID, entID);
			Coms.AtId(comID).EntID = entID;
			Coms.AtId(comID).DataID = dataID;
			Coms.AtId(comID).ComType = comType;

			ent.ComCount++;
			int index = ent.ComCount - 1;
			ent.ComIDs[index] = comID;
			ent.ComTypes[index] = comType;
			ent.DataIDs[index] = dataID;

			if (typeof(IInput).IsAssignableFrom(comType))
				Input.AddInputCom(comType, 2);

			return comID;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="comTypes"></param>
		/// <param name="entID"></param>
		/// <returns>Array of ComIDs</returns>
		public static int[] AddComsToEnt(System.Type[] comTypes, int entID = -1)
		{
			if (comTypes == null || comTypes.Length <= 0)
				return null;
			int[] comIDs = new int[comTypes.Length];
			for (int i = 0; i < comTypes.Length; i++)
				comIDs[i] = AddComToEnt(comTypes[i], entID);
			return comIDs;
		}
		public static int[] AddComToEnts(System.Type comType, int[] entIDs = null)
		{
			if (comType == null || entIDs == null || entIDs.Length == 0)
				return null;
			int[] comIDs = new int[entIDs.Length];
			for (int i = 0; i < entIDs.Length; i++)
				comIDs[i] = AddComToEnt(comType, entIDs[i]);
			return comIDs;
		}
		public static int[] AddComsToEnts(System.Type[] comTypes, int[] entIDs = null)
		{
			if (comTypes == null || entIDs == null || entIDs.Length == 0)
				return null;

			int[] outputComIDs = new int[comTypes.Length * entIDs.Length];
			for (int i = 0; i < entIDs.Length; i++)
				System.Array.Copy(AddComsToEnt(comTypes, entIDs[i]), 0, outputComIDs, i * comTypes.Length, comTypes.Length);
			return outputComIDs;
		}
		/// <summary>
		/// Searches for an ID in the same ent by type
		/// (ComID or EntID) + Type -> (ComID or DataID) of Type in same ent
		/// </summary>
		/// <param name="inID">An EntID, or a ComID that lets us find an EntID.</param>
		/// <param name="inComOrEntID">Is inID a ComID or EntID?</param>
		/// <param name="inType">The type to search for in our ent</param>
		/// <param name="outComOrDataID">Is our return value a ComID or DataID?</param>
		/// <returns>Returns either a ComID or DataID for the given type in the given ent</returns>
		public static int FindTypeInEnt<T>(int inID, bool inComOrEntID, bool outComOrDataID)
		{
			int entID = inComOrEntID ? Coms.AtId(inID).EntID : inID;
			// find com of matching type in ent
			ref SEnt ent = ref Ents.AtId(entID);
			for (int i = 0; i < ent.ComCount; i++)
			{
				if (ent.ComTypes[i] == typeof(T))
					return outComOrDataID ? ent.ComIDs[i] : ent.DataIDs[i];
			}
			return -1;
		}
		public static void InitScene(IInstantiator inst)
		{
			Inst = inst;
			for (int i = 0; i < MandatoryBehaviors.Length; i++)
				AddBehaviors(MandatoryBehaviors);

			// behaviors, ents, coms
			object[] ent = Inst.GetEditorEnts();
			for (int i = 0; i < ent.Length; i++)
				AddBehaviors(((IEditorEnt) ent[i]).LoadBehaviors);
		}
		public static System.Type ComIDToType(int comID)
		{
			return Coms.AtId(comID).ComType;
		}
		public static int[] TypeToComIDs<T>()
		{
			List<int> output = new List<int>();
			for (int comIndex = 0; comIndex < Coms.Count; comIndex++)
			{
				if (Coms[comIndex].ComType == typeof(T))
					output.Add(Coms.IndicesToIds[comIndex]);
			}
			return output.ToArray();
		}
		public static Behavior InstantiateBehavior(System.Type behavior, int behaviorIndex)
		{
			if (typeof(Behavior).IsAssignableFrom(behavior) == false && DebugPrint)
				Logger.WriteLine("[EntManager.InstantiateBehavior()] " + behavior + " does not implement Behavior");

			Behavior b = Inst.Instantiate(Inst, behavior);
			TypeList.Add(behavior);
			TypeToRef[behavior] = b;
			return b;
		}
	}
	public interface IInstantiator
	{
		public Behavior Instantiate(object parent, System.Type behavior);
		public object[] GetEditorEnts();
	}
}
