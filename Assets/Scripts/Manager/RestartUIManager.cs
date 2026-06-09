using UnityEngine;

public class RestartUIManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject uiPopUp;
    [SerializeField] private Transform playerTransform;

    [Header("Configuración de Posición")]
    [SerializeField] private float distance = 0.5f;
    [SerializeField] private float heightOffset = 1.5f;

    private bool _isFollowing = false;

    private void LateUpdate()
    {
        if (!_isFollowing || uiPopUp == null || playerTransform == null) return;

        Vector3 targetPosition = playerTransform.position;
        targetPosition.z += distance; // The 'distance' variable now serves as the X-axis offset
        targetPosition.y += heightOffset;
        
        uiPopUp.transform.position = targetPosition;
    
    }

    public void ShowPopUpAtNorth()
    {
        if (uiPopUp == null) return;

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
