using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

public class JettController : MonoBehaviourPun, ISkillBlocker
{
    public bool isDashing { get; private set; } = false;
    public bool isThrowingSmoke { get; private set; } = false;
    public bool isUpdrafting { get; private set; } = false;
    public bool isFalling { get; private set; } = false;
    public bool ShouldBlockShooting => isUsingUltimate;

    [SerializeField] Camera playerCamera;
    [SerializeField] GameObject smokeBallPrefab;
    [SerializeField] Transform smokeFiringTransform;

    [Header("Ultimate")]
    [SerializeField] private GameObject knifeObject_Local;
    [SerializeField] private GameObject knifeObject_Remote;
    [SerializeField] private Transform daggerSpawnPoint;
    [SerializeField] private GameObject daggerPrefab;

    private bool isUsingUltimate = false;

    [Header("Skill UI")]
    private SkillUIEntry dashUI;
    private SkillUIEntry smokeUI;
    private SkillUIEntry updraftUI;
    private SkillUIEntry ultimateUI;

    public bool isSkillEnabled { get; set; }

    private int dashAttempts = 0;
    private float dashStartTime = 0f;

    JettSmokeProjectile currentSmokeProjectile;
    private int smokeAttempts = 0;
    private float lastTimeSmokeEnded = 0f;

    private int updraftAttempts = 0;
    private float lastTimeUpdrafted = 0.0f;

    private float lastYVelocity = 0f;

    private PlayerController playerController;
    private JettStats jettStats;

    private const float DefaultGravity = -24.525f;
    private float currentGravity = DefaultGravity;
    private Vector3 velocity;
    private bool isGrounded = true;

    [Header("Effect")]
    [SerializeField] GameObject dashEffect;
    [SerializeField] GameObject updraftEffect;
    [SerializeField] GameObject jetUltimateEffect;

    private void Awake()
    {
        jettStats = GetComponent<JettStats>();
    }

    void Start()
    {
        if (!photonView.IsMine) return;

        isSkillEnabled = false;

        playerController = GetComponent<PlayerController>();
        playerCamera = Camera.main;

        var ui = UIController.instance;
        AssignSkillUI(ui.skill1UI, ui.skill2UI, ui.skill3UI, ui.ultimateUI);

        if (knifeObject_Local != null)
            knifeObject_Local.SetActive(false);
        if (knifeObject_Remote != null)
            knifeObject_Remote.SetActive(false);
    }

    void Update()
    {
        if (!photonView.IsMine || !isSkillEnabled || ChatUIManager.Instance.isTyping) return;

        CheckIsFalling();

        if (Input.GetKeyDown(KeyCode.R) && !isUsingUltimate && ultimateUI.CanUse)
        {
            photonView.RPC(nameof(SetUltimateActive), RpcTarget.All, true);
        }

        if (isUsingUltimate)
        {
            if (Input.GetMouseButtonDown(0) && ultimateUI.CanUse)
            {
                FireDagger();
            }
        }
        else
        {
            HandleDash();
            HandleSmoke();
            HandleUpdraft();
        }

        HandleFloat();
        ApplyGravity();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (isDashing)
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            Vector3 moveDir = input == Vector3.zero ? transform.forward : transform.TransformDirection(input.normalized);
            playerController.characterController.Move(moveDir * jettStats.dashSpeed * Time.fixedDeltaTime);

            if (Time.time - dashStartTime > jettStats.dashDurationSeconds)
            {
                EndDash();
            }
        }
    }

    void CheckIsFalling()
    {
        isGrounded = playerController.characterController.isGrounded;
        float yVel = velocity.y;
        isFalling = !isGrounded && yVel <= 0 && yVel < lastYVelocity;
        lastYVelocity = yVel;
    }

    void ApplyGravity()
    {
        if (!isGrounded && !isDashing)
        {
            velocity.y += currentGravity * Time.deltaTime;
            playerController.characterController.Move(velocity * Time.deltaTime);
        }
        else if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    #region Dash
    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isDashing && dashAttempts < jettStats.maxDashAttempts)
        {
            photonView.RPC(nameof(RPC_StartDash), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_StartDash()
    {
        if (!photonView.IsMine || !dashUI.CanUse) return;

        dashUI.TriggerUse();
        photonView.RPC("PlayDashEffect", RpcTarget.All);
        isDashing = true;
        dashStartTime = Time.time;
        dashAttempts++;
    }

    void EndDash()
    {
        isDashing = false;
        dashStartTime = 0f;
    }
    #endregion

    #region Smoke
    void HandleSmoke()
    {
        if (Input.GetKeyDown(KeyCode.C) && smokeAttempts < jettStats.maxSmokeAttempts)
        {
            if (Time.time - lastTimeSmokeEnded >= jettStats.smokeDelaySeconds)
            {
                photonView.RPC(nameof(RPC_ThrowSmoke), RpcTarget.All, smokeFiringTransform.position, playerCamera.transform.rotation);
            }
        }

        if (isThrowingSmoke && currentSmokeProjectile != null)
        {
            currentSmokeProjectile.SetIsControlled(Input.GetKey(KeyCode.C));

            if (Input.GetKeyUp(KeyCode.C))
            {
                OnThrowingSmokeEnd();
            }
        }
    }

    [PunRPC]
    void RPC_ThrowSmoke(Vector3 spawnPos, Quaternion camRot)
    {
        if (!photonView.IsMine || !smokeUI.CanUse) return;

        smokeUI.TriggerUse();
        GameObject smoke = PhotonNetwork.Instantiate("Jett/JettSmokeProjectile", spawnPos, camRot);
        currentSmokeProjectile = smoke.GetComponent<JettSmokeProjectile>();
        currentSmokeProjectile.Initialize(false, playerCamera);

        currentSmokeProjectile.OnExplode = (pos, rot) =>
        {
            photonView.RPC(nameof(RPC_CreateSmokeBall), RpcTarget.All, pos, rot);
        };

        isThrowingSmoke = true;
        smokeAttempts++;
    }

    void OnThrowingSmokeEnd()
    {
        lastTimeSmokeEnded = Time.time;
        isThrowingSmoke = false;
        if (currentSmokeProjectile != null)
        {
            currentSmokeProjectile.SetIsControlled(false);
            currentSmokeProjectile = null;
        }
    }

    [PunRPC]
    void RPC_CreateSmokeBall(Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.Instantiate("Jett/JettSmokeBall", position, rotation);
    }
    #endregion

    #region Updraft
    void HandleUpdraft()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time - lastTimeUpdrafted >= jettStats.updraftDelaySeconds && updraftAttempts < jettStats.maxUpdraftAttempts)
        {
            photonView.RPC(nameof(RPC_Updraft), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_Updraft()
    {
        if (!photonView.IsMine || !updraftUI.CanUse) return;

        updraftUI.TriggerUse();
        photonView.RPC("PlayUpdrafEffect", RpcTarget.All);
        isUpdrafting = true;
        lastTimeUpdrafted = Time.time;
        updraftAttempts++;

        float upVelocity = playerController.characterController.isGrounded ?
            Mathf.Sqrt(jettStats.updraftHeight * -2f * currentGravity) :
            Mathf.Sqrt((jettStats.updraftHeight / 2.5f) * -2f * currentGravity);

        velocity.y = upVelocity;
    }
    #endregion

    #region Ultimate
    [PunRPC]
    public void SetUltimateActive(bool isActive)
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();

        isUsingUltimate = isActive;

        if (photonView.IsMine)
        {
            photonView.RPC("PlayJetUltimateEffect", RpcTarget.All);
            if (isActive == true)
            {
                foreach (Gun gun in playerController.AllGuns)
                    gun.gameObject.SetActive(!isActive);
                playerController.CallCoroutineToggleCanShoot(false, 0f);
            }    
            else
            {
                playerController.SwitchGun();
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }

            if (knifeObject_Local != null)
                knifeObject_Local.SetActive(isActive);
        }
        else
        {
            if (knifeObject_Remote != null)
                knifeObject_Remote.SetActive(isActive);
        }
    }

    private void FireDagger()
    {
        if (!ultimateUI.CanUse) return;

        ultimateUI.TriggerUse();

        GameObject dagger = PhotonNetwork.Instantiate("Dagger", daggerSpawnPoint.position, daggerSpawnPoint.rotation);

        Rigidbody rb = dagger.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = daggerSpawnPoint.forward * 30f;
        }

        if (!ultimateUI.CanUse && ultimateUI.RemainingCharges == 0)
        {
            photonView.RPC(nameof(SetUltimateActive), RpcTarget.All, false);
        }
    }
    #endregion

    #region Float
    void HandleFloat()
    {
        currentGravity = (isFalling && Input.GetKey(KeyCode.Space)) ? JettStats.FloatingGravity : DefaultGravity;
    }
    #endregion

    #region Helper
    public void AssignSkillUI(SkillUIEntry up, SkillUIEntry dash, SkillUIEntry smoke, SkillUIEntry ult)
    {
        dashUI = dash;
        smokeUI = smoke;
        updraftUI = up;
        ultimateUI = ult;

        dashUI.Initialize();
        smokeUI.Initialize();
        updraftUI.Initialize();
        ultimateUI.Initialize();
    }

    float effectDuration = 1f;

    [PunRPC]
    public void PlayDashEffect()
    {
        if (dashEffect == null || jettStats == null) return;
        StartCoroutine(PlayEffectRoutine(dashEffect, effectDuration));
    }
    [PunRPC]
    public void PlayUpdrafEffect()
    {
        if (updraftEffect == null || jettStats == null) return;
        StartCoroutine(PlayEffectRoutine(updraftEffect, effectDuration));
    }
    [PunRPC]
    public void PlayJetUltimateEffect()
    {
        if (jetUltimateEffect == null || jettStats == null) return;
        StartCoroutine(PlayEffectRoutine(jetUltimateEffect, effectDuration));
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
