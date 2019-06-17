using UnityEngine;
using System.Collections;
using System;

public class RunAfter : MonoBehaviour {

	private static Hashtable _hash;

	public static Hashtable Hash {
		get {
			if(_hash == null)
				_hash = new Hashtable();
			return _hash;
		}
	}

	public static void runAfter(object owner, Action task, float time) {
		GameObject go = new GameObject("RunAfter");
		RunAfter run = go.AddComponent<RunAfter>();

		ArrayList tasks = (ArrayList) Hash[owner];
		if(tasks == null) {
			tasks = new ArrayList();
			Hash.Add(owner, tasks);
		}
		tasks.Add(run);

		run.runTask(owner, task, time);
	}

	public static void removeTasks(object owner) {
		ArrayList tasks = (ArrayList) Hash[owner];
		if(tasks != null) {
			for(int i = 0; i < tasks.Count; ++i) {
				RunAfter run = (RunAfter) tasks[i];
				run.cleanUp();
			}
			tasks.Clear();
		}
	}

	public static void removeTask(object owner, object task) {
		ArrayList tasks = (ArrayList) Hash[owner];
		if(tasks != null) {
			tasks.Remove(task);
		}
	}

	private object _owner;

    private Action _task;

    public void runTask(object owner, Action task, float time)
    {
		_owner = owner;
		_task = task;
		Invoke("_runTask", time);
	}

	private void _runTask() {
		if(_task != null)
			_task();

		removeTask(_owner, this);
		cleanUp();
	}

	public void cleanUp() {
		CancelInvoke();
		Destroy(gameObject);
	}
}
