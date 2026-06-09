using UnityEngine;
using UnityEngine.Serialization;

public class RestartUIManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject uiPopUp;
    [SerializeField] private Transform playerTransform;

    [Header("Configuración de Posición")]
    [SerializeField] private float distance = 3.0f;
    [SerializeField] private float heightOffset = 1.5f;
    [Tooltip("Ajuste en grados para la dirección frontal. Si el pop-up no aparece en frente, ajuste este valor (e.g., 90 o -90).")]
    [FormerlySerializedAs("angleOffset")] [SerializeField] private float angleOffset_ = 0f;

    private Vector3 _positionOffset;
    private bool _isFollowing = false;

    private void LateUpdate()
    {
        if (!_isFollowing || uiPopUp == null || playerTransform == null) return;

        uiPopUp.transform.position = playerTransform.position + _positionOffset;
    }

    public void ShowPopUpAtNorth()
    {
        if (uiPopUp == null || playerTransform == null) return;

        // Apply the angle offset to the player's forward direction
        Vector3 forward = Quaternion.Euler(0, angleOffset_, 0) * playerTransform.forward;
        forward.y = 0; // Flatten to the horizontal plane
        _positionOffset = forward.normalized * distance;
        _positionOffset.y += heightOffset;
        
        // Rotate the popup to face the same direction
        uiPopUp.transform.rotation = Quaternion.LookRotation(forward.normalized);

        _isFollowing = true;
        uiPopUp.SetActive(true);
    }

    public void HidePopUp()
    {
        if (uiPopUp == null) return;

        _isFollowing = false;
        uiPopUp.SetActive(false);
    }
}
