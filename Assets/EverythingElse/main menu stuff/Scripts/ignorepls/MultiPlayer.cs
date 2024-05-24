//-------------------------------------------------------------------------------------------------------------------------------------------
//Script for changing scene to multiplayer scene
//By Marje-Alicia Harms 
//mtr.-nr.: 768147
//Bachelor semester Expanded Realities
//-------------------------------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiPlayer : MonoBehaviour
{
    public AudioSource buttonPatient;

    public float delayP = 1f; //delay for the therapist button

    //private bool touchedButtonP;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            buttonPatient.pitch = Random.Range(1.1f, 1.2f);
            buttonPatient.Play();
            SceneManager.LoadScene("MultiPlayer");
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "ButtonP")
    //    {
    //        buttonPatient.pitch = Random.Range(1.1f, 1.2f);
    //        buttonPatient.Play();
    //        //SceneManager.LoadScene("SampleScene");
    //        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "ButtonP")
    //    {

    //    }
    //}

    //since calling this isn't working lets try the tag appriach
    public void PatientButtonPressed() //called the buttons therapist and patient button at first but then realized I just need one to join first to make that distinction so this is a remnant of that
    {
        buttonPatient.pitch = Random.Range(1.1f, 1.2f);
        buttonPatient.Play();
        SceneManager.LoadScene("MultiPlayer");

        
    }

    public void DufficultiIPress() //chooses newYork room
    {
        buttonPatient.pitch = Random.Range(1.1f, 1.2f);
        buttonPatient.Play();
        SceneManager.LoadScene("MainMenu 1");
    }

    public void BackPress() //back to first menu
    {
        buttonPatient.pitch = Random.Range(1.1f, 1.2f);
        buttonPatient.Play();
        SceneManager.LoadScene("FirstMenu");
    }

    public void DelaySceneChange() //chooses new York
    {
        Invoke("PatientButtonPressed", delayP);
    }
}
