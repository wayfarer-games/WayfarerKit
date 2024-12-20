using System;
using Unity.Logging;
using UnityEngine;
using WayfarerKit.Systems.SceneManagement;

namespace WayfarerKit.Helpers
{
	/// <summary>
	///     Custom behaviour that allows to subscribe to the scene loading event and execute some logic
	///     when specific scene group is loaded from BootAssistant.
	///     Can be used as base class for SceneGroupManager that should start some logic when scene group is loaded.
	/// </summary>
	public abstract class SceneGroupBehaviour<TBoot, TEnum> : MonoBehaviour
        where TBoot : BootAssistant<TBoot, TEnum>
        where TEnum : Enum
    {
        private BootAssistant<TBoot, TEnum> _bootAssistant;
        protected abstract TEnum SceneToFollow { get; }
        protected bool IsLoaded { get; set; }

        protected virtual void OnEnable()
        {
            _bootAssistant = BootAssistant<TBoot, TEnum>.Instance;
            _bootAssistant.SubscribeWhenLoaded(SceneToFollow, OnInternalGroupWasLoaded);
        }

        protected virtual void OnDisable()
        {
            _bootAssistant?.UnsubscribeWhenLoaded(SceneToFollow, OnInternalGroupWasLoaded);
        }
        
        private void OnInternalGroupWasLoaded()
        {
            IsLoaded = true;
            OnGroupWasLoaded();
        }

        protected abstract void OnGroupWasLoaded();
    }
}