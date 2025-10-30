using UnityEngine;
using System.Collections.Generic;

public class TalismanObject : MonoBehaviour
{
    public enum TalismanType
    {
        Stun,
        Thai,
        Lighting,
        Cross,
        Normal
    }
    [SerializeField] private TalismanInfoList talismanList;
    [SerializeField] private GameObject parent;
    private Rigidbody rb;
    private MeshRenderer mr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();
        talismanList.InitDict();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitEffect(TalismanType type)
    {
        mr.material = talismanList.talismanInfoDict[type].material;
        talismanList.talismanInfoDict[type].vfxObject.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = true;
        parent.transform.SetParent(collision.transform, true);
    }
}

[System.Serializable]
public class TalismanInfoList
{
    public List<TalismanInfo> talismanInfos = new List<TalismanInfo>(); 
    public Dictionary<TalismanObject.TalismanType, TalismanInfo> talismanInfoDict = new Dictionary<TalismanObject.TalismanType, TalismanInfo>();

    public void InitDict()
    {
        foreach (TalismanInfo talismanInfo in talismanInfos)
        {
            talismanInfoDict[talismanInfo.talismanType] = talismanInfo;
        }
    }
}

[System.Serializable]
public class TalismanInfo
{
    public TalismanObject.TalismanType talismanType;
    public Material material;
    public GameObject vfxObject;
}
