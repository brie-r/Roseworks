// using System.Collections;
using System.Collections.Generic;
using RelaStructures;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Roseworks
{
	public static class ECS
	{
		public static ECSState State;
		public static bool DebugPrint = true;

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void AddBehavior<T>()
		{
			AddBehaviorDependencies(new System.Type[] { typeof(T) });
		}
		public static void AddBehaviors(System.Type[] behaviors)
		{
			if (behaviors != null && behaviors.Length > 0)
				AddBehaviorDependencies(behaviors);
		}
		public static void AddBehaviors(string[] behaviorNames)
		{
			// check for behaviors
			if (behaviorNames == null || behaviorNames.Length <= 0)
				return;
			System.Type behavior;
			// add all behaviors in list
			System.Type[] behaviorsToAdd = new System.Type[behaviorNames.Length];
			for (int i = 0; i < behaviorsToAdd.Length; i++)
			{
				behavior = System.Type.GetType(behaviorNames[i], true);
				behaviorsToAdd[i] = behavior;
			}
			AddBehaviorDependencies(behaviorsToAdd);
		}
		/// <summary>
		/// Recursive, depth first. Adds and initializes dependencies first, then dependents.
		/// </summary>
		/// <param name="dependencies"></param>
		private static void AddBehaviorDependencies(System.Type[] dependencies)
		{

			if (dependencies == null || dependencies.Length <= 0)
				return;
			for (int i = 0; i < dependencies.Length; i++)
			{					
				if (State.TypeList.Contains(dependencies[i]) == false)
				{
					Behavior b = InstantiateBehavior(dependencies[i], i);

					// set up dependencies, initialize
					if (b.Dependencies != null && b.Dependencies.Length > 0)
						AddBehaviorDependencies(b.Dependencies);
					if (b is IInput)
						Input.AddInputBehavior(b);
					b.Init();
				}
			}
		}
		/// <summary>Register an entity with no components.</summary>
		/// <returns>Entity ID</returns>
		public static int AddEnt()
		{
			int entID = State.Ents.Request();
			return entID;
		}
		/// <summary>Register multiple entities with no components.</summary>
		/// <param name="count">Number of entities to register</param>
		/// <returns>Entity IDs of all entities registered</returns>
		public static int[] AddEnts(int count)
		{
			if (count <= 0)
				return new int[0];
			int[] entIDs = new int[count];
			for (int i = 0; i < count; i++)
				entIDs[i] = State.Ents.Request();
			return entIDs;
		}
		/// <summary>
		/// Register a single entity with a single component.
		/// </summary>
		/// <param name="comType">Type of component to register</param>
		/// <returns>Entity ID</returns>
		public static int AddEntWithCom(System.Type comType)
		{
			int entID = State.Ents.Request();
			if (comType != null)
				AddComToEnt(comType, entID);
			return entID;
		}
		/// <summary>
		/// Register a single entity with a single component.
		/// </summary>
		/// <typeparam name="T">Type of component to register</typeparam>
		/// <returns>Entity ID
		/// <br></br>
		/// Returns -1 if entity limit reached</returns>
		public static int AddEntWithCom<T>()
		{
			int entID = State.Ents.Request();
			AddComToEnt(typeof(T), entID);
			return entID;
		}

		/// <summary>
		/// Register a single entity with multiple components.
		/// </summary>
		/// <param name="comTypes">Type of all components to register</param>
		/// <returns>Entity ID
		/// <br></br>
		/// Returns -1 if: component types invalid, entity limit reached</returns>
		
		public static int AddEntWithComs(System.Type[] comTypes = null)
		{
			if (comTypes == null || comTypes.Length <= 0)
				return -1;
			int entID = State.Ents.Request();
			if (entID < 0)
				return -1;
			AddComsToEnt(comTypes, entID);
			return entID;
		}
		/// <summary>
		/// Register multiple entities with a single component each.
		/// </summary>
		/// <param name="count">Number of entities to register</param>
		/// <param name="comType">Type of component to register</param>
		/// <returns>Entity IDs of all entities registered
		/// <br></br>
		/// Returns null if: component type invalid
		/// <br></br>
		/// Return contains negative IDs if: entity limit reached</returns>
		public static int[] AddEntsWithCom(int count, System.Type comType)
		{
			if (count == 0)
				return null;
			int[] entIDs = AddEnts(count);
			AddComToEnts(comType, entIDs);
			return entIDs;
		}
		/// <summary>
		/// Register multiple entities with multiple components each.
		/// </summary>
		/// <param name="count">Number of entities to register</param>
		/// <param name="comTypes">Type of components to register</param>
		/// <returns>Entity IDs of all entities registered
		/// <br></br>
		/// Returns null if component types invalid
		/// <br></br>
		/// Contains -1 for each requested entity exceeding entity limit</returns>
		public static int[] AddEntsWithComs(int count, System.Type[] comTypes = null)
		{
			if (count == 0)
				return null;
			int[] entIDs = AddEnts(count);
			AddComsToEnts(comTypes, entIDs);
			return entIDs;
		}
		/// <summary>
		/// Register component and add to existing entity.
		/// </summary>
		/// <typeparam name="T">Type of component to register</typeparam>
		/// <param name="comType">Type of component to register</param>
		/// <param name="comTypes">Types of components to register</param>
		/// <param name="entID">ID of entity to add component to</param>
		/// <returns>Component ID of component registered
		/// <br></br>
		/// -1 if: entity not found, component limit reached, component per entity limit reached</returns>
		public static int AddComToEnt(System.Type comType, int entID)
		{
			if (comType == null || entID < 0)
				return -1;

			Behavior b = State.TypeToRef[comType];

			ref SEnt ent = ref State.Ents.AtId(entID);

			if (ent.ComCount == State.MaxComsPerEnt)
				return -1;

			// set up dependencies, initialize
			if (b.Dependencies != null && b.Dependencies.Length > 0)
				AddComsToEnt(b.Dependencies, entID);

			int comID = State.Coms.Request();
			int dataID = State.TypeToRef[comType].InitCom(comID, entID);
			State.Coms.AtId(comID).EntID = entID;
			State.Coms.AtId(comID).DataID = dataID;
			State.Coms.AtId(comID).ComType = comType;

			ent.ComCount++;
			int index = ent.ComCount - 1;
			ent.ComIDs[index] = comID;
			ent.ComTypes[index] = comType;
			ent.DataIDs[index] = dataID;

			if (typeof(IInput).IsAssignableFrom(comType))
				Input.AddInputCom(comType, 2);

			return comID;
		}
		/// <include file='ECSDocs.xml' path='/root/addCom/*'/>
		/// <include file='ECSDocs.xml' path='/root/ECSParams/*'/>
		public static int AddComToEnt<T>(int entID)
		{
			return AddComToEnt(typeof(T), entID);
		}
		public static int[] AddComsToEnt(System.Type[] comTypes, int entID = -1)
		{
			if (comTypes == null || comTypes.Length <= 0)
				return null;
			int[] comIDs = new int[comTypes.Length];
			for (int i = 0; i < comTypes.Length; i++)
				comIDs[i] = AddComToEnt(comTypes[i], entID);
			return comIDs;
		}
		/// <include file='ECSDocs.xml' path='/root/addCom/*'/>
		/// <include file='ECSDocs.xml' path='/root/ECSParams/*'/>
		public static int[] AddComToEnts(System.Type comType, int[] entIDs = null)
		{
			if (comType == null || entIDs == null || entIDs.Length == 0)
				return null;
			int[] comIDs = new int[entIDs.Length];
			for (int i = 0; i < entIDs.Length; i++)
				comIDs[i] = AddComToEnt(comType, entIDs[i]);
			return comIDs;
		}
		/// <include file='ECSDocs.xml' path='/root/addCom/*'/>
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
			int entID = inComOrEntID ? State.Coms.AtId(inID).EntID : inID;
			// find com of matching type in ent
			ref SEnt ent = ref State.Ents.AtId(entID);
			for (int i = 0; i < ent.ComCount; i++)
			{
				if (ent.ComTypes[i] == typeof(T))
					return outComOrDataID ? ent.ComIDs[i] : ent.DataIDs[i];
			}
			return -1;
		}
		public static void InitScene(IInstantiator inst, System.Type[] mandatoryBehaviors = null, int maxEnts = byte.MaxValue, int maxComs = ushort.MaxValue, int maxComsPerEnt = byte.MaxValue)
		{
			State = new ECSState(inst, mandatoryBehaviors, maxEnts, maxComs, maxComsPerEnt);
			State.Init();
			for (int i = 0; i < State.MandatoryBehaviors.Length; i++)
				AddBehaviors(State.MandatoryBehaviors);

			// behaviors, ents, coms
			object[] ent = State.Inst.GetEditorEnts();
			for (int i = 0; i < ent.Length; i++)
				AddBehaviors(((IEditorEnt) ent[i]).LoadBehaviors);
		}
		public static System.Type ComIDToType(int comID)
		{
			return State.Coms.AtId(comID).ComType;
		}
		public static int[] TypeToComIDs<T>()
		{
			List<int> output = new List<int>();
			for (int comIndex = 0; comIndex < State.Coms.Count; comIndex++)
			{
				if (State.Coms[comIndex].ComType == typeof(T))
					output.Add(State.Coms.IndicesToIds[comIndex]);
			}
			return output.ToArray();
		}
		public static Behavior InstantiateBehavior(System.Type behavior, int behaviorIndex)
		{
			if (typeof(Behavior).IsAssignableFrom(behavior) == false && DebugPrint)
				Logger.WriteLine("[EntManager.InstantiateBehavior()] " + behavior + " does not implement Behavior");

			Behavior b = State.Inst.Instantiate(State.Inst, behavior);
			State.TypeList.Add(behavior);
			State.TypeToRef[behavior] = b;
			return b;
		}
		public static ref SCom ComAtId(int id)
		{
			return ref State.Coms.AtId(id);
		}
		public static ref SEnt EntAtId(int id)
		{
			return ref State.Ents.AtId(id);
		}
	}
}
