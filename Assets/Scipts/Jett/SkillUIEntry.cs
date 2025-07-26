using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIEntry : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI countText;

    private int maxCharges;
    private float cooldownDuration;

    private int currentCharges;
    private float cooldownTimer;
    private bool isOnCooldown = false;

    public int RemainingCharges => currentCharges;

    public void SetData(SkillUIEntryData data)
    {
        maxCharges = data.maxCharges;
        cooldownDuration = data.cooldownDuration;
        iconImage.sprite = data.icon;
        iconImage.color = Color.white;
    }

    public void Initialize()
    {
        currentCharges = maxCharges;
        fillImage.fillAmount = 1f;
        iconImage.color = Color.white;
        isOnCooldown = false;
        UpdateUI();
    }

    public void TriggerUse()
    {
        if (!CanUse) return;
        currentCharges--;
        cooldownTimer = cooldownDuration;
        isOnCooldown = true;

        fillImage.fillAmount = 0f;
        iconImage.color = Color.gray;
        UpdateUI();
    }

    void Update()
    {
        if (!isOnCooldown) return;

        cooldownTimer -= Time.deltaTime;
        float percent = 1f - (cooldownTimer / cooldownDuration);
        fillImage.fillAmount = percent;
        iconImage.color = Color.Lerp(Color.gray, Color.white, percent);

        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;
            iconImage.color = currentCharges > 0 ? Color.white : Color.black;
            fillImage.fillAmount = currentCharges > 0 ? 1f : 0f;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        countText.text = $"{currentCharges}/{maxCharges}";
        fillImage.enabled = currentCharges > 0;
    }

    public bool CanUse => currentCharges > 0 && !isOnCooldown;
}