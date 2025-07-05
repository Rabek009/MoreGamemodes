using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime;

namespace MoreGamemodes
{
    // https://github.com/tukasa0001/TownOfHost/blob/main/Helpers/IEnumeratorHelper.cs
    public class CoroutinPatcher
    {
        Dictionary<string, Action> _prefixActions = new();
        Dictionary<string, Action> _postfixActions = new();
        private readonly Il2CppSystem.Collections.IEnumerator _enumerator;
        public CoroutinPatcher(Il2CppSystem.Collections.IEnumerator enumerator)
        {
            _enumerator = enumerator;
        }
        public void AddPrefix(Type type, Action action)
        {
            var key = Il2CppType.From(type).FullName;
            _prefixActions[key] = action;
        }
        public void AddPostfix(Type type, Action action)
        {
            var key = Il2CppType.From(type).FullName;
            _postfixActions[key] = action;
        }
        public Il2CppSystem.Collections.IEnumerator EnumerateWithPatch()
        {
            return EnumerateWithPatchInternal().WrapToIl2Cpp();
        }
        public System.Collections.IEnumerator EnumerateWithPatchInternal()
        {
            while (_enumerator.MoveNext())
            {
                var fullName = _enumerator.Current?.GetIl2CppType()?.FullName;
                if (fullName == null)
                {
                    yield return _enumerator.Current;
                    continue;
                }
                if (_prefixActions.TryGetValue(fullName, out var prefixAction))
                {
                    prefixAction();
                }
                yield return _enumerator.Current;
                if (_postfixActions.TryGetValue(fullName, out var postfixAction))
                {
                    postfixAction();
                }
            }
        }
    }
}

