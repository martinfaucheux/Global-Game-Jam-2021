using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quote", menuName = "Quote")]
public class Quote : ScriptableObject
{
    public GameOverReasons gameOverReasons;
    public string quote;
}
