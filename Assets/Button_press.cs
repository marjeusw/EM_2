using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_press : MonoBehaviour

{
    public TextToSpeech textToSpeech;

    void Start()
    {
        if (textToSpeech != null)
        {
            textToSpeech.StartTextToSpeech();
        }
    }
}