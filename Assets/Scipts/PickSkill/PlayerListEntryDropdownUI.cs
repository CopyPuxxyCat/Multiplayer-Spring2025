using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerListEntryDropdownUI : MonoBehaviourPunCallbacks
{
    public TMP_Text playerNameText;
    public TMP_Dropdown elementDropdown;
    public Sprite[] elementSprites; // Gắn Wind, Water, Fire, Earth

    private Player targetPlayer;
    private int selectedIndex = -1;

    void Start()
    {
        if (elementDropdown != null)
        {
            SetupDropdownOptions();
            elementDropdown.onValueChanged.AddListener(OnElementChanged);
        }
    }

    public void Setup(Player player)
    {
        targetPlayer = player;
        playerNameText.text = player.NickName;
        elementDropdown.interactable = player == PhotonNetwork.LocalPlayer;

        SetElementDefault(player);
    }

    void SetupDropdownOptions()
    {
        elementDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < elementSprites.Length; i++)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData
            {
                image = elementSprites[i],
                text = "" // Không cần text
            };
            options.Add(option);
        }

        elementDropdown.AddOptions(options);
    }

    void SetElementDefault(Player player)
    {
        int defaultIndex = 0;
        if (player.CustomProperties.TryGetValue("SelectedElement", out object index))
            defaultIndex = (int)index;

        selectedIndex = defaultIndex;
        elementDropdown.SetValueWithoutNotify(defaultIndex);
        elementDropdown.captionImage.sprite = elementSprites[defaultIndex];

        StartCoroutine(UpdateDropdownItemSprites());
        PlaySelectEffect(defaultIndex);
    }

    void OnElementChanged(int index)
    {
        if (targetPlayer != PhotonNetwork.LocalPlayer) return;

        selectedIndex = index;
        elementDropdown.captionImage.sprite = elementSprites[index];

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
            { "SelectedElement", index }
        });

        StartCoroutine(UpdateDropdownItemSprites());
        PlaySelectEffect(index);
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (target.ActorNumber != targetPlayer.ActorNumber) return;
        if (!changedProps.ContainsKey("SelectedElement")) return;

        int index = (int)changedProps["SelectedElement"];
        selectedIndex = index;

        elementDropdown.SetValueWithoutNotify(index);
        elementDropdown.captionImage.sprite = elementSprites[index];

        StartCoroutine(UpdateDropdownItemSprites());
        PlaySelectEffect(index);
    }

    IEnumerator UpdateDropdownItemSprites()
    {
        yield return new WaitForEndOfFrame(); // Đợi Unity clone xong template

        Transform viewport = elementDropdown.template.GetComponentInChildren<ScrollRect>()?.content;
        if (viewport == null) yield break;

        int count = Mathf.Min(viewport.childCount, elementSprites.Length);
        for (int i = 0; i < count; i++)
        {
            Transform item = viewport.GetChild(i);
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = elementSprites[i];
                bg.color = Color.white;
            }

            TMP_Text label = item.GetComponentInChildren<TMP_Text>();
            if (label) label.text = ""; // Ẩn text
        }
    }

    void PlaySelectEffect(int index)
    {
        // OPTIONAL: Tween caption image scale (hover-like feedback)
        if (elementDropdown.captionImage != null)
        {
            LeanTween.scale(elementDropdown.captionImage.rectTransform, Vector3.one * 1.2f, 0.15f)
                .setEaseOutBack().setOnComplete(() =>
                {
                    LeanTween.scale(elementDropdown.captionImage.rectTransform, Vector3.one, 0.15f);
                });
        }
    }
}
