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
        if (!photonView.IsMine || !isSkillEnabled) return;

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
    }

    void StartMolotov()
    {
        skill3UI.TriggerUse();
        isHoldingBall = true;
        isMolotovMode = true;
        holdBallObject.SetActive(true);
        ToggleWeapons(false);
    }

    void HandleHoldBall()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Vector3 forward = cam.transform.forward;
            Vector3 side = Input.GetMouseButtonDown(0) ? -transform.right : transform.right;
            Vector3 throwDir = (forward + side * 0.5f + Vector3.up * 0.3f).normalized;

            ThrowBall(throwDir);
        }
    }

    void ThrowBall(Vector3 direction)
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
            ball.GetComponent<FlashBall>().Init(direction);
        }

        holdBallObject.SetActive(false);
        isHoldingBall = false;
        playerController.SwitchGun();
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
