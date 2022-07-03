using System;
using System.Collections.Generic;
using System.Text;

namespace RoseworksTest
{
	public static class TriggerSlots
	{
		public enum ESlots { Use, Select, EndTurn, Zoom }
		public static int Count = Enum.GetValues(typeof(ESlots)).Length - 1;
	}
}
