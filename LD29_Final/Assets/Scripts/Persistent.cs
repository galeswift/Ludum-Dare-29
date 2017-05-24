using UnityEngine;
using System.Collections;

public class Persistent : MonoBehaviour {

    public int level=1;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
