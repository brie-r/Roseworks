using System.Collections;
using System.Collections.Generic;
using RelaStructures;
using log4net;
using log4net.Config;

namespace Roseworks
{
	public static class ECS
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ECS));
		private static bool DebugPrint = true;
		private static IMono Mono;

		public static int MaxComsPerEnt = 256;
		private static System.Type[] MandatoryBehaviors = { };
		private static object BehaviorObject;
		private static System.Text.StringBuilder DebugString =
			new System.Text.StringBuilder();
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
			DebugString.AppendLine("ADDING BEHAVIORS:");

			// check for behaviors
			if (behaviors != null && behaviors.Length > 0)
				AddBehaviorDependencies(behaviors);
			else
				if (DebugPrint) Log.Info("No behaviors to load");
			if (DebugPrint)
				Log.Info(DebugString);

		}
		public static void AddBehaviors(string[] behaviorNames)
		{
			// TODO: make sure not already added
			DebugString.AppendLine("ADDING BEHAVIORS:");

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
						Log.Error("BehaviorEntManager: type named " + behaviorNames[i] + " not found");
					behaviorsToAdd[i] = behavior;
					if (DebugPrint) Log.Info("Planning to add: " + behaviorsToAdd[i]);
				}
				AddBehaviorDependencies(behaviorsToAdd);
			}
			else
				if (DebugPrint) Log.Info("No behaviors to load");

			// Log.Info component debug
			if (DebugPrint)
				Log.Info(DebugString);
		}
		/// <summary>
		/// Recursive, depth first. Adds and initializes dependencies first, then dependents.
		/// </summary>
		/// <param name="dependencies"></param>
		private static void AddBehaviorDependencies(System.Type[] dependencies)
		{
			if (dependencies != null && dependencies.Length > 0)
			{
				if (DebugPrint) Log.Info("Adding " + dependencies.Length + " behavior dependencies");
				for (int i = 0; i < dependencies.Length; i++)
				{
					if (DebugPrint) Log.Info("Add: " + dependencies[i]);

					// if not instantiated
				
					if (TypeList.Contains(dependencies[i]) == false)
					{
						Behavior b = InstantiateBehavior(dependencies[i], i);

						if (b == null && DebugPrint) Log.Info("Behavior not properly instantiated: " + dependencies[i]);

						// set up dependencies, initialize
						if (b.Dependencies != null && b.Dependencies.Length > 0)
							AddBehaviorDependencies(b.Dependencies);
						bool hasInput = b is IInput;
						Log.Info(b.GetType() + " is" + (hasInput?" ":" NOT ") + "a subclass of IInput");
						if (hasInput)
						{
							if (DebugPrint) Log.Info(dependencies[i] + " has input");
							InputHandler.AddInputBehavior(b);
						}
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
			AddComToEnt<T>(entID);
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
			int entID = Ents.Request();
			if (comTypes != null)
				AddComsToEnt(comTypes, entID);
			return entID;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Count"></param>
		/// <param name="comTypes"></param>
		/// <param name="receivesInput"></param>
		/// <returns>EntIDs</returns>
		public static int[] AddEntsWithComs(int Count, System.Type[] comTypes = null)
		{
			if (comTypes == null)
				return null;

			int[] entIDs = new int[Count];
			for (int i = 0; i < Count; i++)
			{
				entIDs[i] = Ents.Request();
				AddComsToEnt(comTypes, entIDs[i]);
			}
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

			if (DebugPrint) Log.Info("AddComToEnt: " + comType);

			Behavior b = TypeToRef[comType];
			if (entID == -1)
				entID = Ents.Request();

			ref SEnt ent = ref Ents.AtId(entID);

			if (ent.ComCount == MaxComsPerEnt)
			{
				Log.Error(nameof(ECS) + "." + nameof(AddComToEnt) + "(): max coms per ent exceeded");
				return -1;
			}

			// set up dependencies, initialize
			if (b.Dependencies != null && b.Dependencies.Length > 0)
				AddComsToEnt(b.Dependencies);

			int comID = Coms.Request();
			Coms.AtId(comID).EntID = entID;
			int dataID = TypeToRef[comType].InitCom(comID, entID);
			Coms.AtId(comID).DataID = dataID;

			ent.ComCount++;
			int index = ent.ComCount - 1;
			ent.ComIDs[index] = comID;
			ent.ComTypes[index] = comType;
			ent.DataIDs[index] = dataID;


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
			Behavior b;

			if (DebugPrint) Log.Info("Adding " + comTypes.Length + " component dependencies");
			for (int i = 0; i < comTypes.Length; i++)
			{
				b = TypeToRef[comTypes[i]];
				if (DebugPrint) Log.Info("Add: " + comTypes[i]);

				if (entID == -1)
					entID = Ents.Request();
				// set up dependencies, initialize
				if (b.Dependencies != null && b.Dependencies.Length > 0)
					AddComsToEnt(b.Dependencies, entID);

				int comID = Coms.Request();
				Coms.AtId(comID).EntID = entID;
				int dataID = b.InitCom(comID, entID);
				Coms.AtId(comID).DataID = dataID;

				ref SEnt ent = ref Ents.AtId(entID);
				ent.ComCount++;
				int index = ent.ComCount - 1;
				ent.ComIDs[index] = comID;
				ent.ComTypes[index] = comTypes[i];
				ent.DataIDs[index] = dataID;
			}
			return comIDs;
		}
		public static int[] AddComToEnts(System.Type comType, int[] entIDs = null)
		{
			if (comType == null || entIDs == null || entIDs.Length == 0)
				return null;

			int[] comIDs = new int[entIDs.Length];
			Behavior b = TypeToRef[comType];

			if (DebugPrint) Log.Info("Add: " + comType);

			for (int i = 0; i < entIDs.Length; i++)
			{
				if (entIDs[i] == -1)
					entIDs[i] = Ents.Request();
				// set up dependencies, initialize
				if (b.Dependencies != null && b.Dependencies.Length > 0)
					AddComsToEnt(b.Dependencies);
				// create and initialize new com
				comIDs[i] = Coms.Request();
				Coms.AtId(comIDs[i]).EntID = entIDs[i];
				int dataID = b.InitCom(comIDs[i], entIDs[i]);
				Coms.AtId(comIDs[i]).DataID = dataID;

				ref SEnt ent = ref Ents.AtId(entIDs[i]);
				ent.ComCount++;
				int index = ent.ComCount - 1;
				ent.ComIDs[index] = comIDs[i];
				ent.ComTypes[index] = comType;
				ent.DataIDs[index] = dataID;
			}
			return comIDs;
		}
		public static int[] AddComsToEnts(System.Type[] comTypes, int[] entIDs = null)
		{
			if (comTypes == null || entIDs == null || entIDs.Length == 0)
				return null;

			int[] outputComIDs = new int[comTypes.Length * entIDs.Length];
			int outputIndex;
			Behavior b;
			for (int comI = 0; comI < comTypes.Length; comI++)
			{
				b = TypeToRef[comTypes[comI]];
				if (DebugPrint) Log.Info("Add: " + comTypes[comI]);

				for (int entI = 0; entI < entIDs.Length; entI++)
				{
					outputIndex = comI * entIDs.Length + entI;
					if (entIDs[entI] == -1)
						entIDs[entI] = Ents.Request();
					// set up dependencies, initialize
					if (b.Dependencies != null && b.Dependencies.Length > 0)
						AddComsToEnt(b.Dependencies);
					// create and initialize new com
					outputComIDs[outputIndex] = Coms.Request();
					Coms.AtId(outputComIDs[outputIndex]).EntID = entIDs[entI];
					int dataID = b.InitCom(outputComIDs[outputIndex], entIDs[entI]);
					Coms.AtId(outputComIDs[outputIndex]).DataID = dataID;

					ref SEnt ent = ref Ents.AtId(entIDs[entI]);
					ent.ComCount++;
					int index = ent.ComCount - 1;
					ent.ComIDs[index] = outputComIDs[outputIndex];
					ent.ComTypes[index] = comTypes[comI];
					ent.DataIDs[index] = dataID;
				}
			}
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
		public static void InitScene(object e, IMono mono)
		{
			BehaviorObject = e;
			Mono = mono;

			for (int i = 0; i < MandatoryBehaviors.Length; i++)
				AddBehaviors(MandatoryBehaviors);

			// behaviors, ents, coms
			object[] ent = Mono.GetEditorEnts();
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
			if (behavior.IsSubclassOf(typeof(Behavior)) == false && DebugPrint)
				Log.Error("[EntManager.InstantiateBehavior()] " + behavior + " is not a subclass of Behavior");

			Behavior b = Mono.Instantiate(BehaviorObject, behavior);
			TypeList.Add(behavior);
			TypeToRef[behavior] = b;
			return b;
		}
	}
	public interface IMono
	{
		public Behavior Instantiate(object parent, System.Type behavior);
		public object[] GetEditorEnts();
	}
}
