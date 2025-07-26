// SkillUIDataBinder.cs
// this is for binding skill data from scriptableobj to skillcontroller in playerprefab
using UnityEngine;
using System.Collections.Generic;

public class SkillUIDataBinder : MonoBehaviour
{
    [System.Serializable]
    public class ElementSkillUIDefinition
    {
        public PlayerSkillManager.ElementType elementType;
        public SkillUIEntryData dashSkill;
        public SkillUIEntryData smokeSkill;
        public SkillUIEntryData updraftSkill;
        public SkillUIEntryData ultimateSkill;
    }

    [Header("Skill UI References")]
    [SerializeField] private SkillUIEntry dashUI;
    [SerializeField] private SkillUIEntry smokeUI;
    [SerializeField] private SkillUIEntry updraftUI;
    [SerializeField] private SkillUIEntry ultimateUI;

    [Header("Element Skill Data Sets")]
    [SerializeField] private List<ElementSkillUIDefinition> elementSkillSets;

    private PlayerSkillManager playerSkillManager;
    private PlayerSkillManager.ElementType lastElementType;

    void Start()
    {
        playerSkillManager = FindObjectOfType<PlayerSkillManager>();
        if (playerSkillManager == null)
        {
            Debug.LogError("[SkillUIDataBinder] Không tìm thấy PlayerSkillManager trên scene!");
            return;
        }

        lastElementType = playerSkillManager.GetCurrentElement();
        ApplySkillUI(lastElementType);
    }

    void Update()
    {
        var currentElement = playerSkillManager.GetCurrentElement();
        if (currentElement != lastElementType)
        {
            ApplySkillUI(currentElement);
            lastElementType = currentElement;
        }
    }

    public void ForceUpdateUI(PlayerSkillManager.ElementType newElement)
    {
        ApplySkillUI(newElement);
        lastElementType = newElement;
    }

    private void ApplySkillUI(PlayerSkillManager.ElementType elementType)
    {
        var set = elementSkillSets.Find(e => e.elementType == elementType);
        if (set == null)
        {
            Debug.LogWarning($"[SkillUIDataBinder] Không có data cho element: {elementType}");
            return;
        }

        dashUI.SetData(set.dashSkill);
        smokeUI.SetData(set.smokeSkill);
        updraftUI.SetData(set.updraftSkill);
        ultimateUI.SetData(set.ultimateSkill);

        dashUI.Initialize();
        smokeUI.Initialize();
        updraftUI.Initialize();
        ultimateUI.Initialize();
    }
}

