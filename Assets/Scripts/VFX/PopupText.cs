using UnityEngine;
using TMPro;

public class PopupText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private static int sortingOrder = 0;

    [Header("Settings")]
    public float disappearTimerMax = 0.8f;
    public float fadeSpeed = 3f;

    // Basis-Skalierungen
    private Vector3 baseScale;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount, bool isCritical)
    {
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();

        // Sorting Order hochzählen und zuweisen
        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;

        // Visuals basierend auf Normal vs. Crit
        if (isCritical)
        {
            textMesh.fontSize = 8f;
            textMesh.color = new Color(1f, 0.2f, 0.1f); // Kräftiges Rot-Orange
            textMesh.text += "!"; // Ausrufezeichen für mehr Impact

            // Etwas wilderer "Ausbruch" nach links/rechts und stärker nach oben
            moveVector = new Vector3(Random.Range(-1.5f, 1.5f), 3f, Random.Range(-0.5f, 0.5f));
            baseScale = Vector3.one * 1.2f;
        }
        else
        {
            textMesh.fontSize = 5f;
            textMesh.color = Color.white;

            // Entspannter Drift leicht nach links/rechts und oben
            moveVector = new Vector3(Random.Range(-0.7f, 0.7f), 2f, Random.Range(-0.5f, 0.5f));
            baseScale = Vector3.one;
        }

        textColor = textMesh.color;
        disappearTimer = disappearTimerMax;

        // Startet klein für den Pop-Up-Effekt
        transform.localScale = baseScale * 0.5f;
    }

    private void Update()
    {
        // Bewegung anwenden
        transform.position += moveVector * Time.deltaTime;

        // "Reibung" hinzufügen: Die Bewegung wird mit der Zeit langsamer
        moveVector -= moveVector * 3f * Time.deltaTime;

        // Animation (Skalierung)
        if (disappearTimer > disappearTimerMax * 0.5f)
        {
            // Erste Hälfte der Lebenszeit: Größer werden ("Pop" nach oben)
            transform.localScale += Vector3.one * 3f * Time.deltaTime;
        }
        else
        {
            // Zweite Hälfte: Langsam wieder kleiner werden
            transform.localScale -= Vector3.one * 1f * Time.deltaTime;

            // Vorbeugung gegen negative Skalierung
            if (transform.localScale.x < 0) transform.localScale = Vector3.zero;
        }

        // Timer & Ausblenden (Fade out)
        disappearTimer -= Time.deltaTime;

        if (disappearTimer < 0)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            // Wenn komplett unsichtbar: Zerstören oder in den Pool zurücklegen
            if (textColor.a <= 0)
            {
                // ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.UI);
                Destroy(gameObject);
            }
        }
    }
}