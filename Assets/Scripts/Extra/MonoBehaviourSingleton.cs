using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
{
	private static T instance;

	public static T Get()
	{
		return instance;
	}

	public virtual void Awake()
	{
		if (instance == null)
		{
			instance = this as T;
		}
		else
		{
			Destroy(gameObject);
		}
	}

    private void OnDestroy()
    {
		if (instance == this as T) instance = null;
    }
}
