using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Dynamic Music Sources")]
    [Tooltip("Your main bassline or drum beat")]
    public AudioSource musicSource1;
    [Tooltip("Your melody, synth, or secondary layer")]
    public AudioSource musicSource2;

    [Header("Dynamic Tempo Settings")]
    public float basePitch = 1.0f;       // Normal speed
    public float maxPitch = 1.4f;        // Maximum speed (1.4 is 40% faster!)
    public int maxDifficultyLevel = 20;  // The level where music hits maximum speed
    public float tempoShiftSpeed = 0.5f; // How smoothly the music speeds up or slows down

    [Header("Audio Sources (SFX)")]
    [Tooltip("Dedicated source for sound effects")]
    public AudioSource sfxSource;
    [Tooltip("Dedicated source for pitch shifted sound effects")]
    public AudioSource pitchShiftedSFXSource;
    
    [Header("Player SFX")]
    public AudioClip playerShootSound;
    public AudioClip playerTakeDamageSound;
    public AudioClip railgunSound;
    public AudioClip deathSound;
    
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
    public AudioClip pauseSound;
    public AudioClip unPauseSound;


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

    void Update()
    {
        HandleDynamicTempo();
    }

    void HandleDynamicTempo()
    {
        // Safety check to ensure we have a GameManager to read the level from
        if (GameManager.Instance == null) return;

        // 1. Figure out how close the player is to the "Max Difficulty"
        // (Returns a value between 0.0 and 1.0)
        float difficultyPercentage = Mathf.Clamp01((float)GameManager.Instance.playerLevel / maxDifficultyLevel);

        // 2. Calculate what the pitch SHOULD be based on that percentage
        float targetPitch = Mathf.Lerp(basePitch, maxPitch, difficultyPercentage);

        // 3. Smoothly ramp the actual audio sources to that target pitch
        // (Using Lerp here stops the music from 'snapping' to a new speed instantly)
        if (musicSource1 != null)
        {
            musicSource1.pitch = Mathf.Lerp(musicSource1.pitch, targetPitch, Time.deltaTime * tempoShiftSpeed);
        }
        
        if (musicSource2 != null)
        {
            musicSource2.pitch = Mathf.Lerp(musicSource2.pitch, targetPitch, Time.deltaTime * tempoShiftSpeed);
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

    // A specialized method to play sounds with slightly randomized pitch
    public void PlayPitchShiftSFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && pitchShiftedSFXSource != null)
        {
            pitchShiftedSFXSource.pitch = Random.Range(0.8f, 1.2f);
            pitchShiftedSFXSource.PlayOneShot(clip, volume);
        }
    }
}