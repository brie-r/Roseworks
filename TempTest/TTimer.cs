using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RelaStructures;

namespace TestTimer
{
	
	public class TestParams
	{
		public int ObjectCount = 1;
		public int TimersPerObject = 1;
		public int TimerCount = 1;
		public int TimerEndCount = 0;
		public bool TimerEndFlag = false;
		public int SkipObject = 0;
		public int SkipTimer = 0;
		public int Successes = 0;
		public int ExpectedSuccesses = 0;
		public float AverageError = 0;
		public float ErrorThreshold = 0.001f;
		public float TimerLength = 0;
		public float StartTime = 0;
		public float TotalTime = 0;
		public int StartFrame = 0;
		public int TotalFrames = 0;
		public bool DebugPrint = false;
		public GameObject[] g;
		public BehaviorContainer[] b;
		public Timer[] t;
		public int[] id;

		public TestParams
		(
			int objectCount = 1,
			int timersPerObject = 1,
			int timerEndCount = 0,
			bool timerEndFlag = false,
			int skipObject = 0,
			int skipTimer = 0,
			int successes = 0,
			int expectedSuccesses = 0,
			float averageError = 0,
			float errorThreshold = 0.001f,
			float timerLength = 0,
			float startTime = 0,
			float totalTime = 0,
			int startFrame = 0,
			int totalFrames = 0,
			bool debugPrint = false
		)
		{
			ObjectCount = objectCount;
			TimersPerObject = timersPerObject;
			TimerCount = ObjectCount * TimersPerObject;
			TimerEndCount = timerEndCount;
			TimerEndFlag = timerEndFlag;
			SkipObject = skipObject;
			SkipTimer = skipTimer;
			Successes = successes;
			ExpectedSuccesses = expectedSuccesses;
			if (ExpectedSuccesses == 0)
				ExpectedSuccesses = (ObjectCount - SkipObject) * TimersPerObject - SkipTimer;
			AverageError = averageError;
			ErrorThreshold = errorThreshold;
			TimerLength = timerLength;
			StartTime = startTime;
			TotalTime = totalTime;
			StartFrame = startFrame;
			TotalFrames = totalFrames;
			DebugPrint = debugPrint;
			g = new GameObject[ObjectCount];
			b = new BehaviorContainer[ObjectCount];
			t = new Timer[ObjectCount];
			id = new int[TimerCount];
	}
}
	public class TTimer
	{
		public void TestInit(TestParams tp)
		{
			tp.TimerEndCount = 0;
			tp.Successes = 0;
			SetStartTime(tp);
		}
		
		public void TestInit2(TestParams tp)
		{
			SetupTimers(tp);
			SetStartTime(tp);
		}

		public void SetStartTime(TestParams tp)
		{
			tp.StartTime = Time.time;
			tp.StartFrame = Time.frameCount;
		}
		public void CalculateTime(TestParams tp)
		{
			tp.TotalFrames = Time.frameCount - tp.StartFrame - 1;
			tp.TotalTime = Time.time - tp.StartTime - Time.deltaTime * 1f;
		}
		public void TestEndPrint(TestParams tp)
		{
			CalculateTime(tp);
			if (tp.ExpectedSuccesses > 1)
				Debug.Log("Successes: " + tp.Successes + " / " + tp.ExpectedSuccesses);
			Debug.Log("Duration: " + tp.TotalTime + " sec, " + tp.TotalFrames + " frames");
		}

		public IEnumerator TestWait(TestParams tp)
		{
			if (tp.DebugPrint) Debug.Log("Looped @ " + Time.time);
			yield return null;
		}

		public void TimerEnd(TestParams tp, int _)
		{
			if (tp.DebugPrint) Debug.Log("TestEndCallback ran @ " + Time.time);
			tp.TimerEndCount++;
			tp.TimerEndFlag = true;
		}
		public bool WithinThreshold(TestParams tp, float input, float center)
		{
			return (input > center - tp.ErrorThreshold
				&& input < center + tp.ErrorThreshold);
		}
		public void SetupTimers(TestParams tp)
		{
			for (int oi = 0; oi < tp.ObjectCount; oi++)
			{
				// initialize test, create gameobjects with timer behavior
				tp.g[oi] = new GameObject();
				tp.b[oi] = tp.g[oi].AddComponent<BehaviorContainer>();
				tp.g[oi].TryGetComponent(out tp.t[oi]);
				if (tp.DebugPrint) Debug.Log("Added timer @ " + Time.time);

				// add timers
				for (int ti = 0; ti < tp.TimersPerObject; ti++)
					tp.id[oi * tp.TimersPerObject + ti] = tp.t[oi].AddTimer(tp.g[oi].GetInstanceID(), tp.TimerLength, callback: (int timerID) => { TimerEnd(tp, timerID); });
			}
		}
		public void TestEnd(TestParams tp)
		{
			CalculateTime(tp);
			Debug.Log("Duration: " + tp.TotalTime + " sec, " + tp.TotalFrames + " frames");
			if (tp.ExpectedSuccesses > 1)
				Debug.Log("Successes: " + tp.Successes + " / " + tp.ExpectedSuccesses);
			Assert.AreEqual(tp.ExpectedSuccesses, tp.Successes);
		}

		// test if adding a BehaviorContainer adds a Timer simultaneously
		[Test] public void AutoAddTimer()
		{
			GameObject g = new GameObject();
			g.AddComponent<BehaviorContainer>();
			Assert.IsTrue(g.TryGetComponent(out BehaviorContainer _) && g.TryGetComponent(out Timer _));
		}
		// sanity check - a new GameObject shouldn't have BehaviorContainer
		[Test] public void NoBehavior()
		{
			GameObject g = new GameObject();
			Assert.IsFalse(g.TryGetComponent(out BehaviorContainer _));
		}
		// test 0-second timers in sequence
		[UnityTest] public IEnumerator Timer0f()
		{
			TestParams tp = new TestParams(objectCount: 100, timerLength: 0);
			TestInit(tp);
			for (int i = 0; i < tp.ObjectCount; i++)
			{
				// initialize test, create gameobject with timer
				SetStartTime(tp);
				GameObject g = new GameObject();
				BehaviorContainer b = g.AddComponent<BehaviorContainer>();
				g.TryGetComponent(out Timer t);
				if (tp.DebugPrint) Debug.Log("Added timer @ " + Time.time);

				// add timer, wait until timer ends
				t.AddTimer(g.GetInstanceID(), tp.TimerLength, callback: (int timerID) => { TimerEnd(tp, timerID); });
				while (tp.TimerEndFlag == false)
				{
					if (tp.DebugPrint) Debug.Log("Looped @ " + Time.time);
					yield return null;
				}
				tp.TimerEndFlag = false;
				CalculateTime(tp);
				if (tp.DebugPrint) Debug.Log("Ended @ " + Time.time);

				// check if the timer ended at the right time
				tp.AverageError += Mathf.Abs(tp.TotalTime - tp.TimerLength);
				if (tp.TimerLength > tp.TotalTime - tp.ErrorThreshold
				&& tp.TimerLength < tp.TotalTime + tp.ErrorThreshold)
					tp.Successes++;
				TestEndPrint(tp);
			}
			Debug.Log("Average Error (s): " + tp.AverageError / (float)tp.ObjectCount);
			Assert.AreEqual(tp.ObjectCount, tp.Successes);
		}
		// test many concurrent timers
		[UnityTest] public IEnumerator TimerOverflowAdd()
		{
			// initialize test, create gameobject with timer
			TestParams tp = new TestParams(objectCount: 1000, timerLength: 0.1f);
			TestInit(tp);
			GameObject g = new GameObject();
			BehaviorContainer b = g.AddComponent<BehaviorContainer>();
			g.TryGetComponent(out Timer t);
			if (tp.DebugPrint) Debug.Log("Added timer @ " + Time.time);

			// add timers, wait until all timers end
			// OR TimerLength + 1 seconds, whichever comes first
			for (int i = 0; i < tp.ObjectCount; i++)
				t.AddTimer(g.GetInstanceID(), tp.TimerLength, callback: (int timerID) => { TimerEnd(tp, timerID); });
			CalculateTime(tp);
			while (tp.TimerEndCount < tp.ObjectCount && Time.time - tp.StartTime < tp.TimerLength + 1f)
			{
				if (tp.DebugPrint) Debug.Log("Timer 95 duration: " + t.Timers.Values[t.Timers.IdsToIndices[95]].Duration
					+ ", Timer 105 duration: " + t.Timers.Values[t.Timers.IdsToIndices[105]].Duration);
				yield return null;
			}
			CalculateTime(tp);
			if (tp.DebugPrint) Debug.Log("Ended @ " + Time.time);

			// check if all timers ended
			tp.Successes = tp.TimerEndCount;
			TestEndPrint(tp);
			Assert.AreEqual(tp.ObjectCount, tp.Successes);
		}
		// test timers with duration>0s in sequence
		[UnityTest] public IEnumerator TimerLength()
		{
			SortedDictionary<float, int> errorSpread = new SortedDictionary<float, int> { };
			TestParams tp = new TestParams(objectCount: 100, timerLength: 0.1f);
			TestInit(tp);
			for (int i = 0; i < tp.ObjectCount; i++)
			{
				// initialize test, create gameobject with timer
				SetStartTime(tp);
				GameObject g = new GameObject();
				BehaviorContainer b = g.AddComponent<BehaviorContainer>();
				g.TryGetComponent(out Timer t);
				if (tp.DebugPrint) Debug.Log("Added timer @ " + Time.time);

				// add timer, wait until timer ends
				t.AddTimer(g.GetInstanceID(), tp.TimerLength, callback: (int timerID) => { TimerEnd(tp, timerID); });
				while (tp.TimerEndFlag == false)
				{
					if (tp.DebugPrint) Debug.Log("Looped @ " + Time.time);
					yield return null;
				}
				tp.TimerEndFlag = false;
				CalculateTime(tp);
				if (tp.DebugPrint) Debug.Log("Ended @ " + Time.time);

				// check if the timer ended at the right time
				tp.AverageError += Mathf.Abs(tp.TotalTime - tp.TimerLength);
				float category = Mathf.RoundToInt((tp.TotalTime - tp.TimerLength) * 2000f)/2000f;
				if (errorSpread.TryGetValue(category, out int e))
				{
					errorSpread[category] += 1;
				}
				else
				{
					errorSpread.Add(category, 1);
				}

				if (tp.TimerLength > tp.TotalTime - tp.ErrorThreshold
				&& tp.TimerLength < tp.TotalTime + tp.ErrorThreshold)
					tp.Successes++;
				//TestEndPrint(tp);
				Object.Destroy(g);
			}
			Debug.Log("\r\nAverage Error (s): " + tp.AverageError / (float)tp.ObjectCount);

			Debug.Log("\r\nError Histogram:");
			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach(KeyValuePair<float,int> kvp in errorSpread)
			{
				s.Clear();
				for (int i = 0; i < kvp.Value/3f; i++)
					s.Append("|");
				Debug.Log(kvp.Key + (kvp.Key.ToString().Length>=7?"\t":"\t\t") + s);
			}
			Assert.AreEqual(tp.ObjectCount, tp.Successes);
		}
		[UnityTest]	public IEnumerator CancelAllByOwnerID()
		{
			TestParams tp = new TestParams(objectCount: 100, timersPerObject: 10, timerLength: 0.1f);
			TestInit2(tp);

			// cancel timers
			for (int i = 0; i < tp.ObjectCount - tp.SkipObject; i++)
				tp.b[i].Ref<Timer>().CancelByOwnerID(tp.g[i].GetInstanceID());

			// wait until timers normally would have finished
			while (tp.TimerEndFlag == false && Time.time - tp.StartTime < tp.TimerLength + 0.1f)
				yield return TestWait(tp);

			// check if the correct number of timers ended
			tp.Successes = tp.TimerCount - tp.TimerEndCount;
			TestEnd(tp);
		}
		[UnityTest] public IEnumerator CancelSomeByOwnerID()
		{
			TestParams tp = new TestParams(objectCount: 100, timersPerObject: 10, timerLength: 0.1f, skipObject: 4);
			TestInit2(tp);

			// cancel timers
			for (int i = 0; i < tp.ObjectCount - tp.SkipObject; i++)
				tp.b[i].Ref<Timer>().CancelByOwnerID(tp.g[i].GetInstanceID());

			// wait until timers normally would have finished
			while (tp.TimerEndFlag == false && Time.time - tp.StartTime < tp.TimerLength + 0.1f)
				yield return TestWait(tp);

			// check if the correct number of timers ended
			tp.Successes = tp.TimerCount - tp.TimerEndCount;
			TestEnd(tp);
		}
		[UnityTest] public IEnumerator CancelAllByTimerID()
		{
			TestParams tp = new TestParams(objectCount: 100, timersPerObject: 10, timerLength: 0.1f);
			TestInit2(tp);

			// cancel timers
			for (int i = 0; i < tp.TimerCount - tp.SkipTimer; i++)
				tp.b[i/tp.TimersPerObject].Ref<Timer>().CancelByTimerID(tp.id[i]);

			// wait until timers normally would have finished
			while (tp.TimerEndFlag == false && Time.time - tp.StartTime < tp.TimerLength + 0.1f)
				yield return TestWait(tp);

			// check if the correct number of timers ended
			tp.Successes = tp.TimerCount - tp.TimerEndCount;
			TestEnd(tp);
		}
		[UnityTest]	public IEnumerator CancelSomeByTimerID()
		{
			TestParams tp = new TestParams(objectCount: 100, timersPerObject: 10, timerLength: 0.1f, skipTimer: 4);
			TestInit2(tp);

			// cancel timers
			for (int i = 0; i < tp.TimerCount - tp.SkipTimer; i++)
				tp.b[i / tp.TimersPerObject].Ref<Timer>().CancelByTimerID(tp.id[i]);

			// wait until timers normally would have finished
			while (tp.TimerEndFlag == false && Time.time - tp.StartTime < tp.TimerLength + 0.1f)
				yield return TestWait(tp);

			// check if the correct number of timers ended
			tp.Successes = tp.TimerCount - tp.TimerEndCount;
			TestEnd(tp);
		}
		[UnityTest]	public IEnumerator CancelNonexistentByTimerID()
		{
			TestParams tp = new TestParams(objectCount: 100, timersPerObject: 1, timerLength: 0.1f);
			TestInit2(tp);

			// cancel timers
			for (int i = 0; i < tp.TimerCount; i++)
				tp.b[i / tp.TimersPerObject].Ref<Timer>().CancelByTimerID(i + 500);

			// wait until timers normally would have finished
			while (tp.TimerEndFlag == false && Time.time - tp.StartTime < tp.TimerLength + 0.1f)
				yield return TestWait(tp);

			// check if the correct number of timers ended
			tp.Successes = tp.TimerEndCount;
			TestEnd(tp);
		}
		[UnityTest]	public IEnumerator CancelNonexistentByIndex()
		{
			TestParams tp = new TestParams(objectCount: 100, timersPerObject: 1, timerLength: 0.1f);
			TestInit2(tp);

			// cancel timers
			for (int i = 0; i < tp.TimerCount; i++)
				tp.b[i / tp.TimersPerObject].Ref<Timer>().CancelByIndex(i + 500);

			// wait until timers normally would have finished
			while (tp.TimerEndFlag == false && Time.time - tp.StartTime < tp.TimerLength + 0.1f)
				yield return TestWait(tp);

			// check if the correct number of timers ended
			tp.Successes = tp.TimerEndCount;
			TestEnd(tp);
		}
		[UnityTest]	public IEnumerator TimerMisc()
		{
			float wait = 0.4f;
			TestParams tp = new TestParams(objectCount: 1, timersPerObject: 1, timerLength: 1f, expectedSuccesses: 3, errorThreshold: 0.001f);
			TestInit2(tp);

			// wait until the designated time
			while (Time.time - tp.StartTime < wait - Time.deltaTime/2f)
				yield return TestWait(tp);

			CalculateTime(tp);
			Debug.Log("GetTimerElapsed: " + tp.t[0].Elapsed(tp.id[0]) + ", Expected: " + wait);
			if (WithinThreshold(tp, tp.t[0].Elapsed(tp.id[0]), wait))
				tp.Successes++;
			else Debug.Log("fail 2");

			Debug.Log("GetTimerRemaining: " + tp.t[0].Remaining(tp.id[0]) + ", Expected: " + (tp.TimerLength - wait));
			if (WithinThreshold(tp, tp.t[0].Remaining(tp.id[0]), tp.TimerLength - wait))
				tp.Successes++;
			else Debug.Log("fail 3");

			// check if the correct number of timers ended
			TestEnd(tp);
		}

	}
}