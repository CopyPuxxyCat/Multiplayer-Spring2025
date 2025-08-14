using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FireSkillController : MonoBehaviourPun, ISkillBlocker
{
    [Header("Prefabs")]
    [SerializeField] GameObject flashBallPrefab;
    [SerializeField] GameObject molotovBallPrefab;
    [SerializeField] GameObject molotovFieldPrefab;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject bowObject;
    [SerializeField] GameObject buffEffect;
    [SerializeField] GameObject holdBallObject;
    [SerializeField] GameObject[] gunsToDisable;

    [Header("Spawn Points")]
    [SerializeField] Transform castPoint;
    [SerializeField] Transform arrowSpawnPoint;

    private FireStats fireStats;
    private PlayerController playerController;
    private Camera cam;

    private SkillUIEntry skill1UI, skill2UI, skill3UI, ultimateUI;
    public bool isSkillEnabled { get; set; }
    private bool isHoldingBall;
    private bool isMolotovMode;
    private int arrowCount;

    void Start()
    {
        if (!photonView.IsMine) return;

        fireStats = GetComponent<FireStats>();
        playerController = GetComponent<PlayerController>();
        cam = Camera.main;
        isSkillEnabled = false;

        var ui = UIController.instance;
        AssignSkillUI(ui.skill1UI, ui.skill2UI, ui.skill3UI, ui.ultimateUI);
    }

    void Update()
    {
        if (!photonView.IsMine || !isSkillEnabled || ChatUIManager.Instance.isTyping) return;

        if (isHoldingBall)
        {
            HandleHoldBall();
            return;
        }

        if (arrowCount > 0)
        {
            HandleArrowShooting();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && skill1UI.CanUse)
            StartFlashBall();

        if (Input.GetKeyDown(KeyCode.E) && skill2UI.CanUse)
            StartBuff();

        if (Input.GetKeyDown(KeyCode.C) && skill3UI.CanUse)
            StartMolotov();

        if (Input.GetKeyDown(KeyCode.R) && ultimateUI.CanUse)
            StartArrow();
    }

    void StartFlashBall()
    {
        skill1UI.TriggerUse();
        isHoldingBall = true;
        isMolotovMode = false;
        holdBallObject.SetActive(true);
        ToggleWeapons(false);
        playerController.CallCoroutineToggleCanShoot(false, 0f);
    }

    void StartMolotov()
    {
        skill3UI.TriggerUse();
        isHoldingBall = true;
        isMolotovMode = true;
        holdBallObject.SetActive(true);
        ToggleWeapons(false);
        playerController.CallCoroutineToggleCanShoot(false, 0f);
    }

    void HandleHoldBall()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 side = -transform.right;
            Vector3 dir = GetThrowDirection(side);
            ThrowBall(dir, side);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Vector3 side = transform.right;
            Vector3 dir = GetThrowDirection(side);
            ThrowBall(dir, side);
        }
    }

    Vector3 GetThrowDirection(Vector3 sideDirection)
    {
        Vector3 baseDir = cam.transform.forward;
        return (baseDir + sideDirection * 0.5f + Vector3.up * 0.3f).normalized;
    }

    void ThrowBall(Vector3 direction, Vector3 curveDir)
    {
        string prefabName = isMolotovMode ? "Fire/FireMolotovBall" : "Fire/FlashBall";

        GameObject ball = PhotonNetwork.Instantiate(prefabName, castPoint.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = direction * fireStats.throwSpeed;

        if (isMolotovMode)
        {
            ball.GetComponent<FireMolotov>().Init(
                fireStats.molotovDuration,
                fireStats.molotovRadius,
                fireStats.molotovDamageAmount,
                fireStats.molotovHealAmount
            );
        }
        else
        {
            ball.GetComponent<FlashBall>().Init(direction, curveDir);
        }

        holdBallObject.SetActive(false);
        isHoldingBall = false;
        playerController.SwitchGun();
        playerController.CallCoroutineToggleCanShoot(true, 0.5f);
    }

    void StartBuff()
    {
        skill2UI.TriggerUse();
        buffEffect.SetActive(true);
        GetComponent<FireBuffUtil>().RunBuff(fireStats.buffDuration, fireStats.buffSpeedMultiplier);
        StartCoroutine(StopBuffEffectAfter(fireStats.buffDuration));
    }

    IEnumerator StopBuffEffectAfter(float time)
    {
        yield return new WaitForSeconds(time);
        buffEffect.SetActive(false);
    }

    void StartArrow()
    {
        ultimateUI.TriggerUse();
        bowObject.SetActive(true);
        arrowCount = 2;
        ToggleWeapons(false);
        playerController.CallCoroutineToggleCanShoot(false, 0f);
    }

    void HandleArrowShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject arrow = PhotonNetwork.Instantiate("Fire/FireArrow", arrowSpawnPoint.position, cam.transform.rotation);
            arrow.GetComponent<FireArrow>().Init(fireStats.arrowRadius, fireStats.arrowDamage);

            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            rb.velocity = cam.transform.forward * 20f;

            arrowCount--;
            if (arrowCount <= 0)
            {
                bowObject.SetActive(false);
                playerController.SwitchGun();
                playerController.CallCoroutineToggleCanShoot(true, 0.5f);
            }
        }
    }

    void ToggleWeapons(bool state)
    {
        foreach (var obj in gunsToDisable)
            obj.SetActive(state);
    }

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

    public bool ShouldBlockShooting => isHoldingBall || arrowCount > 0;
}
