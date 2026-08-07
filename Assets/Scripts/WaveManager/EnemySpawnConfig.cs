using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    public string name;
    public GameObject enemyPrefab;

    [Header("Economy")]
    [Tooltip("Wie viel vom Budget kostet dieser Gegner?")]
    public int cost = 10;
    [Tooltip("Wie viele dürfen MAXIMAL in einer einzelnen Sub-Wave sein? (0 = unbegrenzt)")]
    public int maxPerSubWave = 0;

    [Header("Progression & Weight")]
    [Tooltip("Ab welchem Zyklus darf dieser Gegner spawnen?")]
    public int minCycleToSpawn = 1;
    [Tooltip("Start-Gewicht bei Freischaltung (Höher = wird öfter gezogen)")]
    public int baseWeight = 100;
    [Tooltip("Wie viel Gewicht kommt pro Zyklus (nach Freischaltung) dazu?")]
    public int weightIncreasePerCycle = 0;
}