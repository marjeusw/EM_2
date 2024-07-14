using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenUpdated : MonoBehaviour
{
    [Header("Pen Properties")]
    public Transform tip;
    public Transform snapBackLocation;
    public Material drawingMaterial;
    public Material tipMaterial;
    [Range(0.01f, 0.1f)]
    public float penWidth = 0.01f;
    public Color penColor;

    private LineRenderer currentDrawing;
    private int index;
    private bool drawingActive = false;
    private bool inCollider = false;
    private Transform Canvas => GameObject.Find("DrawingCanvas").transform;
    // Start is called before the first frame update
    void Start()
    {
        tipMaterial.color = penColor;
        gameObject.transform.position = snapBackLocation.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(drawingActive && inCollider)
        {
            Draw();
        }
        else
        {
            currentDrawing = null;
        }
    }

    public void ActivateDrawing()
    {
        drawingActive = true;
    }
    public void DeactivateDrawing() 
    { 
        drawingActive = false;
        gameObject.transform.position = snapBackLocation.position;
        gameObject.transform.rotation = snapBackLocation.rotation;
    }

    private void Draw()
    {
        Vector3 tipPosition2D = new Vector3(tip.position.x, tip.position.y, Canvas.transform.position.z);

        if (currentDrawing == null)
        {
            index = 0;
            currentDrawing = new GameObject($"Drawing: {penColor}").AddComponent<LineRenderer>();
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = penColor;
            currentDrawing.startWidth = currentDrawing.endWidth = penWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.tag = "Drawing";
            currentDrawing.SetPosition(0, tipPosition2D);
        }
        else
        {
            var currentPos = currentDrawing.GetPosition(index);
            if (Vector3.Distance(currentPos, tipPosition2D) > 0.01f)
            {
                index++;
                currentDrawing.positionCount = index + 1;
                currentDrawing.SetPosition(index, tipPosition2D);

                // Convert v3 to v2
                Vector2 v2 = new Vector2(tipPosition2D.x, tipPosition2D.y);
                Debug.Log("Vector2 is: " + v2);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == ("Canvas"))
        {
            inCollider = true;
        }      
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == ("Canvas"))
        {
            inCollider = false;
        }
    }
}
