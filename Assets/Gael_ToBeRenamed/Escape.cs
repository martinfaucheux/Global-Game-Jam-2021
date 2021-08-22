using UnityEngine;

public class Escape : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private float probabilityOfSpawn = 1f;
    private bool wasDestroyedOnSceneUnloaded = false;

    void Awake()
    {
        if (probabilityOfSpawn < 0.99f)
        {
            if (Random.Range(0f, 1f) > probabilityOfSpawn) despawn();
        }
            
    }

   private void despawn()
    {
        this.gameObject.SetActive(false);
    }

    // Script to be called AFTER EscapesManager
    void Start()
    {
        if (EscapesManager.instance != null) EscapesManager.instance.Register(this);
    }

    // Update is called once per frame
    void OnDestroy()
    {
        if (EscapesManager.instance != null && !wasDestroyedOnSceneUnloaded) EscapesManager.instance.UnRegister(this);
    }
}
