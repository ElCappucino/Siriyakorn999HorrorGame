using UnityEngine;

public class LineRendererToTexture : MonoBehaviour
{
    [SerializeField] private Camera renderCamera;
    [SerializeField] private RenderTexture renderTexture;

    public Texture2D SaveLineRendererToTexture()
    {
        // Ensure the camera renders to the correct RenderTexture
        renderCamera.targetTexture = renderTexture;

        // Render the scene from the camera's perspective
        RenderTexture.active = renderTexture;
        renderCamera.Render();

        // Create a new Texture2D and read pixels from the RenderTexture
        Texture2D lineTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        lineTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        lineTexture.Apply();

        return lineTexture;
    }
}