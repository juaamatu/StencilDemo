using System.Collections.Generic;
using UnityEngine;

// Somewhat based on water reflections (implemented as planar reflections) in Unity Boat Attack demo.
// https://github.com/Verasl/BoatAttack/blob/master/Packages/com.verasl.water-system/Scripts/Rendering/PlanarReflections.cs
public class PairedPortal : MonoBehaviour
{
    [Header("Portal Mesh References")]
    [SerializeField] private MeshRenderer xPortalMesh;
    [SerializeField] private MeshRenderer yPortalMesh;
    
    private Shader maskShader;
    private readonly Dictionary<MeshRenderer, Camera> portalCameras = new Dictionary<MeshRenderer, Camera>();
    private readonly Dictionary<MeshRenderer, int> portalIndices = new Dictionary<MeshRenderer, int>();
    private readonly int portalIndexShaderProperty = Shader.PropertyToID("_PortalIndex");
    private readonly int hiddenObjectLayer = 8;
    private readonly Quaternion flippedRotation = Quaternion.Euler(0, 180, 0);

    private void Awake()
    {
        maskShader = Shader.Find("Hidden/MaskShader");
    }

    private void SetupPortal(MeshRenderer meshRenderer, int index, RenderTexture targetTexture, Camera mainCamera)
    {
        if (!portalCameras.ContainsKey(meshRenderer))
        {
            GameObject cameraGameObject = new GameObject { hideFlags = HideFlags.HideInHierarchy };
            cameraGameObject.transform.SetParent(meshRenderer.transform);
            Camera portalCamera = cameraGameObject.AddComponent<Camera>();
            portalCamera.CopyFrom(mainCamera);
            portalCamera.depth = mainCamera.depth + 1;
            portalCamera.cullingMask |= 1 << hiddenObjectLayer;
            portalCamera.enabled = false;
            portalCamera.targetTexture = targetTexture;
            portalCameras[meshRenderer] = portalCamera;
        }
        
        Material[] materials = new Material[meshRenderer.sharedMaterials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = new Material(maskShader);
            materials[i].SetInt(portalIndexShaderProperty, portalIndices[meshRenderer] = index);
        }
        meshRenderer.sharedMaterials = materials;
    }

    private void BlitPortal(MeshRenderer portal, MeshRenderer targetPortal, RenderTexture source, Material blitMaterial, Transform cameraTransform)
    {
        if (portal.isVisible)
        {
            Camera portalCamera = portalCameras[portal];
            Matrix4x4 localMatrix = portal.worldToLocalMatrix;
            Matrix4x4 worldMatrix = targetPortal.localToWorldMatrix * Matrix4x4.Rotate(flippedRotation);
            Matrix4x4 cameraMatrix = portalCamera.worldToCameraMatrix;
            
            Vector3 local = localMatrix.MultiplyPoint(cameraTransform.position);
            portalCamera.transform.position = worldMatrix.MultiplyPoint(local);
            
            Vector3 lookLocal = localMatrix.MultiplyVector(cameraTransform.forward);
            Vector3 transformedLook = worldMatrix.MultiplyVector(lookLocal);
            portalCamera.transform.rotation = Quaternion.LookRotation(transformedLook, Vector3.up);

            Vector3 offsetPosition = targetPortal.transform.position + -targetPortal.transform.forward * 0.01f;
            Vector3 cameraPosition = cameraMatrix.MultiplyPoint(offsetPosition);
            Vector3 cameraNormal = cameraMatrix.MultiplyVector(-targetPortal.transform.forward).normalized * 1.0f;
            Vector4 clipPlane = new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
            portalCamera.projectionMatrix =  portalCamera.CalculateObliqueMatrix(clipPlane);

            portalCamera.Render();
            blitMaterial.SetInt(portalIndexShaderProperty, portalIndices[portal]);
            Graphics.Blit(portalCamera.targetTexture, source, blitMaterial);
        }
    }

    public void BlitPortals(RenderTexture source, Material blitMaterial, Transform cameraTransform)
    {
        BlitPortal(xPortalMesh, yPortalMesh, source, blitMaterial, cameraTransform);
        BlitPortal(yPortalMesh, xPortalMesh, source, blitMaterial, cameraTransform);
    }

    public void Initialize(int index, RenderTexture targetTexture, Camera mainCamera)
    {
        SetupPortal(xPortalMesh, index, targetTexture, mainCamera);
        SetupPortal(yPortalMesh, index + 1, targetTexture, mainCamera);
    }
}
