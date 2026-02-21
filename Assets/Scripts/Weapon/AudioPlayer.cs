using UnityEngine;
using UnityEngine.Rendering;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioCollection[] audioFiles;

    [System.Serializable]
    public class AudioCollection
    {
        public string name;
        public float volume = 1f;
        public float variation = 0.2f;
        public float pitch = 1f;
        public bool Sound3D = true;
        public AudioClip[] values;
    }

    private AudioSource player;

    void Awake()
    {
        player = GetComponent<AudioSource>();
    }

    public void Play(string name)
    {
        if (audioFiles == null || audioFiles.Length == 0)
        {
            Debug.LogError("Empty audio array.");
            return;
        }

        AudioCollection collection = System.Array.Find(audioFiles, x => x.name == name);
        float Sound3D = 0f;
        if (collection.Sound3D) Sound3D = 1f;

        if (collection == null)
        {
            Debug.LogError("No audio collection found with name: "+name);
            return;
        }

        if (collection.values == null || collection.values.Length == 0)
        {
            Debug.LogError("Audio collection " + name + " has no clips.");
            return;
        }

        AudioClip randomAudio = collection.values[
            Random.Range(0, collection.values.Length)
        ];
        if(collection.variation != 0f)
        {
            player.pitch = Random.Range(1f - collection.variation, 1f + collection.variation);
        } else
        {
            player.pitch = collection.pitch;
        }
        player.spatialBlend = Sound3D;
        player.volume = collection.volume;
        player.PlayOneShot(randomAudio);
    }
}