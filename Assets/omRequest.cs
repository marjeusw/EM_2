using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json; // install json with -> "com.unity.nuget.newtonsoft-json" in package manager/add url...
using Newtonsoft.Json.Linq;
using System; // need for Convert.ToBase64String
using UnityEngine.UI;
using TMPro;
using UnityEditor; // for saving images

public class omRequest : MonoBehaviour
{
    // image to text
    public TMP_InputField input;
    public TMP_Text output;
    public Button button_request;

    public Texture2D tex;
    public RawImage img;

    private bool answered;
    private float requestTimestamp;
    private bool request_started;

    // text to image
    public TMP_InputField input2;
    public TMP_Text output2;
    public Button button_request2;
    public RawImage outImage;
    public Button button_saveImg;

    private bool answered2;
    private float requestTimestamp2;
    private bool request_started2;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    void Start()
    {
        answered = false;
        request_started = false;
        output.text = "";
        button_request.onClick.AddListener(() => ButtonClicked(1));

        answered2 = false;
        request_started2 = false;
        output2.text = "";
        button_request2.onClick.AddListener(() => ButtonClicked(2));

        button_saveImg.onClick.AddListener(() => ButtonClicked(3));

        string url = "http://owimahn.de/wp-content/uploads/2013/09/owi_denkt.png";

        StartCoroutine(text_to_speech());
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    void Update()
    {
        if (!answered && request_started)
        {
            output.text = "[ " + (Time.realtimeSinceStartup - requestTimestamp).ToString("F2") + " s ]";
        }
        if (!answered2 && request_started2)
        {
            output2.text = "[ " + (Time.realtimeSinceStartup - requestTimestamp2).ToString("F2") + " s ]";
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    void ButtonClicked(int buttonNo)
    {
        if (buttonNo == 1)
        {
            requestTimestamp = Time.realtimeSinceStartup;
            request_started = true;
            answered = false;
            StartCoroutine(image_to_text());
        }
        if (buttonNo == 2)
        {
            requestTimestamp2 = Time.realtimeSinceStartup;
            request_started2 = true;
            answered2 = false;
            StartCoroutine(text_to_image());
        }
        if (buttonNo == 3) StartCoroutine(saveRawImage(outImage));
    }
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    IEnumerator saveRawImage(RawImage _rawImage)
    {
        yield return null;
        var tmp = RenderTexture.GetTemporary(
                _rawImage.texture.width,
                _rawImage.texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );
        Graphics.Blit(_rawImage.texture, tmp);

        var previousRenderTexture = RenderTexture.active;
        RenderTexture.active = tmp;

        var texture2d = new Texture2D(_rawImage.texture.width, _rawImage.texture.height);
        texture2d.ReadPixels(new Rect(0, 0, _rawImage.texture.width, _rawImage.texture.height), 0, 0);
        texture2d.Apply();

        RenderTexture.active = previousRenderTexture;
        RenderTexture.ReleaseTemporary(tmp);

        byte[] bytes;
        bytes = texture2d.EncodeToPNG();

        DateTime theTime = DateTime.Now;
        string datetime = theTime.ToString("yyyy-MM-dd_HH_mm_ss");
        if (!AssetDatabase.IsValidFolder("Assets/dalle3images"))
            AssetDatabase.CreateFolder("Assets", "dalle3images");
        string path = "Assets/dalle3images/img_"+ datetime + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
        Debug.Log("Saved to " + path);
    }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        IEnumerator loadImageFromURLtoRawImage(string url, RawImage _rawImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            //RawImage _rawImage = GetComponent<RawImage>();
            _rawImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    IEnumerator image_to_text()
    {
        string requestURL = "https://api.openai.com/v1/chat/completions";


        JObject jdata = new JObject();

        JProperty m1 = new JProperty("model", "gpt-4o");
        jdata.Add(m1);
        JProperty m2 = new JProperty("max_tokens", 300);

        JArray messages = new JArray();

        JObject ob = new JObject();
        JProperty s = new JProperty("role", "user");

        JObject coco = new JObject();
        JArray content = new JArray();

        JObject cc1 = new JObject();
        JProperty t1 = new JProperty("type", "text");
        JProperty t2 = new JProperty("text", input.text);
        cc1.Add(t1);
        cc1.Add(t2);
        content.Add(cc1);

        byte[] imgBytes = tex.EncodeToPNG();
        string enc = Convert.ToBase64String(imgBytes);

        JObject cc2 = new JObject();
        JProperty tt = new JProperty("type", "image_url");
        JObject tt2 = new JObject();
        JProperty zz1 = new JProperty("url", "data:image/png;base64," + enc);
        JProperty zz2 = new JProperty("detail", "low");
        tt2.Add(zz1);
        tt2.Add(zz2);
        cc2.Add(tt);
        cc2.Add("image_url", tt2);

        content.Add(cc2);

        coco.Add(s);
        coco.Add("content", content);
        messages.Add(coco);


        jdata.Add("messages", messages);
        jdata.Add(m2);

        string jsondata = jdata.ToString();

        UnityWebRequest request = UnityWebRequest.Post(requestURL, jsondata, "application/json");
        //request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer <YOUR_KEY>");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("i am here");
            Debug.LogError(request.error);
        }
        else
        {
            JObject a = JObject.Parse(request.downloadHandler.text);
            string answer = (string)a["choices"][0]["message"]["content"];
            Debug.Log("respond: " + answer);

            output.text = answer;
        }
        answered = true;
        request_started = false;
    }


    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    IEnumerator text_to_text()
    {
        string requestURL = "https://api.openai.com/v1/chat/completions";
        //string r1 = "{\"model\": \"gpt-4o\", \"messages\":[{\"role\": \"system\",\"content\": \"You are a helpful assistant.\"},";
        //string r2 = "{\"role\": \"user\",\"content\": \"What is creativity?\"}]}";
        //string data = r1 + r2;

        JObject jdata = new JObject();

        JProperty m = new JProperty("model", "gpt-4o");
        JArray arr = new JArray();

        JObject ob0 = new JObject();
        JProperty s1 = new JProperty("role", "system");
        JProperty s2 = new JProperty("content", "You are a helpful assistant ");
        ob0.Add(s1);
        ob0.Add(s2);
        arr.Add(ob0);

        JObject ob1 = new JObject();
        JProperty u1 = new JProperty("role", "user");
        JProperty u2 = new JProperty("content", "What is creativity?");
        ob1.Add(u1);
        ob1.Add(u2);
        arr.Add(ob1);

        jdata.Add("messages", arr);

        jdata.Add(m);

        string jsondata = jdata.ToString();


        UnityWebRequest request = UnityWebRequest.Post(requestURL, jsondata, "application/json");
        //request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer <YOUR_KEY>");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            JObject a = JObject.Parse(request.downloadHandler.text);
            string answer = (string)a["choices"][0]["message"]["content"];
            Debug.Log("respond: " + answer);
        }

    }


    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    IEnumerator text_to_image()
    {
        string requestURL = "https://api.openai.com/v1/images/generations";

        JObject jdata = new JObject();

        JProperty m1 = new JProperty("model", "dall-e-3");
        JProperty m2 = new JProperty("prompt", input2.text);
        JProperty m3 = new JProperty("size", "1024x1024");
        JProperty m4 = new JProperty("n", 1);
        jdata.Add(m1);
        jdata.Add(m2);
        jdata.Add(m3);
        jdata.Add(m4);

        string jsondata = jdata.ToString();

        UnityWebRequest request = UnityWebRequest.Post(requestURL, jsondata, "application/json");
        request.SetRequestHeader("Authorization", "Bearer <YOUR_KEY>");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            JObject a = JObject.Parse(request.downloadHandler.text);
            string revised = (string)a["data"][0]["revised_prompt"];
            string imgURLStr = (string)a["data"][0]["url"];
            Debug.Log("revised_prompt: " + revised);
            Debug.Log("image url: " + imgURLStr);
            output2.text = revised;
            StartCoroutine(loadImageFromURLtoRawImage(imgURLStr, outImage));
        }
        answered2 = true;
        request_started2 = false;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    IEnumerator text_to_speech()
    {
        string requestURL = "https://api.openai.com/v1/audio/speech";

        JObject jdata = new JObject();

        JProperty m1 = new JProperty("model", "tts-1");
        JProperty m2 = new JProperty("input", "funktioniert wohl auch in unity");
        JProperty m3 = new JProperty("voice", "nova");
        JProperty m4 = new JProperty("speed", "0.9");
        jdata.Add(m1);
        jdata.Add(m2);
        jdata.Add(m3);
        jdata.Add(m4);

        string jsondata = jdata.ToString();

        UnityWebRequest request = UnityWebRequest.Post(requestURL, jsondata, "application/json");
        request.SetRequestHeader("Authorization", "Bearer <YOUR_KEY>");
        //request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            byte[] bytes = request.downloadHandler.data;

            string path = "Assets/testm.mp3";
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            Debug.Log("Saved to " + path);
        }
        answered2 = true;
        request_started2 = false;
    }
}
