using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PlayerSkillManager.ElementType elementType;
    public float hoverScale = 1.1f;

    private Vector3 originalScale;
    private LTDescr currentTween;

    void OnEnable()
    {
        StartCoroutine(DelayedInitializeScale());
    }

    IEnumerator DelayedInitializeScale()
    {
        yield return null;
        originalScale = transform.localScale;

        if (originalScale == Vector3.zero)
        {
            originalScale = Vector3.one;
            transform.localScale = originalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale * hoverScale, 0.15f).setEaseOutBack();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PlayerSkillManager.Instance.GetCurrentElement() != elementType)
        {
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale, 0.15f).setEaseInBack();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayerSkillManager.Instance.EnableSkill(elementType);
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale * hoverScale, 0.1f);
    }
}
