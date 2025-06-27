using UnityEngine;
using Photon.Pun;

public class JettController : MonoBehaviourPun
{
    public bool isDashing { get; private set; } = false;
    public bool isThrowingSmoke { get; private set; } = false;
    public bool isUpdrafting { get; private set; } = false;
    public bool isFalling { get; private set; } = false;

    [SerializeField] Camera playerCamera;
    [SerializeField] GameObject smokeBallPrefab;
    [SerializeField] Transform smokeFiringTransform;

    [Header("Skill UI")]
    private SkillUIEntry dashUI;
    private SkillUIEntry smokeUI;
    private SkillUIEntry updraftUI;
    private SkillUIEntry ultimateUI;

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

    void Start()
    {
        if (!photonView.IsMine) return;

        playerController = GetComponent<PlayerController>();
        jettStats = GetComponent<JettStats>();
        playerCamera = Camera.main;

        var ui = UIController.instance;
        AssignSkillUI(ui.dashUI, ui.smokeUI, ui.updraftUI, ui.ultimateUI);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        CheckIsFalling();

        HandleDash();
        HandleSmoke();
        HandleUpdraft();

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
        isUpdrafting = true;
        lastTimeUpdrafted = Time.time;
        updraftAttempts++;

        float upVelocity = playerController.characterController.isGrounded ?
            Mathf.Sqrt(jettStats.updraftHeight * -2f * currentGravity) :
            Mathf.Sqrt((jettStats.updraftHeight / 2.5f) * -2f * currentGravity);

        velocity.y = upVelocity;
    }
    #endregion

    #region Float
    void HandleFloat()
    {
        currentGravity = (isFalling && Input.GetKey(KeyCode.Space)) ? JettStats.FloatingGravity : DefaultGravity;
    }
    #endregion

    #region Helper
    public void AssignSkillUI(SkillUIEntry dash, SkillUIEntry smoke, SkillUIEntry up, SkillUIEntry ult)
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
    #endregion
}



