using System.Collections;
using UnityEngine;

public class DesaparecerObjeto : MonoBehaviour
{
    [Header("timers")]
    [Tooltip("delay")]
    public float delayAntesDeDesaparecer = 0.5f;

    [Tooltip("segs desaparecido")]
    public float tiempoDesaparecido = 2f;

    private Renderer render;
    private Collider col;

    private void Awake()
    {
        render = GetComponent<Renderer>();
        col = GetComponent<Collider>();
    }

    public void Desaparecer()
    {
        StartCoroutine(DesaparecerYReaparecer());
    }

    private IEnumerator DesaparecerYReaparecer()
    {
        yield return new WaitForSeconds(delayAntesDeDesaparecer);

        render.enabled = false;
        col.enabled = false;

        yield return new WaitForSeconds(tiempoDesaparecido);

        render.enabled = true;
        col.enabled = true;
    }
}
