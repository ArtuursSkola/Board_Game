using UnityEngine;

public class SoundEffectScript : MonoBehaviour
{

    public AudioClip[] soundEffects;
    public AudioSource audioSource;

    public void Hover(){
        audioSource.PlayOneShot(soundEffects[0]);
    }
    public void Click(){
        audioSource.PlayOneShot(soundEffects[1]);
    }

    public void OnDice(){
        audioSource.loop = true;
        audioSource.clip = soundEffects[2];
        audioSource.Play();
    }

    public void CancelButton(){
        audioSource.PlayOneShot(soundEffects[3]);
    }

    public void PlayButton(){
        audioSource.PlayOneShot(soundEffects[4]);
    }

    public void NameField(){
        audioSource.PlayOneShot(soundEffects[5]);
    }
    public void FunnySound()
    {
        audioSource.PlayOneShot(soundEffects[6]);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
