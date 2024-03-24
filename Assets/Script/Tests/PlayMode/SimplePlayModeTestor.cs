using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System.Linq;
using App.MVCS;

[TestFixture]
public class SimplePlayModeTestor //: MonoBehaviourBaseTest
{
    [UnityTest]
    public IEnumerator MoveTest()
    {
        var gameObj = new GameObject();
        var activator = gameObj.AddComponent<SurgeAnimation3DView>();

        yield return null;

        activator.SetAnimationSpeed(1.0f);

        yield break;
    }

    [SetUp]
    public virtual void SetUp()
    {

    }

    [TearDown]
    public virtual void TearDownAttribute()
    {
        Object.FindObjectsOfType<GameObject>().ToList().ForEach(go => Object.DestroyImmediate(go));
    }

}
