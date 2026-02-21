using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] AudioClip[] audioFiles;
    public void playSoundWithVariation(float variation = 0.2f)
    {
        if(audioFiles.Length == 0)
        {
            Debug.LogError("No audiofiles selected.");
            return;
        }
        AudioClip randomAudio = audioFiles[Random.Range(0, audioFiles.Length)];
        AudioSource player = GetComponent<AudioSource>();
        player.pitch = Random.Range(1f-variation, 1f+variation);
        player.PlayOneShot(randomAudio);
    }
}
