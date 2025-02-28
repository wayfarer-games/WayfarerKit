using System;
using System.Collections.Generic;
using Unity.Logging;
using UnityEngine;

namespace WayfarerKit.Helpers.UI
{
    public class UniversalObjectSelector : MonoBehaviour
    {
        [SerializeField] private bool hideEverythingOnAwake = true;
        [SerializeField] private List<GameObject> gameObjects;
        
        private void OnEnable()
        {
            if (hideEverythingOnAwake)
                HideEverything();
        }

        public GameObject ShowFor<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            var index = Convert.ToInt32(enumValue);
            if (index >= 0 && index < gameObjects.Count) return ShowForIndex(index);
            
            Log.Warning($"Invalid enum value {enumValue} for {typeof(TEnum).Name}");
            return null;
        }

        public GameObject ShowForIndex(int index)
        {
            for (var i = 0; i < gameObjects.Count; i++)
            {
                var obj = gameObjects[i];
                obj.SetActive(i == index);

                return obj;
            }

            return null;
        }

        public void HideEverything() => ShowForIndex(-1);
    }
}