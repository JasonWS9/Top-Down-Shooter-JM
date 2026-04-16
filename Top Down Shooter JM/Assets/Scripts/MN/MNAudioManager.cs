using UnityEngine;

public class MNAudioManager : MonoBehaviour
{
    public static MNAudioManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void PlaySFX()
    {
        // Add AudioSource logic here
    }
}