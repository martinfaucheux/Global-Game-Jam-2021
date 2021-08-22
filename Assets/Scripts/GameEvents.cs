using System;
using UnityEngine;

public enum GameOverReasons{
        NoExit,
        ExitReached,
        KillBySpike,
    }

public class GameEvents : MonoBehaviour
{
    
    #region singleton
    private static GameEvents _instance;
    public static GameEvents instance { get { return _instance; } }

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
    }
    #endregion

    

    public event Action onPutBlock;
    public event Action<GameOverReasons> onGameOver;

    public static void TriggerGameOver(GameOverReasons gameOverReason)
    {
        if (instance != null) instance.GameOverTrigger(gameOverReason);
    }

    public void GameOverTrigger(GameOverReasons gameOverReason)
    {
        onGameOver?.Invoke(gameOverReason);
    }

    public void PutBlockTrigger()
    {
        if (onPutBlock != null)
        {
            onPutBlock();
        }
    }

}
