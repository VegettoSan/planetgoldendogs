using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MascotaListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private MascotaButtonView buttonPrefab;

    private void OnEnable()
    {
        if (MascotaDatabase.Instance != null)
        {
            if (MascotaDatabase.Instance.IsReady)
                BuildList();
            else
                MascotaDatabase.Instance.OnDatabaseReady += BuildList;
        }
    }

    private void OnDisable()
    {
        if (MascotaDatabase.Instance != null)
            MascotaDatabase.Instance.OnDatabaseReady -= BuildList;
    }

    private void BuildList()
    {
        ClearList();

        foreach (var mascota in MascotaDatabase.Instance.All)
        {
            var view = Instantiate(buttonPrefab, contentParent);
            StartCoroutine(SetupButton(view, mascota));
        }
    }

    private IEnumerator SetupButton(MascotaButtonView view, MascotaRecord mascota)
    {
        Sprite thumb = null;

        if (MascotaDatabase.Instance.TryGetThumbnail(mascota.id, out var cached))
        {
            thumb = cached;
        }
        else if (!string.IsNullOrWhiteSpace(mascota.foto_url))
        {
            yield return StartCoroutine(DownloadAndCacheThumbnail(mascota, result => thumb = result));
        }

        view.Setup(mascota, thumb, OnMascotaSelected);
    }

    private IEnumerator DownloadAndCacheThumbnail(MascotaRecord mascota, System.Action<Sprite> onDone)
    {
        using UnityWebRequest req = UnityWebRequestTexture.GetTexture(mascota.foto_url, true);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"No se pudo cargar imagen de {mascota.id}: {req.error}");
            onDone?.Invoke(null);
            yield break;
        }

        Texture2D original = DownloadHandlerTexture.GetContent(req);
        Texture2D small = ResizeTexture(original, 128, 128);
        Sprite sprite = Sprite.Create(small, new Rect(0, 0, small.width, small.height), new Vector2(0.5f, 0.5f), 100f);

        MascotaDatabase.Instance.CacheThumbnail(mascota.id, sprite);
        onDone?.Invoke(sprite);
    }

    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    private void OnMascotaSelected(MascotaRecord mascota)
    {
        Debug.Log("Seleccionada: " + mascota.nombre + " / " + mascota.id);

        // Aquí llamas tu sistema de documento:
        DocumentoManager.Instance.CargarMascota(mascota);
    }

    private void ClearList()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}