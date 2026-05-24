using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GTAG_NotificationLib
{
    public class NotifiLib : MonoBehaviour
    {
        private GameObject hudObj;
        private GameObject hudObjParent;
        private GameObject mainCamera;
        private static Text notificationText;
        private Material textMaterial = new Material(Shader.Find("GUI/Text Shader"));
        private int decayTime = 150;
        private int decayCounter = 200;
        private bool hasInit = false;
        public static bool IsEnabled = true;
        private static float lastNotifyTime;
        private static System.Collections.Generic.Queue<int> lineDecayTimes = new System.Collections.Generic.Queue<int>();

        private void Init()
        {
            mainCamera = GameObject.Find("Main Camera");
            if (mainCamera == null) return;

            hudObj = new GameObject("NOTIFICATIONLIB_HUD_OBJ");
            hudObjParent = new GameObject("NOTIFICATIONLIB_HUD_OBJ2");

            Canvas canvas = hudObj.AddComponent<Canvas>();
            hudObj.AddComponent<CanvasScaler>();
            hudObj.AddComponent<GraphicRaycaster>();

            canvas.enabled = true;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera.GetComponent<Camera>();

            hudObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
            hudObj.GetComponent<RectTransform>().position = new Vector3(
                mainCamera.transform.position.x,
                mainCamera.transform.position.y,
                mainCamera.transform.position.z);

            hudObjParent.transform.position = new Vector3(
                mainCamera.transform.position.x,
                mainCamera.transform.position.y,
                mainCamera.transform.position.z - 4.6f);

            hudObj.transform.SetParent(hudObjParent.transform);
            hudObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);

            Vector3 rot = hudObj.GetComponent<RectTransform>().rotation.eulerAngles;
            rot.y = -270f;
            hudObj.transform.localScale = Vector3.one;
            hudObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(rot);

            GameObject textObj = new GameObject();
            textObj.transform.parent = hudObj.transform;
            notificationText = textObj.AddComponent<Text>();
            notificationText.text = "";
            notificationText.fontSize = 32;
            notificationText.font = MalachiTemp.UI.WristMenu.MenuFont;
            notificationText.rectTransform.sizeDelta = new Vector2(800, 200);
            notificationText.alignment = TextAnchor.LowerLeft;
            notificationText.rectTransform.localScale = new Vector3(0.0025f, 0.0025f, 1f);
            notificationText.rectTransform.localPosition = new Vector3(-0.3f, -0.175f, -0.15f);
            notificationText.material = textMaterial;
        }

        private void FixedUpdate()
        {
            if (!hasInit)
            {
                if (GameObject.Find("Main Camera") != null)
                {
                    Init();
                    hasInit = true;
                }
                return;
            }

            if (mainCamera == null) return;
            hudObjParent.transform.position = mainCamera.transform.position;
            hudObjParent.transform.rotation = mainCamera.transform.rotation;

            if (notificationText.text != "")
            {
                decayCounter++;
                int currentLineDecay = lineDecayTimes.Count > 0 ? lineDecayTimes.Peek() : decayTime;
                if (decayCounter > currentLineDecay)
                {
                    string[] lines = notificationText.text
                        .Split(Environment.NewLine.ToCharArray())
                        .Skip(1)
                        .Where(l => l != "")
                        .ToArray();
                    notificationText.text = string.Join("\n", lines) + (lines.Length > 0 ? "\n" : "");
                    if (lineDecayTimes.Count > 0)
                        lineDecayTimes.Dequeue();
                    decayCounter = 0;
                }
            }
            else
            {
                decayCounter = 0;
                lineDecayTimes.Clear();
            }
        }

        public static void SendNotification(string text, int decayMultiplier = 1)
        {
            if (notificationText == null) return;
            if (lastNotifyTime < Time.time)
            {
                lastNotifyTime = Time.time + 0.05f;
                if (IsEnabled)
                {
                    int idx = text.IndexOf("] ");
                    if (idx >= 0)
                        text = text.Substring(idx + 2);
                    text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", "");
                    text = "<color=green>[Noti]</color> - " + text.Trim();
                    if (!text.Contains(Environment.NewLine))
                        text += Environment.NewLine;
                    string[] existing = notificationText.text.Split(Environment.NewLine.ToCharArray());
                    int lineCount = 0;
                    for (int i = 0; i < existing.Length; i++)
                    {
                        if (existing[i] != "") lineCount++;
                    }
                    int maxLines = 4;
                    if (lineCount >= maxLines)
                    {
                        int removeCount = lineCount - maxLines + 1;
                        var remaining = existing.Where(l => l != "").Skip(removeCount).ToArray();
                        notificationText.text = string.Join("\n", remaining) + "\n";
                        for (int i = 0; i < removeCount && lineDecayTimes.Count > 0; i++)
                            lineDecayTimes.Dequeue();
                    }
                    notificationText.text += text;
                    var instance = FindAnyObjectByType<NotifiLib>();
                    int baseDecay = instance != null ? instance.decayTime : 150;
                    lineDecayTimes.Enqueue(baseDecay * decayMultiplier);
                }
            }
        }

        public static void ClearAllNotifications()
        {
            if (notificationText != null)
                notificationText.text = "";
        }

        public static void ClearPastNotifications(int amount)
        {
            if (notificationText == null) return;
            string[] lines = notificationText.text
                .Split(Environment.NewLine.ToCharArray())
                .Skip(amount)
                .Where(l => l != "")
                .ToArray();
            notificationText.text = string.Join("\n", lines) + (lines.Length > 0 ? "\n" : "");
        }
    }
}
