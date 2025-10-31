using UnityEngine;

public class DetectarColision : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Si el objeto tiene el script "DesaparecerObjeto", lo activa
        DesaparecerObjeto obj = hit.gameObject.GetComponent<DesaparecerObjeto>();
        if (obj != null)
        {
            obj.Desaparecer();
        }
    }
}