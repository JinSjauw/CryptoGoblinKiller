using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Dictionary<string, Queue<GameObject>> _objectPool = new Dictionary<string, Queue<GameObject>>();

    public GameObject GetObject(GameObject objectToReturn) 
    {
        if(_objectPool.TryGetValue(objectToReturn.name, out Queue<GameObject> objectList)) 
        {
            if(objectList.Count == 0) 
            {
                return CreateNewObject(objectToReturn);
            }
            else 
            {
                GameObject currentObject = objectList.Dequeue();
                currentObject.SetActive(true);
                return currentObject;
            }
        }
        else { return CreateNewObject(objectToReturn); }
    }

    private GameObject CreateNewObject(GameObject createdGameObject)
    {
        Transform parentTransform = transform.Find(createdGameObject.name);
        
        if (parentTransform == null)
        {
            parentTransform = new GameObject().transform;
            parentTransform.SetParent(transform);
            parentTransform.name = createdGameObject.name;
        }
        
        GameObject newGameObject = Instantiate(createdGameObject, parentTransform);
        newGameObject.name = createdGameObject.name;
        return newGameObject;
    }
    
    public void ReturnGameObject(GameObject returnedGameObject) 
    {
        if(_objectPool.TryGetValue(returnedGameObject.name, out Queue<GameObject> objectList)) 
        {
            objectList.Enqueue(returnedGameObject);
        }
        else 
        {
            Queue<GameObject> newObjectQueue = new Queue<GameObject>();
            newObjectQueue.Enqueue(returnedGameObject);
            _objectPool.Add(returnedGameObject.name, newObjectQueue);
        }

        returnedGameObject.SetActive(false);
    }

}
