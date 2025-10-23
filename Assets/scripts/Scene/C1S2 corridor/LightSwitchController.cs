using UnityEngine;

public class LightSwitchController : MonoBehaviour
{
    public GameObject switchOnObject; // "switchon" ����
    public GameObject lightParent;    // "Light" ������
    private bool isDisplayed = false;

    void Start()
    {
        // Ĭ����������� "switchon" �� "Light" ��
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
        // ��ȡ��ҵ�λ��
        Vector3 playerPosition = transform.position;

        // �������Ƿ���ָ����X�᷶Χ��
        if (playerPosition.x >= -2.9f && playerPosition.x <= -1.44f)
        {
            //Debug.Log("Player is within the range.");
            // �����Ұ���E��
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
        // �л���ʾ״̬
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


