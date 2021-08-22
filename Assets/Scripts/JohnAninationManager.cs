using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JohnAnimationManager : MonoBehaviour
{
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        GameEvents.instance.onGameOver += SetAnimationIdle;
    }

    public void SetAnimationIdle(GameOverReasons ignored){
        _animator.SetBool("is_running", false);
    }
}
