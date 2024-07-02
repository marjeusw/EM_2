using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class Hologram : MonoBehaviour

{

    public float maxGlitch;



    public float minGlitch;



    public float glitchLength;



    public float timeBetweenGlitches;



    private void Start()

    {

        StartCoroutine(Glitch());

    }



    IEnumerator Glitch()

    {

        Renderer renderer = GetComponent<Renderer>();



        Material mainMaterial = renderer.material;



        yield return new WaitForSeconds(timeBetweenGlitches);



        mainMaterial.SetFloat("_GlitchStrength", Random.Range(minGlitch, maxGlitch));



        yield return new WaitForSeconds(glitchLength);



        mainMaterial.SetFloat("_GlitchStrength", 0f);



        StartCoroutine(Glitch());

    }

}