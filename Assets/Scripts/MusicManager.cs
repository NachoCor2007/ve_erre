using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Tooltip("El AudioSource que reproducirá la música. Si está vacío, tomará el del objeto.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Tiempo en segundos que tarda en hacer la transición (Fade In / Fade Out) entre canciones.")]
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    private Coroutine currentFadeCoroutine;
    private float maxVolume = 1.0f; // Puedes cambiar esto si tus canciones suenan muy fuerte

    private void Awake()
    {
        // Patrón Singleton para acceder desde cualquier otro script fácilmente
        if (Instance == null)
        {
            Instance = this;
            // No usamos DontDestroyOnLoad porque todo ocurre en la misma escena
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Configuración por defecto para música de fondo
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        // Guardamos el volumen inicial por si lo configuraste desde el Inspector
        maxVolume = audioSource.volume;
    }

    private void Start()
    {
        // Por defecto, reproducir la música del menú al iniciar
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("MusicManager: Intentaste reproducir una canción pero no asignaste el AudioClip.");
            return;
        }

        // MAGIA: Si ya está sonando ESTA MISMA canción, no hace nada. 
        // Ideal para cuando reinicias la partida sin cambiar de menú.
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        // Si ya estábamos haciendo una transición, la cancelamos para empezar la nueva
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // Iniciar la transición suave
        currentFadeCoroutine = StartCoroutine(CrossfadeMusic(clip));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Fade Out: Si ya hay algo sonando, bajamos el volumen gradualmente a 0
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            audioSource.volume = 0;
            audioSource.Stop();
        }

        // Cambiamos a la nueva canción
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade In: Subimos el volumen gradualmente hasta el volumen máximo
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, maxVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = maxVolume;
    }
}
