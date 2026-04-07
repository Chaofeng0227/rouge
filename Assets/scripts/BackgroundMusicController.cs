using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicController : MonoBehaviour
{
    private const string MusicResourcePath = "Music/ArcadeGameBGM3Loop";

    private static BackgroundMusicController instance;
    private static AudioClip cachedMusic;

    [SerializeField] private float musicVolume = 0.28f;

    public static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject musicObject = new GameObject("BackgroundMusicController");
        instance = musicObject.AddComponent<BackgroundMusicController>();
        DontDestroyOnLoad(musicObject);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAudio();
    }

    void InitializeAudio()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = musicVolume;

        if (cachedMusic == null)
        {
            cachedMusic = Resources.Load<AudioClip>(MusicResourcePath);
        }

        if (cachedMusic == null)
        {
            Debug.LogWarning("Background music clip not found at Resources/" + MusicResourcePath);
            return;
        }

        if (audioSource.clip != cachedMusic)
        {
            audioSource.clip = cachedMusic;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
