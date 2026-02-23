using UnityEngine;

public class SoundToggle : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isMuted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("M pressed");
            isMuted = !isMuted;
            audioSource.mute = isMuted;
        }
    }
}