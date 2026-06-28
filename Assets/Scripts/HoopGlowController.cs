using UnityEngine;

public class HoopGlowController : MonoBehaviour
{
    [Header("Renderer Settings")]
    [SerializeField] private Renderer _hoopRenderer;

    [Header("Glow Animation Settings")]
    [SerializeField] private float _pulseSpeed = 4f;
    [SerializeField] private float _maxIntensity = 3f;

    private Material[] _hoopMaterials;
    private bool _isGlowing = false;
    private Color[] _baseColors;

    private void Start()
    {
        if (_hoopRenderer == null)
        {
            _hoopRenderer = GetComponent<Renderer>();
        }

        if (_hoopRenderer != null)
        {
            // Gets local instances of all materials to avoid modifying the assets on disk
            _hoopMaterials = _hoopRenderer.materials;
            _baseColors = new Color[_hoopMaterials.Length];

            for (int i = 0; i < _hoopMaterials.Length; i++)
            {
                // Save the base emission color configured in the inspector
                _baseColors[i] = _hoopMaterials[i].GetColor("_EmissionColor");
                
                // If the emission color is black or default, default to white so it can glow
                if (_baseColors[i] == Color.black)
                {
                    _baseColors[i] = Color.white;
                }

                // Start with emission off
                _hoopMaterials[i].SetColor("_EmissionColor", Color.black);
                _hoopMaterials[i].DisableKeyword("_EMISSION");
            }
        }
        else
        {
            Debug.LogWarning("HoopGlowController: No Renderer assigned and none found on this GameObject.");
        }
    }

    /// <summary>
    /// Activates the pulsing glow effect on all materials. Call this when the play is almost complete.
    /// </summary>
    public void StartGlow()
    {
        _isGlowing = true;
        if (_hoopMaterials != null)
        {
            foreach (var mat in _hoopMaterials)
            {
                if (mat != null)
                {
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    /// <summary>
    /// Deactivates the glow effect on all materials, returning them to their normal appearance.
    /// </summary>
    public void StopGlow()
    {
        _isGlowing = false;
        if (_hoopMaterials != null)
        {
            foreach (var mat in _hoopMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor("_EmissionColor", Color.black);
                    mat.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    private void Update()
    {
        if (_isGlowing && _hoopMaterials != null)
        {
            // Pulse the emission intensity using a sine wave
            float emissionMultiplier = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f * _maxIntensity;
            
            for (int i = 0; i < _hoopMaterials.Length; i++)
            {
                if (_hoopMaterials[i] != null)
                {
                    _hoopMaterials[i].SetColor("_EmissionColor", _baseColors[i] * emissionMultiplier);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up the dynamic material instances to prevent memory leaks
        if (_hoopMaterials != null)
        {
            foreach (var mat in _hoopMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }
}
