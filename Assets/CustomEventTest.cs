using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomEventTest : MonoBehaviour
{
    public UnityEvent simpleEvent;
    public CustomComplexEvent complexEvent;

	public ScriptableObjectEventTest soTest;

    public void TriggerSimpleEvent()
    {
        if(simpleEvent != null)
        {
            simpleEvent.Invoke();
        }
    }

    public void TriggerComplexEvent()
    {
        if (complexEvent != null)
        {
            complexEvent.Invoke("a", 1, 2);
        }
        StartCoroutine(DelayedTrigger("Test", 0, 1));
    }

    private IEnumerator DelayedTrigger(string message, int a, int b)
    {
        for(int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.1f);
            if (complexEvent != null)
            {
                complexEvent.Invoke(message, a, b);
            }
        }
    }
}

[System.Serializable]
public class CustomComplexEvent : UnityEvent<string, int, int> { }
