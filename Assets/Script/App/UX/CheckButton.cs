using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckButton : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button Button;
    [SerializeField] TMP_Text txtCheck;

    bool mChecked = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetCheck(bool check)
    {
        if (mChecked == check) return;

        txtCheck.text = check ? "V" : "";
        mChecked = check;
    }

    public bool GetCheck()
    {
        return mChecked;
    }

    public void OnBtnCheckClicked()
    {
        SetCheck(!GetCheck());
    }

}
