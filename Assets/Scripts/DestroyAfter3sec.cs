using UnityEngine;
using System.Collections;
public class DestroyAfter3sec : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyAfterSeconds());
    }

    private IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
