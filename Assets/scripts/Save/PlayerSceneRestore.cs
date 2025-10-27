using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneRestore : MonoBehaviour
{
    public Transform fallbackSpawnPoint;

    void Start()
    {
        var scene = SceneManager.GetActiveScene().name;
        var save = SaveManager.LoadOrDefault(scene);
        save.MigrateLegacyPlayerPosIfNeeded();

        if (save.TryGetPlayerPos(scene, out Vector2 pos2))
        {
            // 2D 物理
            if (TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                rb2d.position = pos2;
                rb2d.velocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }
            // 非 2D 物理（或用 3D 角色控制器）
            else
            {
                var p3 = transform.position;               // 保留原 z
                transform.position = new Vector3(pos2.x, pos2.y, p3.z);
            }
        }
        else if (fallbackSpawnPoint)
        {
            transform.position = fallbackSpawnPoint.position;
        }
    }
}
