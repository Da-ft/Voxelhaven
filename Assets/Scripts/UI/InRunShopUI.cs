using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InRunShopUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopDraftSystem draftSystem;
    [SerializeField] private ShopUpgradeButton[] cardButtons;

    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI rerollCostText;

    private void Start()
    {
        shopPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            GameManager.Instance.OnScrapChanged += UpdateRerollButtonState;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameManager.Instance.OnScrapChanged -= UpdateRerollButtonState;
        }
    }

    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
        Debug.Log($"[InRunShopUI] Phasenwechsel erkannt: {newPhase}");
        if (newPhase == GameManager.GamePhase.Rest)
        {
            Time.timeScale = 0f;
            OpenShop();
        }
        else
        {
            CloseShop();
        }
    }

    private void OpenShop()
    {
        Debug.Log("[InRunShopUI] OpenShop() wurde gestartet!");
        RefreshShopCards();
        UpdateRerollButtonState(GameManager.Instance.Scrap);

        Debug.Log("[InRunShopUI] Karten geladen, aktiviere jetzt das Panel!");

        shopPanel.SetActive(true);
    }

    private void RefreshShopCards()
    {
        List<UpgradeDataSO> currentDraft = draftSystem.GenerateShopDraft();

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < currentDraft.Count)
            {
                cardButtons[i].Setup(currentDraft[i]);
            }
            else
            {
                cardButtons[i].Setup(null);
            }
        }
    }

    public void OnRerollButtonClicked()
    {
        int cost = draftSystem.GetCurrentRerollCost();

        if (GameManager.Instance.Scrap >= cost)
        {
            GameManager.Instance.StealScrap(cost);
            draftSystem.RegisterReroll();

            RefreshShopCards();
            UpdateRerollButtonState(GameManager.Instance.Scrap);
        }
    }

    private void UpdateRerollButtonState(int currentScrap)
    {
        int cost = draftSystem.GetCurrentRerollCost();

        if (rerollCostText!= null)
        {
            rerollCostText.text = $"Reroll\n<color=#FFD700>{cost} Scrap</color>";
        }

        if (rerollButton != null)
            rerollButton.interactable = currentScrap >= cost;
    }

    private void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    public void FinishRestPhase()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LeaveRestPhase();
            Time.timeScale = 1f;
        }
    }
}
