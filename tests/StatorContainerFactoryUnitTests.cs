using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Stator.Tests
{
    public class StatorContainerFactoryUnitTests
    {
        [MenuItem("Window/Stator/Run unit tests")]
        public static void RunTests()
        {
            var instance = new StatorContainerFactoryUnitTests();

            var methods = instance.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("Test_"))
                .ToList();
            
            var passed = new List<string>();
            var failed = new List<string>();

            foreach (var testMethod in methods)
            {
                try
                {
                    testMethod.Invoke(instance, new object[0]);
                    passed.Add($"<color=green>* {testMethod.Name} - OK</color>");
                }
                catch(Exception e)
                {
                    failed.Add($"<color=red>* {testMethod.Name} - FAIL</color>: {e.InnerException.ToString()}");
                }
            }

            var message = string.Join("\n", failed.Union(passed));

            if(failed.Count > 0)
            {
                EditorUtility.DisplayDialog("Stator unit tests", "Unit tests FAILED", "Ok");
                Debug.LogError(message);
            }
            else
            {
                EditorUtility.DisplayDialog("Stator unit tests", "Unit tests PASS", "Ok");
                Debug.Log(message);
            }
        }

        public void Resolve_ShouldThrowBecouseNotRegistred()
        {
            // Arrange
            // Act
            // Assert
        }

        public void Test_Ok()
        {

        }

        public void Test_Fail()
        {
            throw new Exception();
        }
    }
}
