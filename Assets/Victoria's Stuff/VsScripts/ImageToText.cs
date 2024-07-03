using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;
using TMPro;

// Image-to-Text script //
// Written by Victoria Amelunxen //

public class ImageToText : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    private string apiKey = APIAccess.apiKey;
    private string url = APIAccess.apiAdress;
    [SerializeField] private string role = "your_system_role";
    [SerializeField] private string question = "your_question";
    [SerializeField] private string imagePath = "Assets/ImageData/horse.jpg";
    [SerializeField] private string imgType = "jpg";
    [SerializeField] public string answer;

    private bool answered = false;
    
    // Trigger Image-to-text from an assigned button
    public void SendImageToText()
    {
        answered = false;
        StartCoroutine(PostRequest());
    }

    private IEnumerator PostRequest()
    {
        // Read the image and convert it to base64
        byte[] imageBytes = PersistantData.imageData;
        string base64Image = Convert.ToBase64String(imageBytes);

        // Create JSON payload
        JObject data = new JObject
        {
            { "model", "gpt-4o" },
            { "max_tokens", 300 }
        };
        JArray messages = new JArray();
        JObject m1 = new JObject
        {
            { "role", "user" },
            { "content", role }
        };
        JArray content = new JArray();
        JObject c1 = new JObject
        {
            { "type", "text" },
            { "text", question }
        };
        JObject c2 = new JObject
        {
            { "type", "image_url" }
        };
        JObject img_url = new JObject
        {
            { "url", "data:image/" + imgType + ";base64," + base64Image }
        };
        c2["image_url"] = img_url;
        content.Add(c1);
        content.Add(c2);
        m1["content"] = content;
        messages.Add(m1);
        data["messages"] = messages;

        // Create UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request Error: " + request.error);
        }
        else
        {
            // Parse JSON response
            JObject json = JObject.Parse(request.downloadHandler.text);

            if (json == null)
            {
                Debug.LogError("JSONObject could not be parsed");
            }
            else
            {
                answer = json["choices"][0]["message"]["content"].ToString();
                text.text = answer;
            }
            answered = true;
        }
    }
}
