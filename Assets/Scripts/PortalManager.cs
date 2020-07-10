using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    private Material blitMaterial;
    private RenderTexture blitTexture;
    private readonly List<PairedPortal> pairedPortals = new List<PairedPortal>();

    private void Start()
    {
        blitMaterial =  new Material(Shader.Find("Hidden/BlitShader"));
        blitTexture = new RenderTexture(Screen.width, Screen.height, 24);
        PairedPortal[] allPortals = FindObjectsOfType<PairedPortal>();
        for (int i = 0; i < allPortals.Length; i++)
        {
            if (i * 2 < 255)
            {
                allPortals[i].Initialize(i * 2 + 1, blitTexture, mainCamera);
                pairedPortals.Add(allPortals[i]);
            }
            else
            {
                Destroy(allPortals[i]);
            }
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        foreach (PairedPortal pairedPortal in pairedPortals)
        {
            pairedPortal.BlitPortals(source, blitMaterial, mainCamera.transform);
        }
        Graphics.Blit(source, destination);
    }
}
