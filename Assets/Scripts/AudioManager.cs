using UnityEngine;

// Plays one-shot SFX and swaps the music loop per game state.
// Any clip left empty is simply skipped.
public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip hop;
    [SerializeField] private AudioClip splash;
    [SerializeField] private AudioClip squash;
    [SerializeField] private AudioClip homeFilled;
    [SerializeField] private AudioClip roundClear;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip timerLow;

    [Header("Music")]
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private AudioClip gameplayMusic;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    private void Awake()
    {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
    }

    public void PlayHop() => PlaySfx(hop);
    public void PlayDeath(bool drowned) => PlaySfx(drowned ? splash : squash);
    public void PlayHomeFilled() => PlaySfx(homeFilled);
    public void PlayRoundClear() => PlaySfx(roundClear);
    public void PlayGameOver() => PlaySfx(gameOver);
    public void PlayTimerLow() => PlaySfx(timerLow);

    public void PlayTitleMusic() => PlayMusic(titleMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);
    public void StopMusic() => _musicSource.Stop();

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (_musicSource.clip == clip && _musicSource.isPlaying) return;

        _musicSource.clip = clip;
        if (clip != null)
        {
            _musicSource.Play();
        }
        else
        {
            _musicSource.Stop();
        }
    }
}
