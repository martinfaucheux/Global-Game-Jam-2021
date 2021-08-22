using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        GameEvents.instance.onPutBlock += PlayPutBlockSound;
    }

    public void PlayPutBlockSound(){
        _audioSource.pitch = (Random.Range(0.6f, 1.1f));
        _audioSource.Play(0);
    }

}
