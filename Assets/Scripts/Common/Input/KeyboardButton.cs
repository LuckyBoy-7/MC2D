using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Button = UnityEngine.UI.Button;

public class KeyboardButton : MonoBehaviour, ISelectHandler
{
    public InputType inputType;
    private Button button;
    private Text text;

    private bool canOperate;

    private void Awake()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        // 当前按钮不是selected状态
        if (EventSystem.current.currentSelectedGameObject != gameObject || !canOperate)
            return;
        if (Input.anyKey && !Input.GetKeyDown(KeyCode.Escape)) // escape按键要保护
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    text.text = key.ToString();
                    EventSystem.current.SetSelectedGameObject(null);
                    // button.OnDeselect(null);
                    ChangeKey(key);
                }
            }
        }
    }

    private void ChangeKey(KeyCode key)
    {
        if (inputType == InputType.Inventory)
            PlayerInfoUI.instance.inventoryKey = key;
        else if (inputType == InputType.Up)
            PlayerFSM.instance.upKey = key;
        else if (inputType == InputType.Down)
            PlayerFSM.instance.downKey = key;
        else if (inputType == InputType.Left)
            PlayerFSM.instance.leftKey = key;
        else if (inputType == InputType.Right)
            PlayerFSM.instance.rightKey=key;
        else if (inputType == InputType.Jump)
            PlayerFSM.instance.jumpKey = key;
        else if (inputType == InputType.Dash)
            PlayerFSM.instance.dashKey = key;
        else if (inputType == InputType.SuperDash)
            PlayerFSM.instance.superDashKey = key;
        else if (inputType == InputType.Spell)
            PlayerFSM.instance.spellKey = key;
        else if (inputType == InputType.Attack)
            PlayerFSM.instance.attackKey = key;
    }


    public void OnSelect(BaseEventData eventData)
    {
        text.text = "";
        ChangeKey(KeyCode.Joystick2Button0);  // 随便设置一个键盘点不出来的
        StartCoroutine(WairForAFrame());
    }

    private IEnumerator WairForAFrame()
    {
        canOperate = false;
        yield return new WaitForEndOfFrame();
        canOperate = true;
    }

    private void OnDisable()
    {
        Debug.Log("Disable");
        EventSystem.current.SetSelectedGameObject(null);
    }
}
