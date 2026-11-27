using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Animacion de los botones del menu: al pasar el mouse (o seleccionarlo con teclado/joystick)
// se ilumina, crece la barra de acento y el texto se corre un poco.
public class BotonMenuEfecto : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    private Button boton;
    private Image fondo;
    private RectTransform barra;
    private RectTransform texto;
    private TextMeshProUGUI tmp;

    private bool seleccionado;
    private float valor;
    private float anchoBarraBase = 4f;
    private float margenTextoBase = 34f;

    public void Configurar(Button boton, Image fondo, RectTransform barra, RectTransform texto, TextMeshProUGUI tmp)
    {
        this.boton = boton;
        this.fondo = fondo;
        this.barra = barra;
        this.texto = texto;
        this.tmp = tmp;
        if (barra != null) anchoBarraBase = barra.sizeDelta.x;
        if (texto != null) margenTextoBase = texto.offsetMin.x;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boton != null && boton.interactable && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnSelect(BaseEventData eventData) { seleccionado = true; }
    public void OnDeselect(BaseEventData eventData) { seleccionado = false; }

    void OnDisable()
    {
        seleccionado = false;
        valor = 0f;
        Actualizar();
    }

    void Update()
    {
        bool activo = boton != null && boton.interactable;
        float objetivo = (seleccionado && activo) ? 1f : 0f;
        valor = Mathf.MoveTowards(valor, objetivo, Time.unscaledDeltaTime * 8f);
        Actualizar();
    }

    private void Actualizar()
    {
        if (fondo == null) return;
        bool activo = boton != null && boton.interactable;
        float k = valor * valor * (3f - 2f * valor);

        fondo.color = Color.Lerp(UIEstilo.FondoBoton, UIEstilo.FondoBotonActivo, k);

        if (barra != null)
        {
            barra.sizeDelta = new Vector2(Mathf.Lerp(anchoBarraBase, anchoBarraBase + 8f, k), barra.sizeDelta.y);
        }

        if (texto != null)
        {
            texto.offsetMin = new Vector2(Mathf.Lerp(margenTextoBase, margenTextoBase + 16f, k), texto.offsetMin.y);
        }

        if (tmp != null)
        {
            tmp.color = activo ? Color.Lerp(UIEstilo.TextoClaro, Color.white, k) : new Color(UIEstilo.TextoApagado.r, UIEstilo.TextoApagado.g, UIEstilo.TextoApagado.b, 0.45f);
        }
    }
}
