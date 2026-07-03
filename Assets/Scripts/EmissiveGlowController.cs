using UnityEngine;
using System.Collections.Generic;

public class EmissiveGlowController : MonoBehaviour
{
    [Header("Renderer Settings")]
    [SerializeField] private List<Renderer> _renderers = new List<Renderer>();

    [Header("Glow Animation Settings")]
    [SerializeField] private float _pulseSpeed = 4f;
    [SerializeField] private float _maxIntensity = 3f;
    [SerializeField] private bool _autoStart = true;

    private List<Material> _materials = new List<Material>();
    private List<Color> _baseColors = new List<Color>();
    private List<string> _emissionPropNames = new List<string>();
    private bool _isGlowing = false;

    private void Start()
    {
        // If no renderers specified, find all renderers in children
        if (_renderers == null || _renderers.Count == 0)
        {
            _renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        }

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            
            // Gets local instances of all materials to avoid modifying the assets on disk
            Material[] mats = r.materials;
            foreach (var mat in mats)
            {
                if (mat == null) continue;

                string propName = null;
                if (mat.HasProperty("_EmissionColor"))
                {
                    propName = "_EmissionColor";
                }
                else if (mat.HasProperty("_Emissions_Color"))
                {
                    propName = "_Emissions_Color";
                }

                if (propName != null)
                {
                    Color baseColor = mat.GetColor(propName);
                    // If the emission color is black or default (regardless of alpha), default to white so it can glow
                    if (baseColor.r == 0f && baseColor.g == 0f && baseColor.b == 0f)
                    {
                        baseColor = Color.white;
                    }
                    else
                    {
                        // Ensure alpha is 1 so color multiplication works correctly
                        baseColor.a = 1f;
                    }

                    _materials.Add(mat);
                    _baseColors.Add(baseColor);
                    _emissionPropNames.Add(propName);

                    // Start with emission off
                    mat.SetColor(propName, Color.black);
                    if (propName == "_EmissionColor")
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        if (_autoStart)
        {
            StartGlow();
        }
    }

    public void StartGlow()
    {
        _isGlowing = true;
        for (int i = 0; i < _materials.Count; i++)
        {
            if (_materials[i] != null && _emissionPropNames[i] == "_EmissionColor")
            {
                _materials[i].EnableKeyword("_EMISSION");
            }
        }
    }

    public void StopGlow()
    {
        _isGlowing = false;
        for (int i = 0; i < _materials.Count; i++)
        {
            if (_materials[i] != null && _emissionPropNames[i] != null)
            {
                _materials[i].SetColor(_emissionPropNames[i], Color.black);
                if (_emissionPropNames[i] == "_EmissionColor")
                {
                    _materials[i].DisableKeyword("_EMISSION");
                }
            }
        }
    }

    private void Update()
    {
        if (_isGlowing && _materials.Count > 0)
        {
            // Pulse the emission intensity using a sine wave
            float emissionMultiplier = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f * _maxIntensity;
            
            for (int i = 0; i < _materials.Count; i++)
            {
                if (_materials[i] != null && _emissionPropNames[i] != null)
                {
                    _materials[i].SetColor(_emissionPropNames[i], _baseColors[i] * emissionMultiplier);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up the dynamic material instances to prevent memory leaks
        foreach (var mat in _materials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
    }
}
