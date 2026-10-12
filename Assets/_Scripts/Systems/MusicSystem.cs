using UnityEngine;

public class MusicSystem : Singleton<MusicSystem>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioClip mapMusic;

    public void PlayDefaultMusic()
    {
        PlayMusic(defaultMusic);
    }

    public void PlayMapMusic()
    {
        PlayMusic(mapMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
        {
            Debug.LogWarning("Missing music source or clip.", this);
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
    }
}
