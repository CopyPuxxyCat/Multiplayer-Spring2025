using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WaterSkillController : MonoBehaviourPun, ISkillBlocker
{
    [Header("Skill Prefabs")]
    [SerializeField] GameObject orbPrefab;
    [SerializeField] GameObject slowFieldPrefab;
    [SerializeField] GameObject tidalWavePrefab;
    [SerializeField] GameObject waveAimIndicatorPrefab;

    [Header("Heal Effects")]
    [SerializeField] GameObject healEffect;
    [SerializeField] GameObject shieldEffect;

    [SerializeField] Transform orbSpawnPoint;
    [SerializeField] Transform waveSpawnPoint;

    [SerializeField] GameObject healOrbObject;
    [SerializeField] GameObject[] gunsToDisable;

    private GameObject waveIndicatorInstance;
    private bool isAimingWave = false;

    private bool isHoldingOrb = false;
    private bool isShieldHeal = false;

    private WaterStats waterStats;
    private Camera cam;
    private PlayerController playerController;

    private SkillUIEntry skill1UI;
    private SkillUIEntry skill2UI;
    private SkillUIEntry skill3UI;
    private SkillUIEntry ultimateUI;

    public bool isSkillEnabled { get; set; }

    void Start()
    {
        if (!photonView.IsMine) return;

        waterStats = GetComponent<WaterStats>();
        cam = Camera.main;
        playerController = GetComponent<PlayerController>();
        isSkillEnabled = false;

        var ui = UIController.instance;
        AssignSkillUI(ui.skill1UI, ui.skill2UI, ui.skill3UI, ui.ultimateUI);
    }

    void Update()
    {
        if (!photonView.IsMine || !isSkillEnabled) return;

        if (isAimingWave)
        {
            HandleWaveAiming();
            return;
        }

        if (isHoldingOrb)
        {
            HandleHealMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && skill1UI.CanUse)
            UseOrb();

        if (Input.GetKeyDown(KeyCode.E) && skill2UI.CanUse)
            StartHealMode(false);

        if (Input.GetKeyDown(KeyCode.C) && skill3UI.CanUse)
            StartHealMode(true);

        if (Input.GetKeyDown(KeyCode.R) && ultimateUI.CanUse)
            BeginWaveAim();
    }

    #region Skill 1 - Orb
    void UseOrb()
    {
        skill1UI.TriggerUse();

        GameObject orb = PhotonNetwork.Instantiate("Water/WaterOrb", orbSpawnPoint.position, cam.transform.rotation);
        Rigidbody rb = orb.GetComponent<Rigidbody>();
        rb.velocity = cam.transform.forward * waterStats.orbThrowForce;

        orb.GetComponent<WaterOrb>().Init(waterStats.slowFieldDuration, slowFieldPrefab);
    }
    #endregion

    #region Skill 2 & 3 - Heal Mode
    void StartHealMode(bool isShield)
    {
        if (isShield)
            skill3UI.TriggerUse();
        else
            skill2UI.TriggerUse();

        isHoldingOrb = true;
        isShieldHeal = isShield;

        healOrbObject.SetActive(true);
        foreach (var gun in gunsToDisable)
            gun.SetActive(false);
    }

    void HandleHealMode()
    {
        if (!isHoldingOrb) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ViewportPointToRay(Vector3.one * 0.5f);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    var target = hit.collider.GetComponent<PlayerController>();
                    var targetPV = target.GetComponent<PhotonView>();

                    if (targetPV.IsMine)
                    {
                        if (isShieldHeal) target.AddArmor(waterStats.selfShieldAmount);
                        else target.AddHealth(waterStats.selfHealAmount);
                    }
                    else
                    {
                        if (isShieldHeal)
                            targetPV.RPC("RPC_AddArmor", targetPV.Owner, waterStats.allyShieldAmount);
                        else
                            targetPV.RPC("RPC_AddHealth", targetPV.Owner, waterStats.allyHealAmount);
                    }

                    if (isShieldHeal)
                        targetPV.RPC("PlayShieldEffect", RpcTarget.All);
                    else
                        targetPV.RPC("PlayHealEffect", RpcTarget.All);
                }
            }
            EndHealMode();
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (isShieldHeal)
                photonView.RPC("RPC_AddArmor", RpcTarget.All, waterStats.selfShieldAmount);
            else
                photonView.RPC("RPC_AddHealth", RpcTarget.All, waterStats.selfHealAmount);

            if (isShieldHeal)
                photonView.RPC("PlayShieldEffect", RpcTarget.All);
            else
                photonView.RPC("PlayHealEffect", RpcTarget.All);

            EndHealMode();
        }
    }

    void EndHealMode()
    {
        isHoldingOrb = false;
        healOrbObject.SetActive(false);
        foreach (var gun in gunsToDisable)
            gun.SetActive(true);
    }
    #endregion

    #region Ultimate - Tidal Wave
    void BeginWaveAim()
    {
        isAimingWave = true;
        waveIndicatorInstance = Instantiate(waveAimIndicatorPrefab);

        foreach (var gun in gunsToDisable)
            gun.SetActive(false);
    }

    void HandleWaveAiming()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            waveIndicatorInstance.transform.position = hit.point;
            Vector3 dir = hit.point - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                waveIndicatorInstance.transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        if (Input.GetMouseButtonDown(0))
        {
            isAimingWave = false;
            Destroy(waveIndicatorInstance);
            EndWaveMode();
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAimingWave = false;
            ultimateUI.TriggerUse();

            Vector3 spawnPos = waveIndicatorInstance.transform.position;
            Quaternion spawnRot = waveIndicatorInstance.transform.rotation;
            Destroy(waveIndicatorInstance);

            GameObject wave = PhotonNetwork.Instantiate("Water/TidalWave", spawnPos, spawnRot);
            wave.GetComponent<TidalWave>().Init(spawnRot * Vector3.forward, waterStats.waveSpeed, waterStats.stunDuration);

            EndWaveMode();
        }
    }

    void EndWaveMode()
    {
        foreach (var gun in gunsToDisable)
            gun.SetActive(true);
    }
    #endregion

    public void AssignSkillUI(SkillUIEntry s1, SkillUIEntry s2, SkillUIEntry s3, SkillUIEntry ult)
    {
        skill1UI = s1;
        skill2UI = s2;
        skill3UI = s3;
        ultimateUI = ult;

        skill1UI.Initialize();
        skill2UI.Initialize();
        skill3UI.Initialize();
        ultimateUI.Initialize();
    }

    public bool ShouldBlockShooting => isHoldingOrb || isAimingWave;

    [PunRPC] public void PlayHealEffect() => StartCoroutine(PlayEffectRoutine(healEffect, waterStats.healEffectDuration));
    [PunRPC] public void PlayShieldEffect() => StartCoroutine(PlayEffectRoutine(shieldEffect, waterStats.healEffectDuration));

    IEnumerator PlayEffectRoutine(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        obj.SetActive(true);
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }
}
