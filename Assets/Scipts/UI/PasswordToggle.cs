using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordToggle : MonoBehaviour
{
    public TMP_InputField passwordInput;
    public Button ShowPassWordBtn;
    public Sprite OpenEyes;
    public Sprite CloseEyes;

    private bool isPasswordHidden = true;

    public void TogglePassword()
    {
        isPasswordHidden = !isPasswordHidden;

        if (isPasswordHidden)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            ShowPassWordBtn.image.sprite = CloseEyes;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            ShowPassWordBtn.image.sprite = OpenEyes;
        }

        passwordInput.ForceLabelUpdate();
    }
}
