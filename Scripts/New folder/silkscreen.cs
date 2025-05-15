using UnityEngine;
//using UnityEngine.Rendering.Universal;

public class Silkscreen : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            string lowerName = child.name.ToLower();
            if (lowerName.Contains("silk"))
            {
                // Activate
                child.gameObject.SetActive(true);

                Renderer rend = child.GetComponent<Renderer>();
                if (rend != null)
                {
                    // Create ONE new URP material
                    Material silkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                    // Render both faces
                    // _RenderFace: 0=Front, 1=Back, 2=Both
                    silkMat.SetFloat("_RenderFace", 2f);

                    // Make it red
                    silkMat.SetColor("_BaseColor", Color.red);

                    // Assign to renderer
                    rend.material = silkMat;
                }
            }
            else
            {
                // Deactivate
                child.gameObject.SetActive(false);
            }
        }
    }
}
