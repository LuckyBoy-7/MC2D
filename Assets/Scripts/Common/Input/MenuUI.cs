using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuUI : Singleton<MenuUI>
{
    public GameObject mainMenu;

    public void Quit()
    {
        Application.Quit();
    }

    public void Continue()
    {
        Time.timeScale = 1;
        GameManager.instance.state = GameStateType.Play;
        EventSystem.current.SetSelectedGameObject(null);
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.instance.state == GameStateType.Play) // 正常游玩
            {
                mainMenu.SetActive(true);
                Time.timeScale = 0;
                GameManager.instance.state = GameStateType.PauseAll;
            }
            else if (GameManager.instance.state == GameStateType.PauseAll) // 这个游戏只有在menu的时候才有可能pauseAll
            {
                Continue();
            }
        }
    }
}