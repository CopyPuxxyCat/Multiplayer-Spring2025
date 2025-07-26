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
        public SkillUIEntryData Skill1;
        public SkillUIEntryData Skill2;
        public SkillUIEntryData Skill3;
        public SkillUIEntryData ultimateSkill;
    }

    [Header("Skill UI References")]
    [SerializeField] private SkillUIEntry skill1UI;
    [SerializeField] private SkillUIEntry skill2UI;
    [SerializeField] private SkillUIEntry skill3UI;
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

        skill1UI.SetData(set.Skill1);
        skill2UI.SetData(set.Skill2);
        skill3UI.SetData(set.Skill3);
        ultimateUI.SetData(set.ultimateSkill);

        skill1UI.Initialize();
        skill2UI.Initialize();
        skill3UI.Initialize();
        ultimateUI.Initialize();
    }
}

