using UnityEngine;

public class Scrappile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ScrapStage[] scrapStages;

    [System.Serializable]
    public struct ScrapStage
    {
        public int threshold;
        public Sprite pileSprite;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScrapChanged += UpdatePileVisual;

            UpdatePileVisual(GameManager.Instance.Scrap);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScrapChanged -= UpdatePileVisual;
        }
    }

    private void UpdatePileVisual(int currentScrap)
    {
        if (spriteRenderer == null || scrapStages == null || scrapStages.Length == 0) return;

        Sprite targetSprite = scrapStages[0].pileSprite; // Fallback auf die erste Stufe

        foreach (var stage in scrapStages)
        {
            if (currentScrap >= stage.threshold)
            {
                targetSprite = stage.pileSprite;
            }
        }

        if (spriteRenderer.sprite != targetSprite)
        {
            spriteRenderer.sprite = targetSprite;
        }
    }
}
