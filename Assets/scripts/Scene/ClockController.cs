using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ClockController : MonoBehaviour
{
    [Header("Needles and UI")]
    public Transform second;
    public Transform minute;
    public Transform hour;
    public Transform minuteAfter;
    public Transform hourAfter;
    public Transform background;
    public Transform year1999;
    public Transform year2000;
    public Transform year0101;
    public Transform pivotMarker;

    [Header("Timing")]
    public float rotationSpeed = 100f;
    public float delayBeforeRotation = 0.5f;
    public float pauseAfterRotation = 1f;
    public float delayBetweenYears = 0.5f;

    private float totalRotation = 0f;
    private bool isRotating = false;
    private bool isPaused = false;
    private float startTime = 0f;
    private float pauseStartTime = 0f;
    private float switchTime = 0f;

    [Header("Warp")]
    public bool warpAtEnd = true;
    public string targetSceneName = "Town";
    public bool useTargetTag = false;
    public string targetObjectTag = "Respawn";
    public string targetObjectName = "ReturnPoint";
    public Vector3 targetOffset = Vector3.zero;
    public string playerTag = "Player";
    public int stickyFrames = 5;
    public float warpDelay = 2f;

    private bool hasWarped = false;

    // ---------- Dialogue ----------
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)] public string content;
    }

    [Header("Dialogue")]
    public KeyCode interactKey = KeyCode.E;
    public List<DialogueLine> dialogue = new List<DialogueLine>();
    public TMP_Text textName;
    public TMP_Text textBox;

    private int dialogIndex = 0;

    private enum Phase { Idle, Rotating, Pausing, Switched, YearDone, Dialog, Exiting, Done }
    private Phase phase = Phase.Idle;

    void Start()
    {
        if (second) second.gameObject.SetActive(false);
        if (minuteAfter) minuteAfter.gameObject.SetActive(false);
        if (hourAfter) hourAfter.gameObject.SetActive(false);
        if (year2000) year2000.gameObject.SetActive(false);
        if (year0101) year0101.gameObject.SetActive(false);

        if (minute) minute.gameObject.SetActive(true);
        if (hour) hour.gameObject.SetActive(true);
        if (year1999) year1999.gameObject.SetActive(true);

        // 一开始 textbox 显示提示
        if (textBox) textBox.text = "按 <b>E</b> 交互";
        if (textName) textName.text = "";

        phase = Phase.Idle;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (phase == Phase.Idle)
            {
                StartClockFlow();
            }
            else if (phase == Phase.Dialog)
            {
                AdvanceDialogueOrExit();
            }
        }

        if (phase == Phase.Rotating && totalRotation < 30f)
        {
            float elapsed = Time.time - startTime;
            if (elapsed >= delayBeforeRotation)
            {
                Vector3 pivot = pivotMarker ? pivotMarker.position : transform.position;
                float step = -rotationSpeed * Time.deltaTime;
                totalRotation -= step;
                if (second) second.RotateAround(pivot, Vector3.forward, step);
            }
            if (totalRotation >= 30f) StopRotationAndPause();
        }
        else if (phase == Phase.Pausing)
        {
            float elapsed = Time.time - pauseStartTime;
            if (elapsed >= pauseAfterRotation) SwitchObjectsAfterPause();
        }
        else if (phase == Phase.Switched)
        {
            float elapsed = Time.time - switchTime;
            if (elapsed >= delayBetweenYears) SwitchYearTo0101ThenDialog();
        }
    }

    // ---- Clock flow ----
    void StartClockFlow()
    {
        if (phase != Phase.Idle) return;
        if (second) second.gameObject.SetActive(true);
        startTime = Time.time;
        totalRotation = 0f;
        isRotating = true;
        phase = Phase.Rotating;
        if (textBox) textBox.text = "正在运行……";
    }

    void StopRotationAndPause()
    {
        isRotating = false;
        isPaused = true;
        pauseStartTime = Time.time;
        phase = Phase.Pausing;
    }

    void SwitchObjectsAfterPause()
    {
        isPaused = false;
        switchTime = Time.time;

        if (minute) minute.gameObject.SetActive(false);
        if (hour) hour.gameObject.SetActive(false);
        if (year1999) year1999.gameObject.SetActive(false);

        if (minuteAfter) minuteAfter.gameObject.SetActive(true);
        if (hourAfter) hourAfter.gameObject.SetActive(true);
        if (year2000) year2000.gameObject.SetActive(true);

        phase = Phase.Switched;
    }

    void SwitchYearTo0101ThenDialog()
    {
        if (year2000) year2000.gameObject.SetActive(false);
        if (year0101) year0101.gameObject.SetActive(true);

        phase = Phase.YearDone;

        if (dialogue != null && dialogue.Count > 0)
            BeginDialog();
        else
            TriggerExitTask();
    }

    // ---- Dialogue ----
    void BeginDialog()
    {
        dialogIndex = 0;
        ShowCurrentLine();
        phase = Phase.Dialog;
    }

    void AdvanceDialogueOrExit()
    {
        dialogIndex++;
        if (dialogue == null || dialogIndex >= dialogue.Count)
        {
            if (textBox) textBox.text = "……";
            TriggerExitTask();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    void ShowCurrentLine()
    {
        var line = dialogue[Mathf.Clamp(dialogIndex, 0, dialogue.Count - 1)];
        if (textName) textName.text = line.speaker ?? "";
        if (textBox) textBox.text = line.content ?? "";
    }

    // ---- Warp ----
    void TriggerExitTask()
    {
        if (phase == Phase.Exiting || phase == Phase.Done) return;
        if (warpAtEnd && !hasWarped)
        {
            hasWarped = true;
            phase = Phase.Exiting;
            StartCoroutine(WarpPlayerRoutine());
        }
        else phase = Phase.Done;
    }

    IEnumerator WarpPlayerRoutine()
    {
        if (string.IsNullOrEmpty(targetSceneName)) yield break;
        yield return new WaitForSeconds(warpDelay);

        DontDestroyOnLoad(this.gameObject);

        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
        while (!op.isDone) yield return null;
        yield return null;

        GameObject playerGO = null;
        float waited = 0f;
        const float maxWait = 2f;
        while (playerGO == null && waited < maxWait)
        {
            playerGO = GameObject.FindGameObjectWithTag(playerTag);
            if (playerGO == null) { waited += Time.unscaledDeltaTime; yield return null; }
        }
        if (playerGO == null) { CleanupAfterWarp(); yield break; }

        Transform target = null;
        if (useTargetTag)
        {
            var tgo = GameObject.FindGameObjectWithTag(targetObjectTag);
            if (tgo) target = tgo.transform;
        }
        else
        {
            var ngo = GameObject.Find(targetObjectName);
            if (ngo) target = ngo.transform;
        }

        if (target == null) { CleanupAfterWarp(); yield break; }

        Vector3 finalPos = target.position + targetOffset;
        Quaternion finalRot = playerGO.transform.rotation;
        PlaceCharacterSafely(playerGO, finalPos, finalRot);

        for (int i = 0; i < Mathf.Max(0, stickyFrames); i++)
        {
            yield return new WaitForEndOfFrame();
            PlaceCharacterSafely(playerGO, finalPos, finalRot);
        }

        CleanupAfterWarp();
    }

    void PlaceCharacterSafely(GameObject player, Vector3 pos, Quaternion rot)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            return;
        }

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            Physics.SyncTransforms();
            return;
        }

        var rb2 = player.GetComponent<Rigidbody2D>();
        if (rb2 != null)
        {
            rb2.velocity = Vector2.zero;
            rb2.angularVelocity = 0f;
            rb2.position = new Vector2(pos.x, pos.y);
            rb2.rotation = rot.eulerAngles.z;
            Physics2D.SyncTransforms();
            return;
        }

        player.transform.SetPositionAndRotation(pos, rot);
    }

    void CleanupAfterWarp()
    {
        Destroy(this.gameObject);
    }
}
