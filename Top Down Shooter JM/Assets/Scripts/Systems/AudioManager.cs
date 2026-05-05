using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [Tooltip("Dedicated source for background music")]
    public AudioSource musicSource;
    [Tooltip("Dedicated source for sound effects")]
    public AudioSource sfxSource;

    [Header("Player SFX")]
    public AudioClip playerShootSound;
    public AudioClip playerTakeDamageSound;
    public AudioClip railgunSound;

    [Header("Enemy SFX")]
    public AudioClip singleShotSound;
    public AudioClip burstShotSound;
    public AudioClip rammerDashSound;
    public AudioClip enemySpawnSound;
    public AudioClip enemyHitSound;
    public AudioClip enemyDeathExplosionSound;

    [Header("Misc SFX")]
    public AudioClip powerupPickupSound;
    public AudioClip levelUpSound;

    void Awake()
    {
        // Standard Singleton Setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // A simple, universal method to play any sound effect once
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}