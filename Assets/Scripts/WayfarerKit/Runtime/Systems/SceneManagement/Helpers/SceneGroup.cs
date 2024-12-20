using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WayfarerKit.Systems.SceneManagement.Helpers
{
    [Serializable]
    public sealed class SceneGroup
    {
        [SerializeField] private string groupName;
        [SerializeField] private List<SceneData> scenes;

        public string GroupName => groupName;
        public IReadOnlyList<SceneData> Scenes => scenes;

        public string FindSceneNameByType(SceneType sceneType) => Scenes.FirstOrDefault(scene => scene.SceneType == sceneType)?.Reference.Name;
        public bool HasSceneWithName(string name) => Scenes.Any(scene => scene.Reference.Name == name);

        public override string ToString() => $"[SceneGroup: {GroupName} with {Scenes.Count} scenes inside]";
    }
}