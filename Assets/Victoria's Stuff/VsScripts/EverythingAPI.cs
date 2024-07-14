using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Serialization;
using UnityEditor;

public class EverythingAPI : MonoBehaviour
{
    // This Text combines the functionities of screenshoting the painting of the user, image-to-text, and text-to-image //
    // Written by Victoria Amelunxen - based on the scripts of Owi Mahn as provided in Moodle //
    // Contains the TextToSpeech script written by Owi as provided in Moodle //
    
    
    private int ImagesCounter = 0;
    #region Screenshot-taker

    // ----------------------------------- Screenshot-Taker Script ------------------------------------------------ //

    [SerializeField] private RenderTexture renderTexture;
    private Texture2D texture;
    private bool waited = true;

    // Used only when the image is saved (currently not in use)
    [SerializeField] private string screenshotName = "screenshot"; // currently not in use
    [SerializeField] private string folderPath = "Assets/Screenshots"; // Change this to the desired path... currently not in use

    // Convert the RenderTexture into a 2D texture to send it via byte arrays (something you can't do with a rendertexture)
    private Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(500, 500, TextureFormat.RGB24, false);
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

        if (waited && ImagesCounter < 3)
        {
            waited = false;
            StartCoroutine(SetConditionAfterDelay(2f)); // delay
            ImagesCounter++;
            PersistantData.imageData = texture.EncodeToJPG(); // Encode the texture to a JPG
            Debug.LogWarning("Neues Bild!");
            SendImageToText();
        }
        else
        {
            Debug.LogWarning("Kein neues Bild zum speichern gefunden!");
        }     

        // Save the bytes to a file
        byte[] bytes = texture.EncodeToJPG();
        string filePath = (folderPath + "/"+  $"{screenshotName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.jpg");
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Screenshot saved to: {filePath}");
    }
    private IEnumerator SetConditionAfterDelay(float delay)
    {
        Debug.LogWarning("waiting initiated");
        yield return new WaitForSeconds(delay);
        waited = true;
        Debug.LogWarning("waiting completed");
    }

    #endregion

    #region Image-to-text

    // ----------------------------------- Image-To-Text Script (voice text) ------------------------------------------------ //

    public TextMeshProUGUI text;

    private string apiKey = APIAccess.apiKey;
    private string url = APIAccess.apiAdress;
    [SerializeField] private string role = "Answer like you are positively reacting to an art students work.";
    [SerializeField] private string question = "Look at the given image I've painted. What do you think? Please answer kindly.";
    [SerializeField] private string imgType = "jpg";
    public string answerReaction;

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
                answerReaction = json["choices"][0]["message"]["content"].ToString();
                text.text = answerReaction;
                StartCoroutine(RequestAnswer());
            }
            answered = true;
        }
    }
    #endregion

    #region Text-to-image

    // ----------------------------------- Text-To-Image Script (projecting the image) ------------------------------------------------ //

    private bool answerImage;
    private string apiUrl = APIAccess.apiUrlTTI;

    public GameObject targetCanvas;
    [HideInInspector] public Texture2D AITexture;

    IEnumerator RequestAnswer()
    {
        answerImage = false;

        JObject data = new JObject
        {
            { "model", "dall-e-3" },
            { "prompt", answerReaction },
            { "size", "1024x1024" },
            { "n", 1 }
        };

        UnityWebRequest post = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data.ToString());
        post.uploadHandler = new UploadHandlerRaw(bodyRaw);
        post.downloadHandler = new DownloadHandlerBuffer();
        post.SetRequestHeader("Content-Type", "application/json");
        post.SetRequestHeader("Authorization", "Bearer " + apiKey);

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

        answerImage = true;
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
            AITexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            //targetCanvas.GetComponent<Renderer>().material.mainTexture = AITexture;
            /*byte[] bytes = texture.EncodeToPNG();
            string timeStamp = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            System.IO.File.WriteAllBytes(Application.dataPath + "/img" + timeStamp + ".png", bytes);
            */
            StartCoroutine(TextToSpeechCoroutine());
        }
    }

    #endregion

    #region TextToSpeech

    // ----------------------------------- Text-To-speech Script (robot voice) ------------------------------------------------ //
    public string endSpeech = "Alright! You created a lot of amazing art today. I enjoyed keeping you company on your creative journey. But now, it's time to say goodbye. Thank you for painting with me. See you!";

    private IEnumerator TextToSpeechCoroutine()
    {
        string requestURL = APIAccess.apiUrlTTS;

        JObject jdata = new JObject
        {
            { "model", "tts-1" },
            { "input", answerReaction },
            { "voice", "nova" },
            { "speed", "0.9" }
        };

        string jsondata = jdata.ToString();

        UnityWebRequest request = new UnityWebRequest(requestURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsondata);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            byte[] bytes = request.downloadHandler.data;
            Debug.LogWarning("We saved de TTS!");

            string path = "Assets/testm.mp3";
            File.WriteAllBytes(path, bytes);

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(path);
            Debug.Log("Saved to " + path);
#endif
            PlayAudio(path);
        }
    }

    private void PlayAudio(string path)
    {
        // Load and play the audio file
        AudioClip clip = (AudioClip)AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip));
        if (clip != null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
            StartCoroutine(WaitForAudioEnd(audioSource));

        }
        else
        {
            Debug.LogError("Failed to load audio clip from path: " + path);
        }
    }

    IEnumerator WaitForAudioEnd(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        Texture2D tex = AITexture;
        targetCanvas.GetComponent<Renderer>().material.mainTexture = tex;
        if(ImagesCounter == 3)
        {
            StartCoroutine(TextToSpeechEndCoroutine());
        }
    }
    private IEnumerator TextToSpeechEndCoroutine()
    {
        string requestURL = APIAccess.apiUrlTTS;

        JObject jdata = new JObject
        {
            { "model", "tts-1" },
            { "input", endSpeech },
            { "voice", "nova" },
            { "speed", "0.9" }
        };

        string jsondata = jdata.ToString();

        UnityWebRequest request = new UnityWebRequest(requestURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsondata);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            byte[] bytes = request.downloadHandler.data;
            Debug.LogError("We saved de TTS!");

            string path = "Assets/endAudio.mp3";
            File.WriteAllBytes(path, bytes);

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(path);
            Debug.Log("Saved to " + path);
#endif
            PlayAudio(path);
        }
    }
    #endregion

}
