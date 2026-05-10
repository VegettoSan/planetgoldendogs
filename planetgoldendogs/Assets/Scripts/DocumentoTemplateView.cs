using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentoTemplateView : MonoBehaviour
{
    [Header("FRONT")]
    public TMP_Text nombreMascotaText;
    public TMP_Text razaText;
    public TMP_Text especieText;
    public TMP_Text sexoText;
    public TMP_Text duenoText;
    public TMP_Text idText;
    public Image fotoImage;

    [Header("BACK")]
    public TMP_Text fechaNacimientoText;
    public TMP_Text ciudadText;
    public TMP_Text fechaExpedicionText;
    public TMP_Text vacunasText;
    public TMP_Text alimentacionText;
    public TMP_Text otrosText;
    public RawImage qrImage;

    public GameObject rootObject;
}