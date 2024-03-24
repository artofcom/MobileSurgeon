using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

[Serializable]
public class PlayerablePath
{
    public string Key;
    public PlayableAsset[] TimeLine;
}

public class AniPrefabMain : MonoBehaviour
{
    // Data Field.
    // Should be serialized.
    [SerializeField] Transform[] CameraAniTransforms;
    [SerializeField] bool UseRevertedCamera = false;
    [SerializeField] PlayerablePath[] TimelinePaths;



    // Tool Only Data.
    [Header("---[Tool Purpose Only]---")]
    public Camera LastRenderedCamera;
    public int ThumbnailCount = 10;



    public Transform[] CameraAnimationTransforms => CameraAniTransforms;
    public bool IsUseRevertedCamera => UseRevertedCamera;

    /*public PlayableAsset GetPathPlayableAsset(string pathKey, int idx=0)
    {
        if (string.IsNullOrEmpty(pathKey)) return null;
        
        if (TimelinePaths == null || TimelinePaths.Length == 0)
            return null;

        for(int q = 0; q < TimelinePaths.Length; ++q)
        {
            if (TimelinePaths[q].Key==pathKey && idx>=0 && idx< TimelinePaths[q].TimeLine.Length)
                return TimelinePaths[q].TimeLine[idx];
        }
        return null;
    }*/

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public PlayerablePath GetPlayerablePath(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        for(int q = 0; q < TimelinePaths.Length; ++q)
        {
            if (TimelinePaths[q].Key == key)
                return TimelinePaths[q];
        }
        return null;
    }
}
