using System.Xml.Linq;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public enum ElementType { Wind, Water, Fire, Earth }

    [Header("Skill Controllers")]
    public MonoBehaviour windSkill;
    public MonoBehaviour waterSkill;
    public MonoBehaviour fireSkill;
    public MonoBehaviour earthSkill;

    private ElementType currentElement;

    private int skillSwitchCount = 0;
    private const int maxSkillSwitches = 2;

    public static PlayerSkillManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        SetSKillFromStartThatPlayerChoose();
        if(skillSwitchCount == 0)
        {
            UIController.instance.GP1.SetActive(true);
            UIController.instance.GP2.SetActive(true);
        }    
    }



    public void SetSKillFromStartThatPlayerChoose()
    {
        
        ElementType element = MatchManager.instance.preGameSelectedElement;
        currentElement = element;

        DisableAllSkills();
        GetSkillComponent(element).GetType().GetProperty("isSkillEnabled")?.SetValue(GetSkillComponent(element), true);
        FindObjectOfType<SkillUIDataBinder>()?.ForceUpdateUI(currentElement);
    }    

    public ElementType GetCurrentElement() => currentElement;

    public void EnableSkill(ElementType element)
    {
        if (element == currentElement) return;
        if (skillSwitchCount >= maxSkillSwitches) return;

        skillSwitchCount++;
        if(skillSwitchCount == 1)
        {
            UIController.instance.GP1.SetActive(false);
        }
        if (skillSwitchCount == 2)
        {
            UIController.instance.GP2.SetActive(false);
        }
        currentElement = element;

        DisableAllSkills();
        GetSkillComponent(element).GetType().GetProperty("isSkillEnabled")?.SetValue(GetSkillComponent(element), true);

        FindObjectOfType<SkillUIDataBinder>()?.ForceUpdateUI(currentElement);
    }

    private void DisableAllSkills()
    {
        SetSkillEnabled(windSkill, false);
        SetSkillEnabled(waterSkill, false);
        SetSkillEnabled(fireSkill, false);
        SetSkillEnabled(earthSkill, false);
    }

    private void SetSkillEnabled(MonoBehaviour skill, bool enabled)
    {
        if (skill == null) return;
        skill.GetType().GetProperty("isSkillEnabled")?.SetValue(skill, enabled);
    }

    private MonoBehaviour GetSkillComponent(ElementType element)
    {
        return element switch
        {
            ElementType.Wind => windSkill,
            ElementType.Water => waterSkill,
            ElementType.Fire => fireSkill,
            ElementType.Earth => earthSkill,
            _ => null,
        };
    }
}