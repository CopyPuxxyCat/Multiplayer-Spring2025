using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIEntry : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;       // Icon vuông
    [SerializeField] private Image fillImage;       // Hình tròn phủ đếm ngược
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Skill Settings")]
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private float cooldownDuration = 10f;

    private int currentCharges;
    private float cooldownTimer;
    private bool isOnCooldown = false;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        currentCharges = maxCharges;
        fillImage.fillAmount = 1f;
        UpdateUI();
    }

    public void TriggerUse()
    {
        if (currentCharges <= 0 || isOnCooldown)
            return;

        currentCharges--;
        cooldownTimer = cooldownDuration;
        isOnCooldown = true;

        fillImage.fillAmount = 0f;
        iconImage.color = Color.gray;

        UpdateUI();
    }

    void Update()
    {
        Debug.Log("check currentchanrges: " + currentCharges + "check isOncooldown" + isOnCooldown);
        if (!isOnCooldown) return;

        cooldownTimer -= Time.deltaTime;

        float percent = 1f - (cooldownTimer / cooldownDuration);
        fillImage.fillAmount = percent;

        iconImage.color = Color.Lerp(Color.gray, Color.white, percent);

        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;

            fillImage.fillAmount = currentCharges > 0 ? 1f : 0f;
            iconImage.color = currentCharges > 0 ? Color.white : Color.black;

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        countText.text = $"{currentCharges}/{maxCharges}";
        bool hasCharge = currentCharges > 0;
        iconImage.color = hasCharge ? iconImage.color : Color.black;
        fillImage.enabled = hasCharge;
    }

    public bool CanUse => currentCharges > 0 && !isOnCooldown;
}

