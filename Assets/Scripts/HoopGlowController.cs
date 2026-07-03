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
    private string[] _emissionPropNames;

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
            _emissionPropNames = new string[_hoopMaterials.Length];

            for (int i = 0; i < _hoopMaterials.Length; i++)
            {
                Material mat = _hoopMaterials[i];
                if (mat == null) continue;

                // Determine the correct emission property name for this material/shader
                if (mat.HasProperty("_EmissionColor"))
                {
                    _emissionPropNames[i] = "_EmissionColor";
                }
                else if (mat.HasProperty("_Emissions_Color"))
                {
                    _emissionPropNames[i] = "_Emissions_Color";
                }
                else
                {
                    _emissionPropNames[i] = null;
                }

                if (_emissionPropNames[i] != null)
                {
                    // Save the base emission color configured in the inspector
                    _baseColors[i] = mat.GetColor(_emissionPropNames[i]);
                    
                    // If the emission color is black or default (regardless of alpha), default to white so it can glow
                    if (_baseColors[i].r == 0f && _baseColors[i].g == 0f && _baseColors[i].b == 0f)
                    {
                        _baseColors[i] = Color.white;
                    }
                    else
                    {
                        // Ensure alpha is 1 so color multiplication works correctly
                        _baseColors[i].a = 1f;
                    }

                    // Start with emission off
                    mat.SetColor(_emissionPropNames[i], Color.black);
                    if (_emissionPropNames[i] == "_EmissionColor")
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
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
            for (int i = 0; i < _hoopMaterials.Length; i++)
            {
                if (_hoopMaterials[i] != null && _emissionPropNames[i] == "_EmissionColor")
                {
                    _hoopMaterials[i].EnableKeyword("_EMISSION");
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
            for (int i = 0; i < _hoopMaterials.Length; i++)
            {
                if (_hoopMaterials[i] != null && _emissionPropNames[i] != null)
                {
                    _hoopMaterials[i].SetColor(_emissionPropNames[i], Color.black);
                    if (_emissionPropNames[i] == "_EmissionColor")
                    {
                        _hoopMaterials[i].DisableKeyword("_EMISSION");
                    }
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
                if (_hoopMaterials[i] != null && _emissionPropNames[i] != null)
                {
                    _hoopMaterials[i].SetColor(_emissionPropNames[i], _baseColors[i] * emissionMultiplier);
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
