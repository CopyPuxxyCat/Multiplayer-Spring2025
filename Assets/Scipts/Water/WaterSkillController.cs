using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WaterSkillController : MonoBehaviourPun, ISkillBlocker
{
    [Header("Skill Prefabs")]
    [SerializeField] GameObject orbPrefab;
    [SerializeField] GameObject slowFieldPrefab;
    [SerializeField] GameObject tidalWavePrefab;

    [Header("Heal Effects")]
    [SerializeField] GameObject healEffect;
    [SerializeField] GameObject shieldEffect;

    [Header("Transforms")]
    [SerializeField] Transform orbSpawnPoint;
    [SerializeField] Transform waveSpawnPoint;

    [Header("Heal Orb")]
    [SerializeField] GameObject healOrbObject;
    [SerializeField] GameObject[] gunsToDisable;

    [Header("Ghost Path")]
    [SerializeField] LineRenderer ghostPathLine;
    [SerializeField] float maxWaveDistance = 30f;
    [SerializeField] LayerMask groundLayer;

    private bool isAimingWave = false;
    private bool isHoldingOrb = false;
    private bool isShieldHeal = false;

    private WaterStats waterStats;
    private Camera cam;
    private PlayerController playerController;

    private SkillUIEntry skill1UI, skill2UI, skill3UI, ultimateUI;

    public bool isSkillEnabled { get; set; }

    private void Awake()
    {
        waterStats = GetComponent<WaterStats>();
    }

    void Start()
    {
        if (!photonView.IsMine) return;

        cam = Camera.main;
        playerController = GetComponent<PlayerController>();
        isSkillEnabled = false;

        var ui = UIController.instance;
        AssignSkillUI(ui.skill1UI, ui.skill2UI, ui.skill3UI, ui.ultimateUI);
    }

    void Update()
    {
        if (!photonView.IsMine || !isSkillEnabled || ChatUIManager.Instance.isTyping) return;

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

        if (Input.GetKeyDown(KeyCode.Q) && skill1UI.CanUse) UseOrb();
        if (Input.GetKeyDown(KeyCode.E) && skill2UI.CanUse) StartHealMode(false);
        if (Input.GetKeyDown(KeyCode.C) && skill3UI.CanUse) StartHealMode(true);
        if (Input.GetKeyDown(KeyCode.R) && ultimateUI.CanUse) BeginWaveAim();
    }

    #region Skill 1 - Orb
    void UseOrb()
    {
        skill1UI.TriggerUse();
        var orb = PhotonNetwork.Instantiate("Water/WaterOrb", orbSpawnPoint.position, cam.transform.rotation);
        orb.GetComponent<Rigidbody>().velocity = cam.transform.forward * waterStats.orbThrowForce;
        orb.GetComponent<WaterOrb>().Init(waterStats.slowFieldDuration, slowFieldPrefab);
    }
    #endregion

    #region Skill 2 & 3 - Heal
    void StartHealMode(bool isShield)
    {
        if (isShield) skill3UI.TriggerUse();
        else skill2UI.TriggerUse();

        isHoldingOrb = true;
        isShieldHeal = isShield;
        healOrbObject.SetActive(true);
        foreach (var gun in gunsToDisable) gun.SetActive(false);
    }

    void HandleHealMode()
    {
        if (!isHoldingOrb) return;

        if (Input.GetMouseButtonDown(0)) // Heal đồng đội
        {
            Ray ray = cam.ViewportPointToRay(Vector3.one * 0.5f);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    var target = hit.collider.GetComponent<PlayerController>();
                    var pv = target.GetComponent<PhotonView>();

                    if (pv.IsMine)
                    {
                        if (isShieldHeal) target.AddArmor(waterStats.selfShieldAmount);
                        else target.AddHealth(waterStats.selfHealAmount);
                    }
                    else
                    {
                        if (isShieldHeal) pv.RPC("RPC_AddArmor", pv.Owner, waterStats.allyShieldAmount);
                        else pv.RPC("RPC_AddHealth", pv.Owner, waterStats.allyHealAmount);
                    }

                    if (isShieldHeal) pv.RPC("PlayShieldEffect", RpcTarget.All);
                    else pv.RPC("PlayHealEffect", RpcTarget.All);
                }
            }
            EndHealMode();
        }

        if (Input.GetMouseButtonDown(1)) // Heal bản thân
        {
            if (isShieldHeal) photonView.RPC("RPC_AddArmor", photonView.Owner, waterStats.selfShieldAmount);
            else photonView.RPC("RPC_AddHealth", photonView.Owner, waterStats.selfHealAmount);

            if (isShieldHeal) photonView.RPC("PlayShieldEffect", RpcTarget.All);
            else photonView.RPC("PlayHealEffect", RpcTarget.All);

            EndHealMode();
        }
    }

    void EndHealMode()
    {
        isHoldingOrb = false;
        healOrbObject.SetActive(false);

        playerController.SwitchGun();
    }
    #endregion

    #region Ultimate - Ghost Path
    void BeginWaveAim()
    {
        isAimingWave = true;
        ghostPathLine.enabled = true;
        foreach (var gun in gunsToDisable) gun.SetActive(false);
    }

    void HandleWaveAiming()
    {
        Vector3 start = waveSpawnPoint.position;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 end = hit.point;
            Vector3 dir = (end - start).normalized;
            end = start + dir * Mathf.Min(Vector3.Distance(start, end), maxWaveDistance);

            ghostPathLine.SetPosition(0, start + Vector3.up * 0.1f);
            ghostPathLine.SetPosition(1, end + Vector3.up * 0.1f);
        }

        if (Input.GetMouseButtonDown(0)) // Hủy
        {
            EndWaveAim();
        }

        if (Input.GetMouseButtonDown(1)) // Thi triển
        {
            ultimateUI.TriggerUse();
            Vector3 direction = (ghostPathLine.GetPosition(1) - ghostPathLine.GetPosition(0)).normalized;
            Vector3 spawnPos = ghostPathLine.GetPosition(0);

            var wave = PhotonNetwork.Instantiate("Water/TidalWave", spawnPos, Quaternion.LookRotation(direction));
            wave.GetComponent<TidalWave>().Init(direction, waterStats.waveSpeed, waterStats.stunDuration);

            EndWaveAim();
        }
    }

    void EndWaveAim()
    {
        isAimingWave = false;
        ghostPathLine.enabled = false;
        playerController.SwitchGun();
    }
    #endregion

    #region RPC & Utility
    public void AssignSkillUI(SkillUIEntry s1, SkillUIEntry s2, SkillUIEntry s3, SkillUIEntry ult)
    {
        skill1UI = s1; skill2UI = s2; skill3UI = s3; ultimateUI = ult;
        skill1UI.Initialize(); skill2UI.Initialize(); skill3UI.Initialize(); ultimateUI.Initialize();
    }

    public bool ShouldBlockShooting => isHoldingOrb || isAimingWave;

    [PunRPC]
    public void PlayHealEffect()
    {
        if (healEffect == null || waterStats == null) return;
        StartCoroutine(PlayEffectRoutine(healEffect, waterStats.healEffectDuration));
    }

    [PunRPC]
    public void PlayShieldEffect()
    {
        if (shieldEffect == null || waterStats == null) return;
        StartCoroutine(PlayEffectRoutine(shieldEffect, waterStats.healEffectDuration));
    }

    IEnumerator PlayEffectRoutine(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        obj.SetActive(true);
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }
    #endregion
}
