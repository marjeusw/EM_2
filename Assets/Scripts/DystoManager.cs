//Script to change the solarpunk setting to the dystopian setting (models, particles for sky & skybox)
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DystoManager : MonoBehaviour
{
    public GameObject Dystopia;
    public GameObject SolarPunk;
    bool dystopia;

    public Material skyMaterialDysto;


    // Start is called before the first frame update
    void Start()
    {
        dystopia = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && dystopia == false)
        {
            Debug.Log("it work");
            SolarPunk.SetActive(false);
            Dystopia.SetActive(true);

            RenderSettings.skybox = skyMaterialDysto;
            DynamicGI.UpdateEnvironment(); //updates the lighting; otherwise still solarpunk lighting

            dystopia = true;
        }
    }
}
