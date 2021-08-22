using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameOverFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public float fadeInTransitionDuration = 3f;
    public List<Quote> quoteList;
    public Text​Mesh​Pro​UGUI textComponent;
    public Text​Mesh​Pro​UGUI quoteTextComponent;
    
    public Animator pressRForRestartAnimator;

    void Start(){
        GameEvents.instance.onGameOver += FadeIn;
    }

    public void FadeIn(GameOverReasons gameOverReason){

        if (textComponent != null){
            string text;
            switch(gameOverReason){
                case GameOverReasons.NoExit:
                    text = "There is no way out";
                    break;
                case GameOverReasons.ExitReached:
                    text = "John escaped";
                    break;
                case GameOverReasons.KillBySpike:
                    text = "John has been impaled";
                    break;
                default:
                    text = "GAME OVER";
                    break;
            }
            textComponent.text = text;
        }

        quoteTextComponent.text = GetRandomQuote(gameOverReason);

        StartCoroutine(FadeInCoroutine());
        StartCoroutine(ShowPressR());
    }

    private IEnumerator FadeInCoroutine(){
        float timeSinceStart = 0f;

        while(timeSinceStart <= fadeInTransitionDuration){

            canvasGroup.alpha = (timeSinceStart / fadeInTransitionDuration);          
            timeSinceStart += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private string GetRandomQuote(GameOverReasons gameOverReason){
        string quoteString = "";
        List<Quote> reasonQuotes = new List<Quote>();
        
        foreach(Quote quote in quoteList){
            if(quote.gameOverReasons == gameOverReason)
                reasonQuotes.Add(quote);
        }
        int listLength = reasonQuotes.Count;
        if (listLength > 0){
            int index = Random.Range(0, listLength);
            return reasonQuotes[index].quote;
        }

        return quoteString;
    }

    private IEnumerator ShowPressR(){
        yield return new WaitForSecondsRealtime(6);
        pressRForRestartAnimator.SetTrigger("show");
    }
}
