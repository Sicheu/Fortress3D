using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public List<PooledObjectData> objectsToPool;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (PooledObjectData data in objectsToPool)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject obj = Instantiate(data.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(data.objectTypeName, objectPool);
        }
    }
    
    public GameObject SpawnFromPool(string objectTypeName, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary == null)
        {
            Debug.Log("풀매니저 비어있음");
            return null;
        }
        
        if (!poolDictionary.ContainsKey(objectTypeName))
        {
            Debug.LogWarning("Pool with type " + objectTypeName + " doesn't exist.");
            return null;
        }
        
        // 풀에 객체가 남아 있는지 확인
        if (poolDictionary[objectTypeName].Count == 0)
        {
            // 풀에 객체가 없으면 새로운 객체 생성
            PooledObjectData pooledObject = objectsToPool.Find(x => x.objectTypeName == objectTypeName);
            GameObject newObject = Instantiate(pooledObject.prefab);
            newObject.SetActive(false);
            poolDictionary[objectTypeName].Enqueue(newObject);
        }
            
        GameObject objectToSpawn = poolDictionary[objectTypeName].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        return objectToSpawn;
    }
    
    public void ReturnToPool(string type, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist.");
            return;
        }

        IPooledObject pooledObj = objectToReturn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectReturn();
        }

        objectToReturn.SetActive(false);
        poolDictionary[type].Enqueue(objectToReturn);
    }
}

public interface IPooledObject
{
    void OnObjectSpawn();
    void OnObjectReturn();
}

// 사용예시
// public class Bullet : MonoBehaviour, IPooledObject
// {
//     public void OnObjectSpawn()
//     {
//         // 총알이 스폰될 때 초기화 로직
//     }
//
//     public void OnObjectReturn()
//     {
//         // 총알이 풀로 반환될 때 정리 로직
//     }
//
//     private void Update()
//     {
//         // 총알 이동 로직
//         // 만약 총알이 수명이 다했거나 충돌했다면:
//         // ObjectPoolManager.Instance.ReturnToPool(PooledObjectType.Bullet, gameObject);
//     }
// }
