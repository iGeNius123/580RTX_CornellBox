using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    public float sensitivity = 5f;
    public float maxYAngle = 80f;
    public GameObject origin;
    private Vector2 currentRotation;
    public Quaternion originalRotationValue; 
    // Use this for initialization
    void Start () {
        originalRotationValue = Camera.main.transform.rotation;

    }
	
	// Update is called once per frame
	void Update () {


        //handle translation
        Vector3 forward = Camera.main.transform.forward;
        if(Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.Translate(forward * .1f, Space.World);;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.Translate(forward * -.1f,Space.World);
        }
        Vector3 right =Camera.main.transform.right;
 
        if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.Translate(right * -.1f, Space.World); ;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.Translate(right * .1f, Space.World);
        }
        if (Input.GetKey(KeyCode.Alpha0))
        {
            Camera.main.transform.position = origin.transform.position;
            Camera.main.transform.localRotation = Quaternion.identity;
            Camera.main.transform.rotation = Quaternion.Slerp(transform.rotation, originalRotationValue, Time.time * 10.0f); ;


        }

        //handle rotation
        currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
        currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
        currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
        Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
    }
}
