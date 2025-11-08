using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; 

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
     
    private GameObject CreateNew()
    {
        var go = Instantiate(prefab, transform);
        return go;
    }

    public GameObject Get()
    {
        GameObject go;
        if (_pool.Count > 0)
        {
            go = _pool.Dequeue();
        }
        else
        {
            go = CreateNew();
        }

        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
        _pool.Enqueue(go);
    }
}
