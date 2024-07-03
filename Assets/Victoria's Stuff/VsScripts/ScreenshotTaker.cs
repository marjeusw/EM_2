using UnityEngine;
using System.IO;
using Unity.VisualScripting;

// Screenshot script //
// Victoria Amelunxen //

public class ScreenshotTaker : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;
    private Texture2D texture;
    private RenderTexture oldTexture = null;
    private ImageToText imgToTxt => GetComponent<ImageToText>();

    // Used only when the image is saved (currently not in use)
    [SerializeField] private string screenshotName = "screenshot"; // currently not in use
    [SerializeField] private string folderPath = "Assets/Screenshots"; // Change this to the desired path... currently not in use

    // Convert the RenderTexture into a 2D texture to send it via byte arrays (something you can't do with a rendertexture)
    private Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(225, 225, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    public void TakeScreenshot()
    {
        
        // Ensure the folder exists before saving (only used when the image is saved)
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        texture = toTexture2D(renderTexture);
        //byte[] bytes = texture.EncodeToJPG();

        if (oldTexture != renderTexture)
        {
            PersistantData.imageData = texture.EncodeToJPG(); // Encode the texture to a JPG
            oldTexture = renderTexture;
            Debug.LogWarning("Neues Bild!");
            imgToTxt.SendImageToText();
            return;
        }

        Debug.LogWarning("Kein neues Bild zum speichern gefunden!");

        // Save the bytes to a file
        /* byte[] bytes = texture.EncodeToJPG();
        string filePath = Path.Combine(folderPath, $"{screenshotName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.jpg");
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Screenshot saved to: {filePath}"); */


    }
}