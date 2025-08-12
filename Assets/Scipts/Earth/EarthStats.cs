using UnityEngine;

public class EarthStats : MonoBehaviour
{
    [Header("Skill 1 - Smoke (minimap)")]
    public int maxSmokes = 3;
    public string smokeProjectileResourceName = "SmokeProjectile"; // Resources/Earth/SmokeProjectile
    public string smokeAreaResourceName = "EarthSmoke";            // Resources/Earth/EarthSmoke
    public string smokePreviewMiniResourceName = "SmokePreviewOnMiniMap"; // icon on minimap
    public float smokeDuration = 15f;

    [Header("Skill 2 - Golem")]
    public string golemResourceName = "EarthGolem";               // networked prefab name
    public string golemPreviewResourceName = "EarthGolemOverview"; // local preview
    public float golemLifetime = 60f;
    public float golemDetectionRange = 15f;
    public float golemFireRate = 1f;
    public string golemProjectileResourceName = "EarthProjectile";
    public float golemProjectileSpeed = 20f;

    [Header("Skill 3 - Wall")]
    public string wallResourceName = "EarthWall";
    public string wallPreviewResourceName = "WallPreview";
    public float wallLifetime = 20f;
    public float wallRotationStep = 45f;

    [Header("Ultimate - Sandstorm")]
    public string sandstormResourceName = "Sandstorm";
    public float sandstormGrowSpeed = 5f;
    public float sandstormMaxRadius = 15f;
    public float sandstormDuration = 10f;
    public float sandstormSlowAmount = 0.5f; // fraction remaining speed (0.5 = 50% speed)

    [Header("NavMesh")]
    public float navSampleMaxDistance = 3f; // radius for NavMesh.SamplePosition
}
