using UnityEngine;

public class TalismanObject : MonoBehaviour
{
    [SerializeField] private GameObject parent;
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
        parent.transform.SetParent(collision.transform, true);
    }
}
