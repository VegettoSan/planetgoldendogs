using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ZXing;
using ZXing.QrCode;

public class DocumentoManager : MonoBehaviour
{
    public static DocumentoManager Instance { get; private set; }

    [Header("Templates")]
    [SerializeField] private List<DocumentoTemplateView> templates = new List<DocumentoTemplateView>();
    [SerializeField] private int templateIndex = 0;

    [Header("Image Settings")]
    [SerializeField] private int qrSize = 512;

    [Header("QR Base URL")]
    [SerializeField] private string qrBaseUrl = "https://script.google.com/macros/s/AKfycbygcLGk6P4h6i3TTHPXFwBHOw9geNJ5OsDW1JPeUwPYCaH90UTzIJ6PLrsWdKDDhpFg/exec?id=";

    private DocumentoTemplateView currentTemplate;
    private MascotaRecord currentMascota;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetTemplate(templateIndex);
    }

    public void SetTemplate(int index)
    {
        if (templates == null || templates.Count == 0)
        {
            Debug.LogError("No hay templates asignados en DocumentoManager.");
            return;
        }

        if (index < 0 || index >= templates.Count)
        {
            Debug.LogError("Índice de template fuera de rango.");
            return;
        }

        for (int i = 0; i < templates.Count; i++)
        {
            if (templates[i] != null)
                templates[i].gameObject.SetActive(i == index);
        }

        currentTemplate = templates[index];
    }

    public void CargarMascota(MascotaRecord mascota)
    {
        if (mascota == null)
        {
            Debug.LogWarning("Mascota nula.");
            return;
        }

        currentMascota = mascota;
        if (currentTemplate == null)
            SetTemplate(templateIndex);

        StartCoroutine(LlenarDocumentoCoroutine(mascota));
    }

    public void CargarMascotaPorId(string id)
    {
        var mascota = MascotaDatabase.Instance != null ? MascotaDatabase.Instance.GetById(id) : null;

        if (mascota == null)
        {
            Debug.LogWarning($"No se encontró la mascota con ID: {id}");
            return;
        }

        CargarMascota(mascota);
    }

    private IEnumerator LlenarDocumentoCoroutine(MascotaRecord mascota)
    {
        if (currentTemplate == null)
            yield break;

        // FRONT
        currentTemplate.nombreMascotaText.text = mascota.nombre;
        currentTemplate.razaText.text = mascota.raza;
        currentTemplate.especieText.text = mascota.especie;
        currentTemplate.sexoText.text = mascota.sexo;
        currentTemplate.duenoText.text = mascota.dueno_nombre;
        currentTemplate.idText.text = mascota.id;

        // BACK
        currentTemplate.fechaNacimientoText.text = FormatearFecha(mascota.fecha_nacimiento);
        currentTemplate.ciudadText.text = mascota.ciudad;
        currentTemplate.fechaExpedicionText.text = System.DateTime.Now.ToString("dd/MM/yyyy");
        currentTemplate.vacunasText.text = mascota.vacunas;
        currentTemplate.alimentacionText.text = mascota.alimentacion;
        currentTemplate.otrosText.text = mascota.otros;

        // FOTO
        yield return StartCoroutine(CargarFoto(mascota.foto_url));

        // QR
        string qrUrl = ConstruirUrlQR(mascota.id);
        Texture2D qrTexture = GenerarQR(qrUrl);
        currentTemplate.qrImage.texture = qrTexture;

        Debug.Log("Documento cargado para: " + mascota.nombre);
    }

    private IEnumerator CargarFoto(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || currentTemplate.fotoImage == null)
            yield break;

        using UnityWebRequest req = UnityWebRequestTexture.GetTexture(url, true);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("No se pudo cargar la foto: " + req.error);
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        currentTemplate.fotoImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        currentTemplate.fotoImage.preserveAspect = true;
    }

    private string ConstruirUrlQR(string id)
    {
        return qrBaseUrl + UnityWebRequest.EscapeURL(id);
    }

    private Texture2D GenerarQR(string contenido)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = qrSize,
                Width = qrSize,
                Margin = 1
            }
        };

        Color32[] pixels = writer.Write(contenido);
        Texture2D tex = new Texture2D(qrSize, qrSize, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }

    private string FormatearFecha(string isoDate)
    {
        if (string.IsNullOrWhiteSpace(isoDate))
            return "";

        if (System.DateTime.TryParse(isoDate, out var dt))
            return dt.ToString("dd/MM/yyyy");

        return isoDate;
    }

    public MascotaRecord GetCurrentMascota()
    {
        return currentMascota;
    }
}