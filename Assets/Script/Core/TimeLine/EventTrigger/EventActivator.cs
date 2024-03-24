using System.Collections.Generic;
using UnityEngine;

public class EventActivator : MonoBehaviour
{
    [SerializeField] GameObject ActivationRoot;
    [SerializeField] List<GameObject> ActivationTargets;

    // Start is called before the first frame update
    void Start()
    { }

    public void OnActivated(bool activated)
    {
        // Debug.Log($"TimeLine Activation triggered! {activated}");

        if (ActivationRoot != null)
            ActivationRoot.SetActive(activated);

        if (ActivationTargets != null && ActivationTargets.Count > 0)
        {
            for (int k = 0; k < ActivationTargets.Count; ++k)
                ActivationTargets[k].SetActive(activated);
        }
    }
}
