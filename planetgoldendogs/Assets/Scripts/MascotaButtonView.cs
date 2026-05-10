using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotaButtonView : MonoBehaviour
{
    [SerializeField] private Image fotoImage;
    [SerializeField] private TMP_Text nombreText;
    [SerializeField] private TMP_Text idText;
    [SerializeField] private Button button;

    public void Setup(MascotaRecord data, Sprite thumbnail, Action<MascotaRecord> onClick)
    {
        nombreText.text = data.nombre;
        idText.text = data.id;

        if (thumbnail != null)
        {
            fotoImage.sprite = thumbnail;
            fotoImage.enabled = true;
        }
        else
        {
            fotoImage.enabled = false;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(data));
    }
}