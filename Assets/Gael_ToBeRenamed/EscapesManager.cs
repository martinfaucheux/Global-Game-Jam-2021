using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EscapesManager : MonoBehaviour
{
    #region singleton
    private static EscapesManager _instance;
    public static EscapesManager instance { get { return _instance; } }

    public void Awake()
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

    [SerializeField] private Transform playerTransform = default;
    [SerializeField] private float detectionWidth = 100f;
    [SerializeField] private float goBackWidth = 10f;

    private List<Escape> escapes = new List<Escape>();

    public static bool HasEscape => instance == null ? false : instance.selectCloseEscapes.Count > 0 ? true : false;
    public static List<Escape> Escapes => instance == null ? null : instance.selectCloseEscapes;

    public static event Action onEscapeRegistered;
    public static event Action onEscapeUnRegistered;

    public void Register(Escape escape)
    {
        escapes.Add(escape);
        onEscapeRegistered?.Invoke();
    }
    public void UnRegister(Escape escape)
    {
        escapes.Remove(escape);
        onEscapeUnRegistered?.Invoke();
    }
    private List<Escape> selectCloseEscapes
    {
        get
        {
            if (playerTransform == null)
                return instance.escapes;
            else
                return instance.escapes.Where(x => 
                Mathf.Abs(x.transform.position.x - playerTransform.position.x) < detectionWidth 
                    && x.transform.position.x  > (playerTransform.position.x - goBackWidth)).ToList();
        }
        
    }
}

