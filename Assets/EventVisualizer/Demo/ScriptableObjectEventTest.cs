using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenuAttribute(menuName = "UnityEventVisualizer/Scriptable Object Test")]
public class ScriptableObjectEventTest : ScriptableObject
{
	public UnityEvent OnTest;

	public void DoTest()
	{
		if(OnTest != null)
		{
			OnTest.Invoke();
		}
	}
}
