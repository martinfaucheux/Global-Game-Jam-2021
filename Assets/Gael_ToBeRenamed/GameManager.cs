using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveExt
{
    public static float MaxTime(this AnimationCurve curve) => curve.keys[curve.length - 1].time;
}


public class GameManager : MonoBehaviour
{
    #region singleton
    private static GameManager _instance;
    public static GameManager instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        initialize();
    }
    #endregion
    [SerializeField] private AIPath playerAI;
    private float cameraShakePower = 2f;
    public float CameraShakePower => cameraShakePower;

    [SerializeField] private float startGameDelay = 0f;

    public static event Action<List<Collider2D>> onBlockRegistered;
    public static event Action<List<Collider2D>> onBlockUnRegistered;

    public static event Action onPlayerDie;
    public Vector3 playerPosition {
        get
        {
            if (playerAI != null)
                return playerAI.transform.position;
            else
                return Vector3.positiveInfinity;
        }
    }

    
    public AnimationCurve SpeedCurve = AnimationCurve.EaseInOut(0f, 1f, 5f, 4f); // SpeedCurve.Evaluate(paintingTimer)
    public AnimationCurve CameraShakeCurve = AnimationCurve.EaseInOut(0f, 0f, 5f, 5f); // SpeedCurve.Evaluate(paintingTimer)
    public AnimationCurve queueTimeCurve = AnimationCurve.EaseInOut(0f, 0f, 5f, 5f); // SpeedCurve.Evaluate(paintingTimer)

    private InstantiateAtMouseClick iamc;
    private Chrono chrono;

    public static void TriggerDeath()
    {
        if (instance != null) onPlayerDie?.Invoke();
    }
    public static void RegisterInstantiator(InstantiateAtMouseClick instantiateAtMouseClick)
    {
        if (instance != null) instance.iamc = instantiateAtMouseClick;
    }

    
    // Initialize on Awake
    private void initialize()
    {

    }

    // 2nd round initialization
    private void Start()
    {
        // Initialization
        chrono = GetComponent<Chrono>();
        onPlayerDie +=  delegate {GameEvents.instance.GameOverTrigger(GameOverReasons.KillBySpike);};

        // Wait for X sec and start the game
        StartCoroutine(startGame());
    }

    public void Update()
    {
        playerAI.maxSpeed = SpeedCurve.Evaluate(chrono.TimeSinceStart);
        cameraShakePower = CameraShakeCurve.Evaluate(chrono.TimeSinceStart);
        if(iamc!=null)
            iamc.SetQueueTime = queueTimeCurve.Evaluate(chrono.TimeSinceStart);
    }

    private IEnumerator startGame()
    {
        yield return new WaitForSeconds(startGameDelay);
        // Start Game
    }

    public void RegisterBlock(List<Collider2D> blockColliders)
    {
        onBlockRegistered?.Invoke(blockColliders);
        iamc.is_created = false;
    }

    /// <summary>
    /// Trigger game over by escape
    /// </summary>
    public void Escape(){
        GameEvents.TriggerGameOver(GameOverReasons.ExitReached);
    }
    


}
