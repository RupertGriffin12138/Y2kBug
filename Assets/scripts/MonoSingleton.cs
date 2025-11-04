using UnityEngine;


public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance)
            {
                instance =(T)FindAnyObjectByType<T>();
            }
            return instance;
        }

    }

    private void Awake()
    {
        if (global)
        {
            if (instance!=null&&instance!=this.gameObject.GetComponent<T>())
            {
                Destroy(this.gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            instance = this.gameObject.GetComponent<T>();
        }
        this.OnStart();
    }

    protected virtual void OnStart()
    {

    }
}