using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("MyMultiSelection")]
public class MyMultiSelection : MonoBehaviour {
	
	public List<GameObject> _gos = new List<GameObject>(); 
	public int count;
	 
	
//	public GameObject source;
	
	void Update() {
		
	}
	
}
