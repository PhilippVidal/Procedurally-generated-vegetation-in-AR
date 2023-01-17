using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public bool useEditorCam = false;
    Camera sceneCam;
    public float speed;
    public float turnSpeed; 
    Transform mainCameraTransform;
    Camera currentCam;
    private void Awake()
    {
        sceneCam = GetComponent<Camera>();
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (useEditorCam)
        {
            Camera currentCam = Camera.current;
            if (currentCam)
            {
                sceneCam.transform.position = currentCam.transform.position;
                sceneCam.transform.rotation = currentCam.transform.rotation;
            }
        }        
    }
}
