using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
// Ensure Newtonsoft.Json is available in your project
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextToSpeech : MonoBehaviour
{
    private bool answered2 = false;
    private bool request_started2 = false;
    private string apiKey = APIAccess.apiKey;
    private string url = APIAccess.apiAdress;

    public void StartTextToSpeech()
    {
        if (!request_started2)
        {
            request_started2 = true;
            StartCoroutine(TextToSpeechCoroutine());
        }
    }

    private IEnumerator TextToSpeechCoroutine()
    {
        string requestURL = "https://api.openai.com/v1/audio/speech";

        JObject jdata = new JObject
        {
            { "model", "tts-1" },
            { "input", " this little dinosaur reminds me of my grandma..." },
            { "voice", "nova" },
            { "speed", "0.9" }
        };

        string jsondata = jdata.ToString();

        UnityWebRequest request = new UnityWebRequest(requestURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsondata);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey); // Replace <YOUR_KEY> with your actual API key

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            byte[] bytes = request.downloadHandler.data;

            string path = "Assets/testm.mp3";
            File.WriteAllBytes(path, bytes);

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(path);
            Debug.Log("Saved to " + path);
#endif
            PlayAudio(path);
        }
        answered2 = true;
        request_started2 = false;
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
        }
        else
        {
            Debug.LogError("Failed to load audio clip from path: " + path);
        }
    }
}
