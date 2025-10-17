using UnityEngine;

public class Bead : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }

    #endregion

    private int frameNum;
    private int[] lineNum = new int[2];

    //算盘珠位置记录，检测是否是正确位置解密
    private int[,] beadRecard = new int[9, 2];

    private GameObject[] upperObjects;
    private GameObject[] lowerObjects;
    public int[,] clickCount = new int[10,3];

    #region States
    public BeadStateMachine stateMachine { get; private set; }

    public BeadShiningState shiningState { get; private set; }

    #endregion

    protected virtual void Awake()
    {
        stateMachine = new BeadStateMachine();

        shiningState = new BeadShiningState(stateMachine, this, "Shining");
    }

    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();

        frameNum = 0;

        // 初始化数组元素为0
        for (int i = 0; i < clickCount.GetLength(0); i++)
        {
            for (int j = 0; j < clickCount.GetLength(1); j++)
            {
                clickCount[i, j] = 0;
            }
        }


    }

    private void lineFind()
    {
        // 获取相应的Line对象
        Transform lineTransform = transform.Find("Line_" + lineNum[frameNum]);
        if (lineTransform != null)
        {
            Transform upperGroup = lineTransform.Find("upper");
            Transform lowerGroup = lineTransform.Find("lower");

            if (upperGroup != null && lowerGroup != null)
            {
                upperObjects = new GameObject[2];
                lowerObjects = new GameObject[5];

                for (int i = 0; i < 2; i++)
                {
                    upperObjects[i] = upperGroup.Find(i.ToString()).gameObject;
                }

                for (int i = 0; i < 5; i++)
                {
                    lowerObjects[i] = lowerGroup.Find(i.ToString()).gameObject;
                }
            }
            else
            {
                Debug.LogError("Upper or Lower group not found in Line_" + lineNum[frameNum]);
            }
        }
        else
        {
            Debug.LogError("Line_" + lineNum[frameNum] + " not found.");
        }
    }

    protected virtual void Update()
    {
        lineNumCount();
        frameNumCount();
        lineFind();

        if (Input.GetKeyDown(KeyCode.E))
        {
            clickCount[lineNum[frameNum], frameNum]++;
            HandleClick();
        }


    }

    private void frameNumCount()
    {
        if (Input.GetKeyUp(KeyCode.W))
            frameNum = 0;
        if (Input.GetKeyUp(KeyCode.S))
            frameNum = 1;
    }

    private void lineNumCount()
    {
        if (Input.GetKeyUp(KeyCode.D))
            lineNum[frameNum] = (++lineNum[frameNum]) % 9;
        if (Input.GetKeyUp(KeyCode.A))
            lineNum[frameNum] = (--lineNum[frameNum]) % 9;
    }

    private void HandleClick()
    {
        if (frameNum == 0)
        {
            clickCount[lineNum[frameNum], frameNum] %= 3;
            beadRecard[lineNum[frameNum], frameNum] = clickCount[lineNum[frameNum], frameNum];

            switch (clickCount[lineNum[frameNum], frameNum])
            {
                case 1:
                    MoveObjectDown(upperObjects[0]);
                    break;
                case 2:
                    MoveObjectDown(upperObjects[1]);
                    break;
                case 0:
                    ResetUpperObjects();
                    break;
            }
        }
        else if (frameNum == 1)
        {
            clickCount[lineNum[frameNum], frameNum] %= 6;
            beadRecard[lineNum[frameNum], frameNum] = clickCount[lineNum[frameNum], frameNum];

            switch (clickCount[lineNum[frameNum], frameNum])
            {
                case 1:
                    MoveObjectUp(lowerObjects[0]);
                    break;
                case 2:
                    MoveObjectUp(lowerObjects[1]);
                    break;
                case 3:
                    MoveObjectUp(lowerObjects[2]);
                    break;
                case 4:
                    MoveObjectUp(lowerObjects[3]);
                    break;
                case 5:
                    MoveObjectUp(lowerObjects[4]);
                    break;
                case 0:
                    ResetLowerObjects();
                    break;
            }
        }
    }

    private void MoveObjectDown(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        position.y -= 0.2f;
        obj.transform.position = position;
    }

    private void MoveObjectUp(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        position.y += 0.2f;
        obj.transform.position = position;
    }

    private void ResetUpperObjects()
    {
        foreach (GameObject obj in upperObjects)
        {
            Vector3 position = obj.transform.position;
            position.y += 0.2f;
            obj.transform.position = position;
        }
    }

    private void ResetLowerObjects()
    {
        foreach (GameObject obj in lowerObjects)
        {
            Vector3 position = obj.transform.position;
            position.y -= 0.2f;
            obj.transform.position = position;
        }
    }
}
