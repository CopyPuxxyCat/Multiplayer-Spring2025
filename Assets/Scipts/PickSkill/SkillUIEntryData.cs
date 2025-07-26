using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SkillUIEntryData", menuName = "Skills/Skill UI Entry Data")]
public class SkillUIEntryData : ScriptableObject
{
    public Sprite icon;
    public float cooldownDuration = 10f;
    public int maxCharges = 3;
}
