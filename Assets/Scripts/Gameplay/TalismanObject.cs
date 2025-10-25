using UnityEngine;

public class TalismanObject : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = true;
        gameObject.transform.SetParent(collision.transform);
    }
}
