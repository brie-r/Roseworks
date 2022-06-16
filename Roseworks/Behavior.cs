using System.Collections;
using System.Collections.Generic;
namespace Roseworks
{
	public interface Behavior
	{
		//public KCController.E.GenericController Controller;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="comID"></param>
		/// <returns>DataID for newly initiated component</returns>
		public int InitCom(int comID, int entID);
		public void Init() { }
		public void Tick(float deltaTime) { }
		public void SetDefaultData(int dataID) { }
		public bool ShouldTick { get; set; }
		public System.Type[] Dependencies { get; set; }
		//public System.Type[] OptionalIntegrations = null;
	}
}