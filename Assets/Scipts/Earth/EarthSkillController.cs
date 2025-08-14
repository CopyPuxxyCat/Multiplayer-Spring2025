using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

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
        
        if (!photonView.IsMine || !isSkillEnabled || ChatUIManager.Instance.isTyping) return;

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

    // … phần trên giữ nguyên …

    #region Smoke (minimap)
    void ToggleSmokePlacement()
    {
        isPlacingSmoke = !isPlacingSmoke;

        UIController.instance.ShowEarthMinimap(isPlacingSmoke);

        // chặn bắn & khoá input gameplay ở ngoài minimap
        playerController.CallCoroutineToggleCanShoot(!isPlacingSmoke, 0f);
        if (isPlacingSmoke) GameInputManager.Instance.LockInput();
        else GameInputManager.Instance.UnlockInput();

        // clear cũ
        UIController.instance.earthMinimap.ClearPreviewIcons();
        if (!isPlacingSmoke) return;

        // bật cursor icon trên minimap
        UIController.instance.earthMinimap.ShowCursor(true);

        StartCoroutine(SmokePlacementRoutine());
    }

    IEnumerator SmokePlacementRoutine()
    {
        yield return null;
        var mini = UIController.instance.earthMinimap;
        var pendingProjectiles = new System.Collections.Generic.List<EarthSmokeProjectile>();

        // số điểm tối đa = min(3, remainingSmokes)
        int maxPoints = Mathf.Min(3, remainingSmokes);

        while (isPlacingSmoke)
        {
            // TRÁI: đặt 1 điểm
            if (Input.GetMouseButtonDown(0) && mini.IsPointerOverMinimap())
            {
                if (pendingProjectiles.Count < maxPoints &&
                    mini.TryGetCursorWorld(out Vector3 groundPos, 0f))
                {
                    // 1) tạo icon preview bám world
                    mini.AddPreviewIcon(groundPos);

                    // 2) spawn projectile treo trên trời tại vị trí "cùng phương" với camera minimap
                    float dropStartY = mini.minimapCamera.transform.position.y; // ngang với cao độ camera minimap
                    Vector3 spawn = new Vector3(groundPos.x, dropStartY, groundPos.z);

                    var go = Photon.Pun.PhotonNetwork.Instantiate("Earth/" + stats.smokeProjectileResourceName, spawn, Quaternion.identity);
                    var proj = go.GetComponent<EarthSmokeProjectile>();
                    if (proj == null) proj = go.AddComponent<EarthSmokeProjectile>();
                    // init: path smoke + thời gian + groundMask từ minimap
                    proj.Init("Earth/" + stats.smokeAreaResourceName, stats.smokeDuration, mini.groundMask);

                    pendingProjectiles.Add(proj);
                }
            }

            // PHẢI: thả tất cả
            if (Input.GetMouseButtonDown(1) && pendingProjectiles.Count > 0)
            {
                foreach (var p in pendingProjectiles)
                    if (p != null) p.ArmAndDrop(5f); // rơi nhanh 1 chút

                // tiêu hao UI usage theo số lượng
                for (int i = 0; i < pendingProjectiles.Count; i++)
                {
                    skill1UI.TriggerUse();
                    remainingSmokes = Mathf.Max(0, remainingSmokes - 1);
                }

                // dọn & thoát
                pendingProjectiles.Clear();
                mini.ClearPreviewIcons();
                mini.ShowCursor(false);

                isPlacingSmoke = false;
                UIController.instance.ShowEarthMinimap(false);
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
                GameInputManager.Instance.UnlockInput();
            }

            // Q: huỷ
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // hủy toàn bộ projectile chưa thả
                foreach (var p in pendingProjectiles)
                {
                    if (p != null && p.TryGetComponent<Photon.Pun.PhotonView>(out var pv) && pv.IsMine)
                        Photon.Pun.PhotonNetwork.Destroy(p.gameObject);
                    else if (p != null)
                        Destroy(p.gameObject);
                }
                pendingProjectiles.Clear();

                mini.ClearPreviewIcons();
                mini.ShowCursor(false);

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
                    GameObject wall = PhotonNetwork.Instantiate("Earth/" + stats.wallResourceName, spawnPos, spawnRot);
                    wall.GetComponent<EarthWall>().Init(stats.wallLifetime);
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
