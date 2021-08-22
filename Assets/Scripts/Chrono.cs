using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chrono : MonoBehaviour
{
    public static string playerPrefChronoKey = "best_time";
    public TextMeshProUGUI chronoGUITextComponent;

    private float timeSinceStart = 0f;
    public float TimeSinceStart => timeSinceStart;
    private bool _isChronoEnabled = true;


    void Start()
    {
        GameEvents.instance.onGameOver += SaveCurrentTime;
    }

    void Update()
    {
        if (_isChronoEnabled){
            timeSinceStart += Time.deltaTime;

            if (chronoGUITextComponent != null){
                chronoGUITextComponent.text = FormatTime(timeSinceStart);
            }
        }
    }

    public void SaveCurrentTime(GameOverReasons uh){

        _isChronoEnabled = false;
        float bestTime = 0f;

        if (PlayerPrefs.HasKey(playerPrefChronoKey)){
            bestTime = PlayerPrefs.GetFloat(playerPrefChronoKey);
        }

        if (timeSinceStart > bestTime){
            PlayerPrefs.SetFloat(playerPrefChronoKey, timeSinceStart);
        }
    }

    public static string FormatTime(float time){
        string seconds = Mathf.Floor((time % 60f)).ToString("00");
        string minutes = Mathf.Floor((time % 3600f) / 60f).ToString("00");
        return minutes + ":" + seconds;
    }
}
