using System;
using System.Collections;
using System.Collections.Generic;
using RelaStructures;
using RoseLog;

namespace Roseworks
{
	public static class FSM
	{
		public static int DefaultState = default;
		public const int InvalidState = -1;

		// Rules are associated with states by ComType. All states have the same set of rules
		public static StructReArray<SState> States = new StructReArray<SState>(256, 1024, SState.Clear, SState.Move);
		public static StructReArray<SRule> Rules = new StructReArray<SRule>(256, 1024, SRule.Clear, SRule.Move);
		public static StructReArray<SJoin> Joins = new StructReArray<SJoin>(65536, 1048576, SJoin.Clear, SJoin.Move);

		[Flags]
		public enum EActive : int
		{
			None = 0,
			From = 1 << 0,
			FromTo = 1 << 1,
			EndCndFrom = 1 << 2,
			EndCndFromTo = 1 << 3,
			EndCbFromTo = 1 << 4,
		}

		#region Callback Preallocation (WIP)
		static List<Action<int, int>> Callbacks;
		public enum ECallback : int { };
		static List<Func<bool>> CallbackCnds;
		public enum ECallbackCnds : int { };
		#endregion

		static bool DebugPrint = true;

		/// <returns>DataID of newly added FSM</returns>
		public static int AddState(Type comType, int entID, float time)
		{
			int stateID = States.Request();
			States.AtId(stateID).ComType = comType;
			int comID = ECS.AddComToEnt(comType, entID);
			ECS.Coms.AtId(comID).DataID = stateID;
			ChangeState(time, stateID, DefaultState);



			return stateID;
		}
		public static (int ruleID, int timerID) AddRule (SRule ruleData, float time)
		{
			int ruleID = Rules.Request();
			ref SRule rule = ref Rules.AtId(ruleID);
			SRule.Move(ref ruleData, ref rule);
			ref SState state = ref States[0];
			int joinID;
			ref SJoin join = ref Joins[0];
			int timerID = -1;

			if (rule.Duration > 0 && rule.To >= 0)
			{
				InitStateTimer(time, state.ComID, ruleID, out timerID);
			}

			// add joins
			for (int i = 0; i < States.Count; i++)
			{
				state = ref States[i];
				if (state.ComType == rule.ComType)
				{
					joinID = Joins.Request();
					join = Joins.AtId(joinID);
					join.ComID = state.ComID;
					join.RuleID = ruleID;
					join.StateID = States.IndicesToIds[i];
					join.TimerID = timerID;
				}
			}
			return (ruleID, timerID);
		}
		private static bool ExitChangeState = false;
		// TODO: switch to to transition instead?
		public static void ChangeState(float time, int stateID, int to, int timerID = -1, bool ignoreSame = false)
		{
			Log.Text.Clear();
			ref SState state = ref States.AtId(stateID);

			if (Equals(state.State, to) && ignoreSame == true)
				return;

			ExitChangeState = false;

			UpdateActive(stateID);
			TryCallbacks(state.ComID, state.State, to);

			// if changestate runs again during this, exit
			if (ExitChangeState == true)
			{
				if (DebugPrint) Log.AppendLineAndWrite("Exiting ChangeState early. State was changed by callback");
				return;
			};
			Log.AppendLineAndWrite(typeof(int).FullName + ": " + state.State + " -> " + to);
			Log.Text.Replace("FSM", "");
			Log.Text.Replace("+", ".");

			if (state.State == to && timerID >= 0)
			{
				Log.AppendAndWrite("Loop timer " + timerID + ": " + DFormatTime(Timer.EndTime(timerID)) + " -> ");

				Timer.Loop(timerID);
				Log.AppendAndWrite(DFormatTime(Timer.EndTime(timerID)) + ". ");
				Timer.CancelByComID(comID: state.ComID, ignoreID: timerID);
			}
			else
			{
				Timer.CancelByComID(state.ComID);
			}

			//UpdateActive();

			// Add timers
			for (int joinIndex = 0; joinIndex < Joins.Count; joinIndex++)
			{
				ref SJoin join = ref Joins[joinIndex];
				ref SRule rule = ref Rules.AtId(join.RuleID);

				if (join.TimerID < 0 && StateValid(rule.To) && join.ActiveChecks.HasFlag(EActive.From))
				{
					int tempTo = rule.To;
					void Callback(int timerID)
					{
						int lambdaWorkaroundDoNotRemove = tempTo;
						ChangeState(time, stateID: stateID, lambdaWorkaroundDoNotRemove, timerID: timerID);
					}

					Timer.Add(time, outTimerID: out join.TimerID, comID: join.ComID, duration: rule.Duration, callback: Callback, autoCancel: false);
				}
			}

			if (DebugPrint && Log.Text.Length > 0)
				Log.AppendLineAndWrite(Log.Text.ToString());
			Log.Text.Clear(); state.State = to;
			ExitChangeState = true;
		}
		public static string DFormatTime(float t)
		{
			return "@" + t.ToString("0.00");
		}
		/// <summary>
		/// Updates ID caches: ActiveIDFrom, ActiveIDFromTo, ActiveIDCndFrom, ActiveIDCndFromTo, ActiveIDCbFromTo
		/// </summary>
		/// <param name="from">The state from which FSM is transitioning.</param>
		/// <param name="to">The state to which FSM is transitioning. Optional: if not provided, UpdateActive will only update ActiveIDFrom.</param>
		private static void UpdateActiveForChangeState(int ruleDataID)
		{
			// same as UpdateActive but use to as from
		}

		private static void UpdateActive(int stateID, int to = -1)
		{

			// take in comid or dataid, update only that one based on current state

			// make alternate updateActives for other use cases
			ref SJoin join = ref Joins[0];
			ref SRule rule = ref Rules[0];

			for (int i = 0; i < Joins.Count; i++)
			{
				join = ref Joins[i];
				rule = ref Rules.AtId(join.RuleID);

				if (join.StateID == stateID)

				join.ActiveChecks = 0;

				if (StateEqualOrInvalid(rule.From, States.AtId(stateID).State))
				{
					join.ActiveChecks |= EActive.From;

					if (rule.EndCnd != null)
						join.ActiveChecks |= EActive.EndCndFrom;

					if (rule.To == to && StateValid(to) && StateValid(rule.To))
					{
						join.ActiveChecks |= EActive.FromTo;

						if (rule.EndCnd != null)
							join.ActiveChecks |= EActive.EndCndFromTo;
					}
				}
				if (StateValid(to) && StateEqualOrInvalid(rule.To, to))
					join.ActiveChecks |= EActive.EndCbFromTo;
			}
		}
		private static void UpdateActiveConditions(int stateDataID)
		{
		
		}

		private static bool TryCallbacks(int comID, int from, int to)
		{
			ref SRule rule = ref Rules[0];

			for (int i = 0; i < Joins.Count; i++)
			{
				if (Joins[i].ComID == comID)
				{
					rule = ref Rules[i];
					bool call = true;
					if (rule.CallbackCnd != null)
						call = rule.CallbackCnd.Invoke();
					if (call)
						rule.Callback?.Invoke(from, to);
				}
			}
			// if callbacks changed state, return true so we can cancel current ChangeState
			return (States.AtId(ECS.Coms.AtId(comID).DataID).State != from);
		}
		public static void TickConditions(float time)
		{
			for (int i = 0; i < Joins.Count; i++)
			{
				UpdateActiveConditions(States.IndicesToIds[i]);

				ref SJoin join = ref Joins[i];
				ref SRule rule = ref Rules.AtId(join.RuleID);

				if (
					join.TimerID < 0
					&& rule.EndCnd.Invoke() == true
					&& StateValid(rule.To))
				{
					ChangeState(time, stateID: States.IndicesToIds[i], rule.To);
				}
			}
		}
		public static void InitStateTimer(float time, int comID, int ruleID, out int timerID)
		{
			ref SState state = ref States.AtId(ECS.Coms.AtId(comID).DataID);
			ref SRule rule = ref Rules.AtId(ruleID);
			void Callback(int timerID)
			{
				ref SRule ruleInCb = ref Rules.AtId(ruleID);
				if (ruleInCb.EndCnd == null && ruleInCb.EndCnd?.Invoke() == true)
					ChangeState(time, stateID: ECS.Coms.AtId(comID).DataID, ruleInCb.To);
			}
			if (rule.To == state.State)
				Timer.Add(time, out timerID, comID: comID, rule.Duration, rule.TimerStart, Callback, false);
			else
				timerID = -1;
		}
		private static bool StatesEqualAndValid(int state1, int state2)
		{ 
			return (state1 == state2 && StateValid(state1) && StateValid(state2));
		}
		private static bool StateEqualOrInvalid(int state1, int state2)
		{
			return state1 == state2 || state1 == InvalidState;
		}
		private static bool StatesEqual(int state1, int state2, bool invalidOk1, bool invalidOk2)
		{
			if (state1 == state2 && state1 != InvalidState)
				return true;
			bool output = true;
			output &= state1 != InvalidState || (invalidOk1 && (state1 == InvalidState));
			output &= state2 != InvalidState || (invalidOk2 && (state2 == InvalidState));
			// breaks when state 1 and 2 are valid but different
			return output;
		}
		private static bool StateValid(int state)
		{
			return state != InvalidState;
		}
		public static void Invoke(ECallback e)
		{

		}
	}
}
