using UnityEngine;
using System.Collections;

public class MusicSystem : Singleton<MusicSystem>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioClip mapMusic;
    [SerializeField] private AudioSource secondMusicSource;
    [SerializeField, Min(0f)] private float fadeDuration = 3f;

    private AudioSource currentSource;
    private Coroutine fadeRoutine;

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
        if (musicSource == null || secondMusicSource == null || clip == null)
        {
            Debug.LogWarning("Missing music source or clip.", this);
            return;
        }

        if (musicSource == secondMusicSource)
        {
            Debug.LogWarning("Assign two different music sources.", this);
            return;
        }

        // Do not restart the current track or its ongoing fade.
        if (currentSource != null
            && currentSource.clip == clip
            && currentSource.isPlaying)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        AudioSource incoming = currentSource == musicSource
            ? secondMusicSource
            : musicSource;

        AudioSource outgoing = incoming == musicSource
            ? secondMusicSource
            : musicSource;

        // Preserve playback when returning to a track during a fade.
        if (incoming.clip != clip || !incoming.isPlaying)
        {
            incoming.Stop();
            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();
        }

        bool isFirstTrack = currentSource == null;
        currentSource = incoming;

        if (isFirstTrack || fadeDuration <= 0f)
        {
            incoming.volume = 1f;
            outgoing.volume = 0f;
            outgoing.Stop();
            return;
        }

        fadeRoutine = StartCoroutine(
            CrossfadeRoutine(outgoing, incoming)
        );
    }

    private IEnumerator CrossfadeRoutine(
    AudioSource outgoing,
    AudioSource incoming)
    {
        float elapsed = 0f;
        float duration = fadeDuration;
        float outgoingStartVolume = outgoing.volume;
        float incomingStartVolume = incoming.volume;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            outgoing.volume = Mathf.Lerp(
                outgoingStartVolume,
                0f,
                progress
            );

            incoming.volume = Mathf.Lerp(
                incomingStartVolume,
                1f,
                progress
            );

            yield return null;
        }

        outgoing.volume = 0f;
        outgoing.Stop();
        incoming.volume = 1f;
        fadeRoutine = null;
    }
}
