using System;
using Eflatun.SceneReference;
using UnityEngine;

namespace WayfarerKit.Systems.SceneManagement.Helpers
{
    [Serializable]
    public sealed class SceneData
    {
        [SerializeField] private SceneReference reference;
        [SerializeField] private SceneType sceneType;

        public SceneReference Reference => reference;
        public string Name => reference.Name;

        public SceneType SceneType => sceneType;

        public static SceneData FromNameAndReference(SceneReference reference) =>
            new()
            {
                reference = reference,
                sceneType = SceneType.Tooling
            };
    }
}