using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Reference")]
    [SerializeField] private GameObject tooltipMenu; // El panel que se va a mostrar/ocultar (ContextualMenu)

    [Header("Tooltip Content")]
    [SerializeField] private Play associatedPlay; // El mensaje a mostrar en el componente de texto

    void Start()
    {
        // Nos aseguramos de que empiece oculto
        if (tooltipMenu != null)
        {
            tooltipMenu.SetActive(false);
        }
    }

    // Se ejecuta cuando el Raycaster de VR apunta al botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipMenu != null)
        {
            // Buscamos el componente de texto en los hijos del menú y actualizamos su contenido
            Text textComponent = GetTooltipTextComponent();
            if (textComponent != null)
            {
                textComponent.text = associatedPlay.PlayDescription;
            }

            tooltipMenu.SetActive(true);
            // Opcional: Aquí podrías reproducir un pequeño sonido de hover o una vibración háptica ligera
        }
    }

    // Se ejecuta cuando el Raycaster deja de apuntar al botón
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipMenu != null)
        {
            tooltipMenu.SetActive(false);
        }
    }

    // Por seguridad, si el botón se desactiva, ocultamos el tooltip
    void OnDisable()
    {
        if (tooltipMenu != null)
        {
            tooltipMenu.SetActive(false);
        }
    }

    /// <summary>
    /// Busca de forma segura el componente Text dentro del panel de tooltip
    /// </summary>
    private Text GetTooltipTextComponent()
    {
        if (tooltipMenu == null) return null;

        // Intentamos buscar por la ruta específica proporcionada:
        // ContextualMenu (tooltipMenu) -> ContextualContainer -> Background Panel -> Title
        Transform titleTransform = tooltipMenu.transform.Find("ContextualContainer/Background Panel/Title");
        if (titleTransform != null)
        {
            return titleTransform.GetComponent<Text>();
        }

        // Si la ruta cambia o falla, buscamos recursivamente en los hijos (incluyendo inactivos)
        return tooltipMenu.GetComponentInChildren<Text>(true);
    }
}