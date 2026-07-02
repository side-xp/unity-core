using System;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    // NOTE: only the unresolved/negative path is unit-tested here. MonoScriptExtensions.GetScriptType delegates to
    // ScriptUtility, whose script-refs list excludes CoreEditorConfig.ExcludedDirectories (default: "Packages"). Every
    // package-embedded script — including this assembly's fixtures — lives under Packages/ and is therefore filtered out,
    // so a package script can never resolve here. Positive resolution would require an Assets/-based script and a
    // non-excluded config; that coverage is deferred to ScriptUtility's own (Tier 4) pass.
    public class MonoScriptExtensionsTests
    {

        [Test]
        public void GetScriptType_UnresolvableScript_ReturnsFalse()
        {
            // A MonoScript that isn't a project asset has no resolvable script path, so no type can be extracted.
            var orphan = new MonoScript();
            try
            {
                bool success = orphan.GetScriptType(out Type type);
                Assert.IsFalse(success);
                Assert.IsNull(type);
            }
            finally
            {
                Object.DestroyImmediate(orphan);
            }
        }

    }

}
