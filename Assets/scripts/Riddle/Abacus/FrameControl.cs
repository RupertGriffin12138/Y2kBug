using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameControl : MonoBehaviour
{

    [SerializeField] private int lineNum = 0;

    public GameObject frame0;   // 物体 "Frame_0"
    public GameObject frame1;   // 物体 "Frame_1"


    protected virtual void Start()
    {
        frame0.SetActive(true);
        frame1.SetActive(false);
    }

    protected virtual void Update()
    {
        frameSetActive();
        frameMoveControl();

    }

    private void frameMoveControl()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            lineNum = (++lineNum) % 9;

            MoveActiveObject();

        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            lineNum = (--lineNum) % 9;

            MoveActiveObject();
        }
    }

    private void MoveActiveObject()
    {
        if (frame0.activeSelf)
        {
            if(lineNum == 0)
                frame0.transform.position = new Vector3(-0.27f, 1.7955f, 0);
            else if (Input.GetKeyUp(KeyCode.D))
                frame0.transform.position += new Vector3(0.3f, 0, 0);
            else if(Input.GetKeyUp(KeyCode.A))
                frame0.transform.position -= new Vector3(0.3f, 0, 0);
        }
        else if (frame1.activeSelf)
        {
            if (lineNum == 0)
                frame1.transform.position = new Vector3(-0.28f, 0.86f, 0);
            else if (Input.GetKeyUp(KeyCode.D))
                frame1.transform.position += new Vector3(0.3f, 0, 0);
            else if (Input.GetKeyUp(KeyCode.A))
                frame1.transform.position -= new Vector3(0.3f, 0, 0);
        }
    }

    private void frameSetActive()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            frame0.SetActive(true);
            frame1.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            frame0.SetActive(false);
            frame1.SetActive(true);
        }
    }

    
}
