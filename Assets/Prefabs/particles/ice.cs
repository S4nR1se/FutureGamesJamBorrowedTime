using System.Runtime.CompilerServices;
using UnityEngine;

public class ice : MonoBehaviour
{
    public GameObject particles;
    private float speed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particles = GetComponent<GameObject>();
        
}

    // Update is called once per frame
    void Update()
    {

        transform.position += new Vector3(0, 0, speed * Time.deltaTime);

        if (transform.position.z > 13f)
        {
            transform.position = new Vector3(33,2,-20);
        }

    }
}
