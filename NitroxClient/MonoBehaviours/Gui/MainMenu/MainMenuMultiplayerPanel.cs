﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMultiplayerPanel : MonoBehaviour
    {
        public const string SERVER_LIST_PATH = @".\servers";
        public GameObject SavedGamesRef;
        public GameObject LoadedMultiplayerRef;

        private bool shouldFocus;
        private bool showingAddServer;
        private string serverNameInput;
        private string serverIpInput;
        private Rect addServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 200);

        GameObject multiplayerButton;
        Transform savedGameAreaContent;

        public void Awake()
        {
            multiplayerButton = SavedGamesRef.transform.Find("SavedGameArea/SavedGameAreaContent/NewGame").gameObject;
            savedGameAreaContent = LoadedMultiplayerRef.transform.Find("SavedGameArea/SavedGameAreaContent");
            if (!File.Exists(SERVER_LIST_PATH))
            {
                AddServer("local", "127.0.0.1");
            }
            CreateButton("Add a server", ShowAddServerWindow);
            using (StreamReader sr = new StreamReader(SERVER_LIST_PATH))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineData = line.Split('|');
                    string serverName = lineData[0];
                    string serverIp = lineData[1];
                    CreateServerButton($"<b>{serverName}</b>\n{serverIp}", serverIp);
                }
            }
        }

        public void CreateButton(string text, UnityEngine.Events.UnityAction clickEvent)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            multiplayerButtonInst.transform.Find("NewGameButton/Text").GetComponent<Text>().text = text;
            Button multiplayerButtonButton = multiplayerButtonInst.transform.Find("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);
        }

        public void CreateServerButton(string text, string joinIp)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            multiplayerButtonInst.name = (savedGameAreaContent.childCount - 1).ToString();
            multiplayerButtonInst.transform.Find("NewGameButton/Text").GetComponent<Text>().text = text;
            Button multiplayerButtonButton = multiplayerButtonInst.transform.Find("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(() => JoinServer(joinIp));
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);
            
            GameObject delete = Instantiate(SavedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>().deleteButton);
            Button deleteButtonButton = delete.GetComponent<Button>();
            deleteButtonButton.onClick = new Button.ButtonClickedEvent();
            deleteButtonButton.onClick.AddListener(() =>
            {
                RemoveServer(multiplayerButtonInst.transform.GetSiblingIndex() - 1);
                Destroy(multiplayerButtonInst);
            });
            delete.transform.SetParent(multiplayerButtonInst.transform, false);
        }

        public void AddServer(string name, string ip)
        {
            using (StreamWriter sw = new StreamWriter(SERVER_LIST_PATH, true))
            {
                sw.WriteLine($"{name}|{ip}");
            }
        }

        public void RemoveServer(int index)
        {
            List<string> serverLines = new List<string>(File.ReadAllLines(SERVER_LIST_PATH));
            serverLines.RemoveAt(index);
            File.WriteAllLines(SERVER_LIST_PATH, serverLines.ToArray());
        }

        public void JoinServer(string serverIp)
        {
            new GameObject().AddComponent<JoinServer>().ServerIp = serverIp;
        }

        public void ShowAddServerWindow()
        {
            serverNameInput = "local";
            serverIpInput = "127.0.0.1";
            showingAddServer = true;
            shouldFocus = true;
        }

        public void HideAddServerWindow()
        {
            showingAddServer = false;
            shouldFocus = true;
        }

        public void OnGUI()
        {
            if (!showingAddServer)
            {
                return;
            }

            addServerWindowRect = GUILayout.Window(0, addServerWindowRect, DoAddServerWindow, "Add server");
        }


        private void OnAddServerButtonClicked()
        {
            AddServer(serverNameInput, serverIpInput);
            CreateServerButton($"<b>{serverNameInput}</b>\n{serverIpInput}", serverIpInput);
            HideAddServerWindow();
        }

        private void OnCancelButtonClicked()
        {
            HideAddServerWindow();
        }

        private void SetGUIStyle()
        {
            GUI.skin.textField.fontSize = 14;
            GUI.skin.textField.richText = false;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.wordWrap = true;
            GUI.skin.textField.stretchHeight = true;
            GUI.skin.textField.padding = new RectOffset(10, 10, 5, 5);

            GUI.skin.label.fontSize = 14;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.skin.label.stretchHeight = true;
            GUI.skin.label.fixedWidth = 60; //change this when adding new labels that need more space.

            GUI.skin.button.fontSize = 14;
            GUI.skin.button.stretchHeight = true;
        }

        private void DoAddServerWindow(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        OnAddServerButtonClicked();
                        break;
                    case KeyCode.Escape:
                        OnCancelButtonClicked();
                        break;
                }
            }

            SetGUIStyle();
            using (GUILayout.VerticalScope v = new GUILayout.VerticalScope("Box"))
            {
                using (GUILayout.HorizontalScope h1 = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name:");
                    GUI.SetNextControlName("serverNameField");
                    //120 so users can't go too crazy.
                    serverNameInput = GUILayout.TextField(serverNameInput, 120);
                }

                using (GUILayout.HorizontalScope h2 = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("IP:");
                    GUI.SetNextControlName("serverIpField");
                    //120 so users can't go too crazy.
                    serverIpInput = GUILayout.TextField(serverIpInput, 120);
                }

                if (GUILayout.Button("Add server"))
                {
                    OnAddServerButtonClicked();
                }

                if (GUILayout.Button("Cancel"))
                {
                    OnCancelButtonClicked();
                }
            }

            if (shouldFocus)
            {
                GUI.FocusControl("serverNameField");
                shouldFocus = false;
            }
        }
    }
}
