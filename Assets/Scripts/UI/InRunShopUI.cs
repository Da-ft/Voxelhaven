using System.Collections.Generic;
using UnityEngine;

public class InRunShopUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopDraftSystem draftSystem;
    [SerializeField] private ShopUpgradeButton[] cardButtons;

    private void Start()
    {
        shopPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
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

        shopPanel.SetActive(true);
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
