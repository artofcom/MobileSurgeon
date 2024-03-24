using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using System.Linq;

public class TimeLineCloner : MonoBehaviour
{
    [MenuItem("Timeline/Duplicate With Bindings", true)]
    public static bool DuplicateWithBindingsValidate()
    {
        if (UnityEditor.Selection.activeGameObject == null)
            return false;

        var playableDirector = UnityEditor.Selection.activeGameObject.GetComponent<PlayableDirector>();
        if (playableDirector == null)
            return false;

        var playableAsset = playableDirector.playableAsset;
        if (playableAsset == null)
            return false;

        var path = AssetDatabase.GetAssetPath(playableAsset);
        if (string.IsNullOrEmpty(path))
            return false;

        return true;
    }

    [MenuItem("Timeline/Duplicate With Bindings")]
    public static void DuplicateWithBindings()
    {
        if (UnityEditor.Selection.activeGameObject == null)
            return;

        var playableDirector = UnityEditor.Selection.activeGameObject.GetComponent<PlayableDirector>();
        if (playableDirector == null)
            return;

        var playableAsset = playableDirector.playableAsset;
        if (playableAsset == null)
            return;

        var path = AssetDatabase.GetAssetPath(playableAsset);
        if (string.IsNullOrEmpty(path))
            return;

        string newPath = path.Replace(".", "(Clone).");
        if (!AssetDatabase.CopyAsset(path, newPath))
        {
            Debug.LogError("Couldn't Clone Asset");
            return;
        }

        var newPlayableAsset = AssetDatabase.LoadMainAssetAtPath(newPath) as PlayableAsset;
        var gameObject = UnityEditor.Selection.activeGameObject;
            //GameObject.Instantiate(UnityEditor.Selection.activeGameObject);
        var newPlayableDirector = gameObject.GetComponent<PlayableDirector>();
        newPlayableDirector.playableAsset = newPlayableAsset;

        /*
        var oldBindings = playableAsset.outputs.ToArray();
        var newBindings = newPlayableAsset.outputs.ToArray();

        for (int i = 0; i < oldBindings.Length; i++)
        {
            newPlayableDirector.SetGenericBinding(newBindings[i].sourceObject,
                    playableDirector.GetGenericBinding(oldBindings[i].sourceObject)
                );
        }*/

        var oldBindings = playableAsset.outputs.GetEnumerator();
        var newBindings = newPlayableAsset.outputs.GetEnumerator();


        while (oldBindings.MoveNext())
        {
            var oldBindings_sourceObject = oldBindings.Current.sourceObject;

            newBindings.MoveNext();

            var newBindings_sourceObject = newBindings.Current.sourceObject;

            newPlayableDirector.SetGenericBinding(
                newBindings_sourceObject,
                playableDirector.GetGenericBinding(oldBindings_sourceObject)
            );
        }
    }
}
