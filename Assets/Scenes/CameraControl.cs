using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    public float sensitivity = 5f;
    public float maxYAngle = 80f;
    private Vector2 currentRotation;
  
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //handle rotation
        currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
        currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
        currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
        Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        //handle translation
        Vector3 forward = Camera.main.transform.forward;
        if(Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.Translate(forward * 3.0f, Space.World);;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.Translate(forward * -3.0f,Space.World);
        }

    }
}
