using UnityEngine;

public class LightSwitchController : MonoBehaviour
{
    public GameObject switchOnObject; // "switchon" 对象
    public GameObject lightParent;    // "Light" 父对象
    private bool isDisplayed = false;

    void Start()
    {
        // 默认情况下隐藏 "switchon" 和 "Light" 组
        if (switchOnObject != null)
        {
            switchOnObject.SetActive(false);
        }
        if (lightParent != null)
        {
            foreach (Transform child in lightParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // 获取玩家的位置
        Vector3 playerPosition = transform.position;

        // 检查玩家是否在指定的X轴范围内
        if (playerPosition.x >= -2.9f && playerPosition.x <= -1.44f)
        {
            //Debug.Log("Player is within the range.");
            // 如果玩家按下E键
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleDisplay();
            }
        }
        /*
        else
        {
            Debug.Log($"Player is not within the range. Current X position: {playerPosition.x}");
        }
        */
    }

    void ToggleDisplay()
    {
        // 切换显示状态
        isDisplayed = !isDisplayed;

        if (switchOnObject != null)
        {
            switchOnObject.SetActive(isDisplayed);
        }
        if (lightParent != null)
        {
            foreach (Transform child in lightParent.transform)
            {
                child.gameObject.SetActive(isDisplayed);
            }
        }

        //Debug.Log($"Objects displayed: {isDisplayed}");
    }

}


