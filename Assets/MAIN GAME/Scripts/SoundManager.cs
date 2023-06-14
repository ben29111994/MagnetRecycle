using UnityEngine;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance;

	public AudioSource audioSource;
    public AudioClip hit, win, lose, cash, button;
    float pitchTimeOut = 1;

    void Awake(){
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (pitchTimeOut > 0)
        {           
            pitchTimeOut -= 0.05f;
        }
        else
        {
            audioSource.pitch = 1;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        //audioSource.PlayOneShot(clip);
    }

    public void PlaySoundPitch(AudioClip clip)
    {
        //pitchTimeOut = 2;
        //audioSource.pitch += 0.001f;
        //audioSource.PlayOneShot(clip);
    }
}
