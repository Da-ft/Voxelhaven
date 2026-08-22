using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectable : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectVFX;
    [SerializeField] private bool isScrap;
    [SerializeField] private bool isManaCrystal;

    private void OnTriggerEnter(Collider other)
    {      
        if(other.CompareTag("Player") && isScrap)
        {
            GameManager.Instance.AddScrap(value);
            ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Collectibles);
        }

        if (other.CompareTag("Player") && isManaCrystal)
        {
            GameManager.Instance.AddMana(value);
            ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Collectibles);
        }
        return;
    }
}
