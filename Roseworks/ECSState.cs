// using System.Collections;
using System.Collections.Generic;
using RelaStructures;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Roseworks
{
	public class ECSState
	{
		public IInstantiator Inst;

		public int MaxEnts = byte.MaxValue;
		public int MaxComs = ushort.MaxValue;
		public int MaxComsPerEnt = byte.MaxValue;

		public System.Type[] MandatoryBehaviors = { };

		public List<System.Type> TypeList = new List<System.Type>();
		public Dictionary<System.Type, Behavior> TypeToRef =
			new Dictionary<System.Type, Behavior>();

		public StructReArray<SEnt> Ents;
		public StructReArray<SCom> Coms;

		public ECSState(IInstantiator inst, System.Type[] mandatoryBehaviors = null, int maxEnts = byte.MaxValue, int maxComs = ushort.MaxValue, int maxComsPerEnt = byte.MaxValue)
		{
			Inst = inst;
			MandatoryBehaviors = mandatoryBehaviors ?? new System.Type[] { };
			
			MaxEnts = maxEnts;
			MaxComs = maxComs;
			MaxComsPerEnt = maxComsPerEnt;
		}
		public void Init()
		{
            Ents = new StructReArray<SEnt>(MaxEnts, MaxEnts, SEnt.Clear, SEnt.Move, SEnt.Init);
            Coms = new StructReArray<SCom>(MaxComs, MaxComs, SCom.Clear, SCom.Move);
        }
	}
}
