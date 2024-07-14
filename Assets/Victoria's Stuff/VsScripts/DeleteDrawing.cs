using UnityEngine;

// This script deletes the drawings of all colors on button press //
// Written by Victoria Amelunxen //

public class DeleteDrawing : MonoBehaviour
{
    GameObject[] drawingObjects => GameObject.FindGameObjectsWithTag("Drawing");

    public void DeleteDrawings()
    {
        foreach (var drawing in drawingObjects)
        {
            Destroy(drawing);
        }
    }
}
