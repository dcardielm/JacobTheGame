using System.Collections;
using System.Collections.Generic;
using Corndog.Interactables;
using UnityEngine;
using UnityEngine.UIElements;


namespace Corndog.Systems
{
    public class InventorySystem
    {
        [SerializeField] private float capacity;
        [SerializeField] private float maxCapacity;

        private List<GameObject> gameObjects;

        public System.Action<GameObject> OnStoreObject;
        public System.Action<GameObject> OnUnstoreObject;

        public T[] GetObjectsByType<T>()
        {
            List<T> objs = new List<T>();

            foreach(GameObject obj in gameObjects)
            {
                if (obj.TryGetComponent(out T component))
                {
                    objs.Add(component);
                }
            }
            return objs.ToArray();
        }

        public void RemoveFromInventory(GameObject gameObject)
        {   
            gameObjects.Remove(gameObject);
        }

        public void AddToInventory(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }
    }
}

