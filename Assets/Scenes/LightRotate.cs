using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRotate : MonoBehaviour {
    public Light light;
    private bool rotating;
	// Use this for initialization
	void Start () {
        rotating = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(rotating)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,transform.localEulerAngles.y + 1.0f, transform.localEulerAngles.z);
        }
        if (Input.GetKey(KeyCode.R))
        {
            rotating = !rotating;
        }
    }
}
