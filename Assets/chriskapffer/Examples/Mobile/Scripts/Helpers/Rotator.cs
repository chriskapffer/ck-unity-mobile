using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

    public Vector3 axis = Vector3.up;
    public float speed = 2;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(axis * speed);
	}
}
