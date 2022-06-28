using System.Collections;
using System.Collections.Generic;
namespace Roseworks
{
	public interface Behavior
	{
		/// <summary>
		/// Runs once per behavior, when instantiated.
		/// </summary>
		public void Init() { }
		/// <summary>
		/// Runs each time a component is added to a behavior.
		/// </summary>
		/// <param name="comID"></param>
		/// <returns>DataID for newly added component</returns>
		public int InitCom(int comID, int entID);
		public bool ShouldTick { get; set; }
		public void Tick(float deltaTime) { }
		public void SetDefaultData(int dataID) { }
		public System.Type[] Dependencies { get; set; }
	}
}