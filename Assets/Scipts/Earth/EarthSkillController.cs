using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;

public class EarthSkillController : MonoBehaviourPun, ISkillBlocker
{
    private EarthStats stats;
    private PlayerController playerController;
    private Camera cam;
    private SkillUIEntry skill1UI, skill2UI, skill3UI, ultimateUI;

    private int remainingSmokes;
    private bool isPlacingSmoke = false;
    private bool isPlacingGolem = false;
    private bool isPlacingWall = false;
    private GameObject golemPreviewInstance;
    private GameObject wallPreviewInstance;

    public bool isSkillEnabled { get; set; }

    void Start()
    {
        if (!photonView.IsMine) return;

        stats = GetComponent<EarthStats>();
        playerController = GetComponent<PlayerController>();
        cam = Camera.main;
        remainingSmokes = stats.maxSmokes;
        isSkillEnabled = false;

        var ui = UIController.instance;
        AssignSkillUI(ui.skill1UI, ui.skill2UI, ui.skill3UI, ui.ultimateUI);
    }

    void Update()
    {
        
        if (!photonView.IsMine || !isSkillEnabled) return;

        // toggle smoke placement (minimap)
        if (Input.GetKeyDown(KeyCode.Q) && skill1UI.CanUse && isPlacingWall == false)
        {
            ToggleSmokePlacement();
        }

        // golem placement (gameview)
        if (Input.GetKeyDown(KeyCode.E) && skill2UI.CanUse && isPlacingWall == false)
        {
            StartGolemPlacement();
        }

        // wall placement
        if (Input.GetKeyDown(KeyCode.C) && skill3UI.CanUse)
        {
            StartWallPlacement();
        }

        // ultimate
        if (Input.GetKeyDown(KeyCode.R) && ultimateUI.CanUse)
        {
            UseSandstorm();
        }
    }

    #region Smoke (minimap)
    void ToggleSmokePlacement()
    {
        isPlacingSmoke = !isPlacingSmoke;
        // show/hide minimap panel (UIController must have ShowEarthMinimap)
        UIController.instance.ShowEarthMinimap(isPlacingSmoke);

        // lock/unlock shooting
        playerController.CallCoroutineToggleCanShoot(!isPlacingSmoke, 0f);
        GameInputManager.Instance.LockInput();

        // clear previous minimap preview icons when turning off
        if (!isPlacingSmoke)
        {
            UIController.instance.earthMinimap.ClearPreviewIcons();
            return;
        }

        // while placing, player will interact with minimap (EarthMinimap handles clicks)
        StartCoroutine(SmokePlacementRoutine());
    }

    IEnumerator SmokePlacementRoutine()
    {
        yield return null;

        var mini = UIController.instance.earthMinimap;
        // Danh sách worldPos các preview
        var previewPositions = new System.Collections.Generic.List<Vector3>();

        while (isPlacingSmoke)
        {
            // Chuột trái: thêm preview
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 clicked = mini.GetLastClickedWorldPosition();
                if (clicked != Vector3.zero && previewPositions.Count < 3)
                {
                    previewPositions.Add(clicked);
                    mini.SpawnPreviewIcon(clicked);
                }
            }

            // Chuột phải: spawn tất cả smoke preview
            if (Input.GetMouseButtonDown(1) && previewPositions.Count > 0)
            {
                foreach (var pos in previewPositions)
                {
                    PhotonNetwork.Instantiate(
                        "Earth/" + stats.smokeProjectileResourceName,
                        pos + Vector3.up * 40f,
                        Quaternion.identity
                    )
                    .GetComponent<EarthSmokeProjectile>()
                    .Init("Earth/" + stats.smokeAreaResourceName, stats.smokeDuration);

                    skill1UI.TriggerUse();
                    remainingSmokes--;
                }

                previewPositions.Clear();
                mini.ClearPreviewIcons();

                isPlacingSmoke = false;
                UIController.instance.ShowEarthMinimap(false);
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
                GameInputManager.Instance.UnlockInput();
            }

            // Q để hủy
            if (Input.GetKeyDown(KeyCode.Q))
            {
                previewPositions.Clear();
                mini.ClearPreviewIcons();
                isPlacingSmoke = false;
                UIController.instance.ShowEarthMinimap(false);
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
                GameInputManager.Instance.UnlockInput();
            }

            yield return null;
        }
    }


    #endregion

    #region Golem (game view, NavMesh sampling)
    void StartGolemPlacement()
    {
        if (isPlacingGolem) return;
        isPlacingGolem = true;
        Debug.Log("check 2: " + isPlacingGolem);
        playerController.CallCoroutineToggleCanShoot(false, 0f);

        // local preview
        if (golemPreviewInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Earth/" + stats.golemPreviewResourceName);
            if (prefab != null) golemPreviewInstance = Instantiate(prefab);
        }

        StartCoroutine(GolemPlacementRoutine());
    }

    IEnumerator GolemPlacementRoutine()
    {
        yield return null;
        while (isPlacingGolem)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, stats.navSampleMaxDistance, NavMesh.AllAreas))
                {
                    if (golemPreviewInstance != null)
                        golemPreviewInstance.transform.position = navHit.position;
                }
            }

            // Chuột trái: confirm spawn (với preview)
            if (Input.GetMouseButtonDown(0) && golemPreviewInstance != null)
            {
                PhotonNetwork.Instantiate("Earth/" + stats.golemResourceName,
                    golemPreviewInstance.transform.position, Quaternion.identity);
                skill2UI.TriggerUse();

                Destroy(golemPreviewInstance);
                golemPreviewInstance = null;
                isPlacingGolem = false;
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }

            // Chuột phải: spawn ngay cả khi không có preview
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 spawnPos = golemPreviewInstance != null
                    ? golemPreviewInstance.transform.position
                    : transform.position + transform.forward * 2f; // fallback gần player

                PhotonNetwork.Instantiate("Earth/" + stats.golemResourceName, spawnPos, Quaternion.identity);
                skill2UI.TriggerUse();

                if (golemPreviewInstance != null) Destroy(golemPreviewInstance);
                golemPreviewInstance = null;
                isPlacingGolem = false;
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (golemPreviewInstance != null) Destroy(golemPreviewInstance);
                golemPreviewInstance = null;
                isPlacingGolem = false;
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }

            yield return null;
        }
    }

    #endregion

    #region Wall (game view, NavMesh sampling + rotation)
    void StartWallPlacement()
    {

        if (isPlacingWall) return;
        isPlacingWall = true;
        Debug.Log("check 3: " + isPlacingWall);
        playerController.CallCoroutineToggleCanShoot(false, 0f);

        if (wallPreviewInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Earth/" + stats.wallPreviewResourceName);
            if (prefab != null) wallPreviewInstance = Instantiate(prefab);
        }

        StartCoroutine(WallPlacementRoutine());
    }

    IEnumerator WallPlacementRoutine()
    {
        yield return null;
        float currentYaw = 0f;
        while (isPlacingWall)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, stats.navSampleMaxDistance, NavMesh.AllAreas))
                {
                    if (wallPreviewInstance != null)
                    {
                        wallPreviewInstance.transform.position = navHit.position;
                        wallPreviewInstance.transform.rotation = Quaternion.Euler(0, currentYaw, 0);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentYaw -= stats.wallRotationStep;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentYaw += stats.wallRotationStep;
            }

            if (Input.GetMouseButtonDown(0))
            {
                // confirm spawn networked wall
                if (wallPreviewInstance != null)
                {
                    Vector3 spawnPos = wallPreviewInstance.transform.position;
                    Quaternion spawnRot = wallPreviewInstance.transform.rotation;
                    PhotonNetwork.Instantiate("Earth/" + stats.wallResourceName, spawnPos, spawnRot);
                    skill2UI.TriggerUse();
                    Destroy(wallPreviewInstance);
                    wallPreviewInstance = null;
                }

                isPlacingWall = false;
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.C))
            {
                // cancel
                if (wallPreviewInstance != null) Destroy(wallPreviewInstance);
                wallPreviewInstance = null;
                isPlacingWall = false;
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }

            yield return null;
        }
    }
    #endregion

    #region Sandstorm (ultimate)
    void UseSandstorm()
    {
        ultimateUI.TriggerUse();
        // spawn at player position (sampled on NavMesh to avoid underground/wrong positions)
        Vector3 p = transform.position;
        if (NavMesh.SamplePosition(p, out NavMeshHit navHit, stats.navSampleMaxDistance, NavMesh.AllAreas))
            PhotonNetwork.Instantiate("Earth/" + stats.sandstormResourceName, navHit.position, Quaternion.identity);
    }
    #endregion

    public void AssignSkillUI(SkillUIEntry s1, SkillUIEntry s2, SkillUIEntry s3, SkillUIEntry ult)
    {
        skill1UI = s1; skill2UI = s2; skill3UI = s3; ultimateUI = ult;
        skill1UI.Initialize(); skill2UI.Initialize(); skill3UI.Initialize(); ultimateUI.Initialize();
    }

    public bool ShouldBlockShooting => isPlacingSmoke || isPlacingGolem || isPlacingWall;
}
