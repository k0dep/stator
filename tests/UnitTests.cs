using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Stator.Tests
{
    public abstract class UnitTests
    {
        public bool RunAll()
        {
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("Test_"))
                .ToList();
            
            var passed = new List<string>();
            var failed = new List<string>();

            foreach (var testMethod in methods)
            {
                try
                {
                    testMethod.Invoke(this, new object[0]);
                    passed.Add($"<color=#009900>PASS</color> - {testMethod.Name}");
                }
                catch(Exception e)
                {
                    failed.Add($"<color=red>FAIL</color> - {testMethod.Name}: {e?.InnerException?.ToString()}");
                }
            }

            var message = string.Join("\n", failed.Union(passed));

            if(failed.Count > 0)
            {
                EditorUtility.DisplayDialog("Stator unit tests", "Unit tests FAILED", "Ok");
                Debug.LogError("Stator unit tests <color=red>FAILS</color> \n" + message);
                return false;
            }
            else
            {
                EditorUtility.DisplayDialog("Stator unit tests", "Unit tests PASS", "Ok");
                Debug.Log("Stator unit tests <color=#009900>PASS</color> \n" + message);
                return true;
            }
        }
        
        public static void AssertThrow<E>(Action a) where E : Exception
        {
            try
            {
                a();
            }
            catch (E)
            {
                return; // OK
            }
        }
    }
}