using System;
using System.Collections.Generic;
using System.Text;

namespace Roseworks
{
	public interface IInstantiator
	{
		public Behavior Instantiate(object parent, Type behavior);
		public object[] GetEditorEnts();
	}
}
