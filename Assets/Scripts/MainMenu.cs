using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public TextMeshProUGUI bestTimeTextComponent;
    // Start is called before the first frame update
    void Start()
    {
        SetBestScore();
    }

    public void StartGame(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame(){
        Application.Quit();
    }

    private void SetBestScore(){
        if (PlayerPrefs.HasKey(Chrono.playerPrefChronoKey)){
            float bestTime = PlayerPrefs.GetFloat(Chrono.playerPrefChronoKey);
            bestTimeTextComponent.text = "Best score:\n" + Chrono.FormatTime(bestTime);
        }
    }


}
