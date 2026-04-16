using UnityEngine;
using System;

public class MNAlignmentManager : MonoBehaviour
{
    public static MNAlignmentManager Instance;

    public enum PlayTrack { Corporate, HeroDefender, HeroAlien }
    public PlayTrack currentTrack = PlayTrack.Corporate;

    [Header("Alignment Gauge (-100 Alien to +100 Defender)")]
    [Range(-100f, 100f)]
    public float alignmentScore = 0f;
    public float thresholdToGoRogue = 50f; // If you pass +/- 50, you lock into a Hero track
    public bool hasGoneRogue = false;

    [Header("Corporate Currencies (Track 1)")]
    public float corporateMoney = 0f;
    public int corporateFavors = 0;

    [Header("Hero Currencies (Track 2)")]
    public int heroXP = 0;
    public int heroReputation = 0;

    public static event Action<float> OnAlignmentChanged;
    public static event Action<PlayTrack> OnTrackChanged;

    void Awake()
    {
        Instance = this;
    }

    public void EvaluateAction(FactionType factionAffected, bool wasKilled)
    {
        if (hasGoneRogue) return; // Once you pick a side, you can't easily go back to Corporate

        float shiftAmount = wasKilled ? 10f : 2f; // Kills shift you faster than just doing damage

        if (factionAffected == FactionType.Alien)
        {
            // Hurting aliens makes you aligned with the Defenders (+)
            alignmentScore += shiftAmount; 
        }
        else if (factionAffected == FactionType.Defender)
        {
            // Hurting defenders makes you aligned with the Aliens (-)
            alignmentScore -= shiftAmount;
        }

        alignmentScore = Mathf.Clamp(alignmentScore, -100f, 100f);
        OnAlignmentChanged?.Invoke(alignmentScore);

        CheckForTrackShift();
    }

    void CheckForTrackShift()
    {
        if (alignmentScore >= thresholdToGoRogue && currentTrack != PlayTrack.HeroDefender)
        {
            Debug.LogWarning("TRACK SHIFT: You have sided with the Planet! Corporate funding severed.");
            currentTrack = PlayTrack.HeroDefender;
            hasGoneRogue = true;
            OnTrackChanged?.Invoke(currentTrack);
        }
        else if (alignmentScore <= -thresholdToGoRogue && currentTrack != PlayTrack.HeroAlien)
        {
            Debug.LogWarning("TRACK SHIFT: You have sided with the Aliens! Corporate funding severed.");
            currentTrack = PlayTrack.HeroAlien;
            hasGoneRogue = true;
            OnTrackChanged?.Invoke(currentTrack);
        }
    }

    // --- DYNAMIC RELATIONSHIP LOGIC ---
    // The Radar will ask this method what color an enemy should be
    public RelationshipStatus GetRelationship(FactionType entityFaction)
    {
        if (currentTrack == PlayTrack.Corporate)
        {
            // Corporate track: Everyone is neutral until they mess with your bottom line
            return RelationshipStatus.Neutral;
        }
        
        if (currentTrack == PlayTrack.HeroDefender)
        {
            if (entityFaction == FactionType.Defender) return RelationshipStatus.Ally;
            if (entityFaction == FactionType.Alien) return RelationshipStatus.Enemy;
        }
        
        if (currentTrack == PlayTrack.HeroAlien)
        {
            if (entityFaction == FactionType.Alien) return RelationshipStatus.Ally;
            if (entityFaction == FactionType.Defender) return RelationshipStatus.Enemy;
        }

        return RelationshipStatus.Neutral;
    }
}

public enum RelationshipStatus { Ally, Neutral, Enemy }