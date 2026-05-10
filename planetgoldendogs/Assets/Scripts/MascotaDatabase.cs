using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class MascotaDatabase : MonoBehaviour
{
    public static MascotaDatabase Instance { get; private set; }

    [Header("API")]
    [SerializeField] private string webAppUrl;

    public bool IsReady { get; private set; }

    public event Action OnDatabaseReady;

    private readonly List<MascotaRecord> allMascotas = new();
    private readonly Dictionary<string, MascotaRecord> byId = new();
    private readonly Dictionary<string, Sprite> thumbnailCache = new();

    public IReadOnlyList<MascotaRecord> All => allMascotas;

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
        StartCoroutine(LoadAllMascotas());
    }

    private IEnumerator LoadAllMascotas()
    {
        IsReady = false;

        using UnityWebRequest req = UnityWebRequest.Get(webAppUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error cargando mascotas: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(json);

        if (response == null || response.data == null)
        {
            Debug.LogError("La respuesta JSON vino vacía o inválida.");
            yield break;
        }

        allMascotas.Clear();
        byId.Clear();

        foreach (var item in response.data)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id))
                continue;

            allMascotas.Add(item);
            byId[item.id.Trim()] = item;
        }

        IsReady = true;
        OnDatabaseReady?.Invoke();

        Debug.Log($"Mascotas cargadas: {allMascotas.Count}");
    }

    public MascotaRecord GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        byId.TryGetValue(id.Trim(), out var mascota);
        return mascota;
    }

    public bool TryGetThumbnail(string id, out Sprite sprite)
    {
        return thumbnailCache.TryGetValue(id, out sprite);
    }

    public void CacheThumbnail(string id, Sprite sprite)
    {
        if (string.IsNullOrWhiteSpace(id) || sprite == null) return;
        thumbnailCache[id] = sprite;
    }

    public void ClearThumbnailCache()
    {
        foreach (var kv in thumbnailCache)
        {
            if (kv.Value != null)
                Destroy(kv.Value.texture);
        }
        thumbnailCache.Clear();
    }
}