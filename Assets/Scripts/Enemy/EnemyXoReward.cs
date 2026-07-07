using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyXoReward : MonoBehaviour
{
    [SerializeField] private EnemyDefinitionSO definition;

    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (definition == null)
        {
            Debug.LogWarning("[EnemyXpReward] Keine EnemyDefinitionSO zugewiesen, keine XP vergeben.");
            return;
        }

        if (Player.Instance == null)
        {
            Debug.LogWarning("[EnemyXpReward] Player.Instance ist null, XP kann nicht vergeben werden.");
            return;
        }

        Player.Instance.AddXp(definition.XpReward);
    }
}
