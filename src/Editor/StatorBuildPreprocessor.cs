using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Stator.Editor
{
    public class StatorBuildPreprocessor : IPreprocessBuild
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            Debug.Log("[Stator] start refresh generated code before build process");
            var service = new StatorRefreshService(new StatorSettings());
            service.Clean();
            AssetDatabase.Refresh();
            service.Refresh();
            AssetDatabase.Refresh();
            Debug.Log("[Stator] finish refresh generated code");
        }
    }
}