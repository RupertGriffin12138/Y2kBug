using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// ����TMP��������Text����TMP_Text��using TMPro;

public class SavePanelUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveDetailPanel;          // ȷ����壨Ĭ�����أ�
    public Button[] saveButtons;                // 8 ���浵��ť
    public Text[] slotLabels;                   // ��ѡ����ʾ������+ʱ�䡱
    public Button btnYes;                       // ȷ�� Yes
    public Button btnNo;                        // ȷ�� No
    public Text confirmText;                    // ��ѡ��ȷ����ʾ����

    [Header("Config")]
    public Transform player;                    // ��ѡ�������������
    public bool savePlayerPos = true;
    public string firstSceneName = "C1S0";

    // ���� ��ť���Ѵ浵�����ӻ�����ѡ��һ����ϣ� ����
    [Header("Visual: Saved State")]
    [Tooltip("�ձ귨��ÿ����λ��Ӧһ�����Ѵ浵��Сͼ/���/����hasSaveʱ SetActive(true)")]
    public GameObject[] savedBadges;            // size=8����Ϊ��

    [Tooltip("����ͼ������ťImage��Sprite�����Ƿ��д浵�л�")]
    public Image[] slotButtonImages;            // size=8����Ϊ��
    public Sprite savedSprite;                  // �д浵ʱ��
    public Sprite emptySprite;                  // �մ浵ʱ��

    [Tooltip("��ɫ����ͨ��ColorBlock�л�normalColor�����������ɫ�ɲ���")]
    public Color savedNormalColor = new Color(0.90f, 0.96f, 1f, 1f); // ǳ��
    public Color emptyNormalColor = Color.white;

    // 8 ���̶���λ�� PlayerPrefs Key
    private readonly string[] slotKeys = new string[8]
    {
        "SaveSlot_1","SaveSlot_2","SaveSlot_3","SaveSlot_4",
        "SaveSlot_5","SaveSlot_6","SaveSlot_7","SaveSlot_8"
    };

    private int selectedIndex = -1;

    void Start()
    {
        if (saveDetailPanel) saveDetailPanel.SetActive(false);

        // �� 8 ����ť
        if (saveButtons != null)
        {
            for (int i = 0; i < saveButtons.Length; i++)
            {
                int idx = i; // �հ�
                if (saveButtons[i] != null)
                    saveButtons[i].onClick.AddListener(() => OnSaveButtonClicked(idx));
            }
        }

        if (btnYes) btnYes.onClick.AddListener(OnConfirmYes);
        if (btnNo) btnNo.onClick.AddListener(CloseDetailPanel);

        RefreshSlotsUI();
    }

    void OnSaveButtonClicked(int index)
    {
        selectedIndex = index;

        if (saveDetailPanel) saveDetailPanel.SetActive(true);

        bool has = HasExistingSave(index);
        string info = GetSlotInfo(index);
        string prompt = has
            ? $"���Ǹô浵��\n��λ {index + 1}\n{info}"
            : $"���浱ǰ���ȵ���λ {index + 1}��";
        if (confirmText) confirmText.text = prompt;
    }

    void OnConfirmYes()
    {
        if (selectedIndex < 0) { CloseDetailPanel(); return; }

        if (GameState.Current == null)
            GameState.LoadGameOrNew(firstSceneName);

        GameState.Current.lastScene = SceneManager.GetActiveScene().name;
        if (savePlayerPos && player)
        {
            GameState.Current.playerX = player.position.x;
            GameState.Current.playerY = player.position.y;
        }

        // �л�����Ӧ��λ������
        SaveManager.UsePlayerPrefs(slotKeys[selectedIndex]);
        GameState.SaveNow();

        // ��չʾ��Ԫ��Ϣ������+ʱ�䣩
        string scene = GameState.Current.lastScene ?? "";
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        PlayerPrefs.SetString(slotKeys[selectedIndex] + "_metaScene", scene);
        PlayerPrefs.SetString(slotKeys[selectedIndex] + "_metaTime", time);
        PlayerPrefs.Save();

        RefreshSlotsUI();
        CloseDetailPanel();

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.ShowMessage($"�ѱ��浽��λ {selectedIndex + 1}");
    }

    void CloseDetailPanel()
    {
        if (saveDetailPanel) saveDetailPanel.SetActive(false);
        selectedIndex = -1;
    }

    // ���� ˢ�����в�λ���ı� + ���״̬ ����
    void RefreshSlotsUI()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            bool has = HasExistingSave(i);

            // �ı�
            if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
                slotLabels[i].text = GetSlotInfo(i);

            // ���
            ApplySlotVisual(i, has);
        }
    }

    // ���� Ӧ�ð�ť��ۣ��ձ�/����ͼ/��ɫ������󶨵��ֶ��Զ���Ч�� ����
    void ApplySlotVisual(int index, bool hasSave)
    {
        // 1) �ձ귨
        if (savedBadges != null && index < savedBadges.Length && savedBadges[index])
            savedBadges[index].SetActive(hasSave);

        // 2) ����ͼ��
        if (slotButtonImages != null && index < slotButtonImages.Length && slotButtonImages[index])
        {
            var img = slotButtonImages[index];
            if (hasSave && savedSprite) img.sprite = savedSprite;
            else if (!hasSave && emptySprite) img.sprite = emptySprite;
            // ���û�ṩ Sprite���Ͳ���
        }

        // 3) ��ɫ����ֻ��normalColor������������ťԭ����
        if (saveButtons != null && index < saveButtons.Length && saveButtons[index])
        {
            var btn = saveButtons[index];
            var colors = btn.colors;
            colors.normalColor = hasSave ? savedNormalColor : emptyNormalColor;
            btn.colors = colors;
        }
    }

    // ���� ��ʾ�ı����մ浵 / ����+ʱ�䣩 ����
    string GetSlotInfo(int index)
    {
        if (!HasExistingSave(index)) return "�մ浵";
        string scene = PlayerPrefs.GetString(slotKeys[index] + "_metaScene", "");
        string time = PlayerPrefs.GetString(slotKeys[index] + "_metaTime", "");
        if (string.IsNullOrEmpty(scene)) scene = "(δ֪����)";
        return $"{scene}\n{time}";
    }

    bool HasExistingSave(int index)
    {
        string key = slotKeys[index];
        return PlayerPrefs.HasKey(key) && !string.IsNullOrEmpty(PlayerPrefs.GetString(key));
    }
}
