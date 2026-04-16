using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MNRadarVisualizer : MonoBehaviour
{
    private LineRenderer line;
    private MNEnemyController parentEnemy;
    
    [Header("Visual Settings")]
    public int segments = 50;
    public float lineWidth = 0.1f;
    
    [Header("Colors")]
    public Color allyColor = Color.green;
    public Color neutralColor = Color.yellow;
    public Color enemyColor = Color.red;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        parentEnemy = GetComponentInParent<MNEnemyController>();

        // Setup the Line Renderer properties
        line.useWorldSpace = false; // Important so it moves with the enemy!
        line.positionCount = segments + 1;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        
        // Use a basic unlit material so it glows in the dark space background
        line.material = new Material(Shader.Find("Sprites/Default"));
        
        DrawRadarCircle();
    }

    void Update()
    {
        UpdateRadarColor();
    }

    void DrawRadarCircle()
    {
        if (parentEnemy == null || parentEnemy.enemyData == null) return;

        // Grab the exact radar range you set in the ScriptableObject!
        float radius = parentEnemy.enemyData.radarRange; 
        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / segments);
        }
    }

    void UpdateRadarColor()
    {
        if (MNAlignmentManager.Instance == null || parentEnemy == null) return;

        // Ask the Alignment Brain what color this enemy should be right now
        RelationshipStatus status = MNAlignmentManager.Instance.GetRelationship(parentEnemy.enemyData.faction);

        Color targetColor = neutralColor;
        switch (status)
        {
            case RelationshipStatus.Ally: targetColor = allyColor; break;
            case RelationshipStatus.Enemy: targetColor = enemyColor; break;
            case RelationshipStatus.Neutral: targetColor = neutralColor; break;
        }

        // Apply a slight transparency so it doesn't block the art
        targetColor.a = 0.3f; 
        line.startColor = targetColor;
        line.endColor = targetColor;
    }
}