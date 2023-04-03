using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public Camera freeCam;
    public Camera frontCam;
    public Camera sideCam;
    public Camera topCam;

    private bool allCams = true;
    private bool allDisabled = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)){
            ToggleCameraView();
        }
    }

    public void DisableCamControl()
    {
        allDisabled = true;
        freeCam.gameObject.GetComponent<SelectorScript>().enabled = false;
        freeCam.gameObject.GetComponent<FreeCam>().enabled = false;
        topCam.gameObject.GetComponent<SelectorScript>().enabled = false;
        sideCam.gameObject.GetComponent<SelectorScript>().enabled = false;
        frontCam.gameObject.GetComponent<SelectorScript>().enabled = false;
    }

    public void EnableCamControl()
    {
        freeCam.gameObject.GetComponent<SelectorScript>().enabled = true;
        freeCam.gameObject.GetComponent<FreeCam>().enabled = true;
        topCam.gameObject.GetComponent<SelectorScript>().enabled = true;
        sideCam.gameObject.GetComponent<SelectorScript>().enabled = true;
        frontCam.gameObject.GetComponent<SelectorScript>().enabled = true;
        allDisabled = false;
    }

    public void ToggleCameraView()
    {
        if (allDisabled)
            return;
        if (allCams)
        {
            Rect freeCamRectCopy = freeCam.rect;
            freeCamRectCopy.width = 1;
            freeCamRectCopy.height = 1;
            freeCamRectCopy.y = 0;
            freeCam.rect = freeCamRectCopy;
            topCam.enabled = false;
            sideCam.enabled = false;
            frontCam.enabled = false;
            topCam.gameObject.SetActive(false);
            sideCam.gameObject.SetActive(false);
            frontCam.gameObject.SetActive(false);
        }
        else
        {
            Rect freeCamRectCopy = freeCam.rect;
            freeCamRectCopy.width = 0.5f;
            freeCamRectCopy.height = 0.5f;
            freeCamRectCopy.y = 0.5f;
            freeCam.rect = freeCamRectCopy;
            topCam.enabled = true;
            sideCam.enabled = true;
            frontCam.enabled = true;
            topCam.gameObject.SetActive(true);
            sideCam.gameObject.SetActive(true);
            frontCam.gameObject.SetActive(true);
        }
        allCams = !allCams;
    }

}
