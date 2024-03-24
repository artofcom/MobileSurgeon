using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewPaginator : MonoBehaviour
{
    [SerializeField] ScrollRect ScrollRect;
    [SerializeField] RectTransform ContentPanel;
    [SerializeField] RectTransform SampleItem;
    [SerializeField] HorizontalLayoutGroup HLG;
    [SerializeField] Image DotImageSample;
    [SerializeField] float PositionOffset = 200.0f;

    bool mNeedUpdate = true;
    List<GameObject> ListDotImageObj = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {}

    void Update()
    {
        if (ScrollRect.velocity.magnitude < 1.0f)
        {
            if (mNeedUpdate)
            {
                mNeedUpdate = false;
                RefreshPage();
            }
        }
        else mNeedUpdate = true;
        
    }

    public void Init(int pageCount, float center=.0f)
    {
        UnityEngine.Assertions.Assert.IsTrue(pageCount > 0);

        if (ListDotImageObj.Count > 0)
        {
            for (int k = 0; k < ListDotImageObj.Count; ++k)
                GameObject.Destroy(ListDotImageObj[k]);
            ListDotImageObj.Clear();
        }

        for(int k = 0; k < pageCount; ++k)
        {
            var dotImgeObject = GameObject.Instantiate(DotImageSample.gameObject, this.transform);
            dotImgeObject.SetActive(true);

            float posX = .0f;
            if (pageCount % 2 == 1)
            {
                int idxMid = (pageCount / 2);
                posX = (k - idxMid) * PositionOffset;
            }
            else
            {
                int idxMid = (pageCount / 2);
                if(k < idxMid)
                {
                    --idxMid;
                    posX -= (PositionOffset * 0.5f);
                    posX += (k - idxMid) * PositionOffset;
                }
                else
                {
                    posX += (PositionOffset * 0.5f);
                    posX += (k - idxMid) * PositionOffset;
                }
            }
            posX += center;
            dotImgeObject.transform.localPosition = new Vector3(posX, dotImgeObject.transform.localPosition.y, dotImgeObject.transform.localPosition.z);
            ListDotImageObj.Add(dotImgeObject);
        }
        RefreshPage();
    }

    void RefreshPage()
    {
        int idxPage = Mathf.RoundToInt((.0f - ContentPanel.localPosition.x / (SampleItem.rect.width + HLG.spacing)));

        for(int k = 0; k < ListDotImageObj.Count; ++k)
        {
            Color color = idxPage == k ? Color.white : Color.gray;
            ListDotImageObj[k].GetComponent<Image>().color = color;
        }
    }
}
