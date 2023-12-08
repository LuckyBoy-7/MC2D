using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StrawberryUI : Singleton<StrawberryUI>
{
    public Text iconMask;
    public Text numberText;
    public Text numberMask;
    public float bounceHeight;
    public float bounceTime;

    public int blinkCount = 3;
    public float blinkMaxAlpha = 0.5f;

    public float panelSlideTime;
    public float panelRemainTime;
    private float panelSlideDist => GetComponent<RectTransform>().sizeDelta.x;
    private Vector3 targetPanelSlideOutPos; // 因为如果用move by的话，如果在slide途中又捡到到草莓了，动画就不是很方便
    private Vector3 origPanelPos;
    private bool isShowingPanel;
    private int strawberryCollected;  // 表示UI的草莓数，因为草莓可能被连续收集，还有种办法就是不写这个属性，但是把一部分逻辑写在UI里，不过这样就和数据耦合了


    private void Start()
    {
        origPanelPos = transform.position;
        targetPanelSlideOutPos = origPanelPos + Vector3.right * panelSlideDist;
    }

    public void OnCollected()
    {
        if (!isShowingPanel)  // 反之，AddCounter会解决一切
        {
            StopAllCoroutines(); // 如果有的话，暂停正在滑出的panel
            StartCoroutine(ShowPanel());
        }
    }

    private IEnumerator ShowPanel()
    {
        isShowingPanel = true;
        yield return StartCoroutine(SlideOutPanel());
        yield return StartCoroutine(AddCounter());
        isShowingPanel = false;
        yield return new WaitForSeconds(panelRemainTime);  // 如果这句话放在前面，则addCounter可能已经结束，但我们仍认为她正在工作，就会出现在remain但是不计数的情况
        yield return StartCoroutine(SlideInPanel());
    }

    private IEnumerator AddCounter()
    {
        var player = PlayerFSM.instance;
        while (strawberryCollected < player.strawberryNumber)
        {
            Debug.Log(123);
            strawberryCollected += 1;
            numberText.text = strawberryCollected.ToString();
            numberMask.text = strawberryCollected.ToString();
            StartCoroutine(BounceText());
            yield return StartCoroutine(BlinkText()); // 这俩耗时一样，返回哪个都行
        }
    }


    private IEnumerator SlideOutPanel()
    {
        transform.DOMove(targetPanelSlideOutPos, panelSlideTime);
        yield return new WaitForSeconds(panelSlideTime);
    }

    private IEnumerator SlideInPanel()
    {
        transform.DOMove(origPanelPos, panelSlideTime);
        yield return new WaitForSeconds(panelSlideTime);
    }

    private IEnumerator BlinkText()
    {
        float eachBlinkTime = bounceTime / blinkCount;
        for (int i = 0; i < blinkCount; i++)
        {
            iconMask.DOFade(blinkMaxAlpha, eachBlinkTime / 2);
            numberMask.DOFade(blinkMaxAlpha, eachBlinkTime / 2);
            yield return new WaitForSeconds(eachBlinkTime / 2);
            iconMask.DOFade(0, eachBlinkTime / 2);
            numberMask.DOFade(0, eachBlinkTime / 2);
            yield return new WaitForSeconds(eachBlinkTime / 2);
        }
    }


    private IEnumerator BounceText()
    {
        yield return MoveBy(bounceHeight);
        yield return MoveBy(-bounceHeight);
    }

    private IEnumerator MoveBy(float target)
    {
        float duration = bounceTime / 2;
        float t = 0;
        while (Mathf.Abs(t - duration) > 0.01f)
        {
            t += Time.deltaTime;
            numberText.transform.position += Vector3.up * Time.deltaTime / duration * target;
            yield return null;
        }
    }
}