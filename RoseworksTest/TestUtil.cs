using System;
using System.Collections.Generic;
using System.Text;
using Roseworks;

namespace RoseworksTest
{
	public static class TestUtil
	{
		public static void Init()
		{
			InstSimulator inst = new InstSimulator();
			ECS.InitScene(inst);
			Input.Init();
		}
	}
}
