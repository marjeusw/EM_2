using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Newtonsoft.Json.Linq;

// Text-to-image script //
// Written by Victoria Amelunxen //

public class TextToImage : MonoBehaviour
{
    private bool answered;
    private string apiUrl = APIAccess.apiUrlTTI;
    private string apiKey = APIAccess.apiKey;
    private string prompt => GetComponent<ImageToText>().answer;

    public GameObject targetCanvas;

    public void StartCall()
    {
        StartCoroutine(RequestAnswer());
    }

    IEnumerator RequestAnswer()
    {
        answered = false;

        JObject data = new JObject
        {
            { "model", "dall-e-3" },
            { "prompt", prompt },
            { "size", "1024x1024" },
            { "n", 1 }
        };

        UnityWebRequest post = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data.ToString());
        post.uploadHandler = new UploadHandlerRaw(bodyRaw);
        post.downloadHandler = new DownloadHandlerBuffer();
        post.SetRequestHeader("Content-Type", "application/json");
        post.SetRequestHeader("Authorization", "Bearer "+ apiKey);

        yield return post.SendWebRequest();

        if (post.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed: " + post.error);
        }
        else
        {
            string jsonResponse = post.downloadHandler.text;
            Debug.Log(jsonResponse);
            JObject json = JObject.Parse(jsonResponse);

            if (json != null)
            {
                string revisedPrompt = json["data"][0]["revised_prompt"].ToString();
                Debug.Log(revisedPrompt);
                string imageUrl = json["data"][0]["url"].ToString();
                Debug.Log(imageUrl);
                StartCoroutine(LoadImage(imageUrl));
            }
            else
            {
                Debug.LogError("JSON could not be parsed");
            }
        }

        answered = true;
    }

    IEnumerator LoadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Image load failed: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            targetCanvas.GetComponent<Renderer>().material.mainTexture = texture;
            /*byte[] bytes = texture.EncodeToPNG();
            string timeStamp = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            System.IO.File.WriteAllBytes(Application.dataPath + "/img" + timeStamp + ".png", bytes);
            */
        }
    }
}
