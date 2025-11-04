using UnityEngine;
using UnityEngine.UI;

namespace Mobile
{
    [RequireComponent(typeof(Button))]
    public class UIButton_E : MonoBehaviour
    {
        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Debug.Log("[UIButton_E] 模拟E键点击");
                GlobalInput.SimulateEPress();
            });
        }
    }
}