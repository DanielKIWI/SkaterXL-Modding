using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200021E RID: 542
public class GUIConsole : MonoBehaviour
{
    // Token: 0x060016BF RID: 5823
    public static void Log(string msg)
    {
        GUIConsole.StaticAddMessage("Info: " + msg);
    }

    // Token: 0x060016C0 RID: 5824
    public static void LogError(string msg)
    {
        GUIConsole.StaticAddMessage("Error: " + msg);
    }

    // Token: 0x060016C1 RID: 5825
    public static void LogWarning(string msg)
    {
        GUIConsole.StaticAddMessage("Warning: " + msg);
    }

    // Token: 0x060016C2 RID: 5826
    private static void StaticAddMessage(string message)
    {
        GUIConsole.Instance.AddMessage(message);
    }

    // Token: 0x060016C3 RID: 5827
    public void AddMessage(string message)
    {
        this.messages.Add(new GUIConsole.Message(message));
        if (this.messages.Count > this.maxMessages)
        {
            this.messages.RemoveAt(0);
        }
    }

    // Token: 0x060016C4 RID: 5828
    public void Awake()
    {
        this.maxMessages = 10;
        this.messageLifeTime = 10f;
        this.font = new GUIStyle();
        this.font.fontSize = 20;
        this.messages = new List<GUIConsole.Message>();
    }

    // Token: 0x060016C5 RID: 5829
    public void OnGUI()
    {
        if (!this.consoleVisible)
        {
            return;
        }
        if (this.messages.Count > 0)
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            GUI.color = Color.white;
            float num = 25f;
            float num2 = (float)Screen.height - ((float)this.messages.Count * num + 50f);
            float num3 = 30f;
            float num4 = (float)Screen.width - 20f;
            GUI.Box(new Rect(10f, num2, num4, (float)this.messages.Count * num + 40f), "Console");
            for (int i = 0; i < this.messages.Count; i++)
            {
                GUI.Label(new Rect(20f, num2 + num3, num4 - 20f, 20f), this.messages[i].text, this.font);
                num3 += 20f;
            }
        }
    }

    // Token: 0x170005A0 RID: 1440
    // (get) Token: 0x060016C6 RID: 5830
    public static GUIConsole Instance
    {
        get
        {
            if (GUIConsole._instance == null)
            {
                SkateTrainer.CoachFrank.showMessage("GUIConsole._instance is null");
                GUIConsole._instance = new GameObject("GUIConsole").AddComponent<GUIConsole>();
                GUIConsole._instance.enabled = true;
            }
            return GUIConsole._instance;
        }
    }

    // Token: 0x060016C8 RID: 5832
    public void Update()
    {
        if (this.messages.Count > 0 && Time.unscaledTime - this.messages[0].time > this.messageLifeTime)
        {
            this.messages.RemoveAt(0);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            this.consoleVisible = !this.consoleVisible;
        }
    }

    // Token: 0x040010D6 RID: 4310
    private static GUIConsole _instance;

    // Token: 0x040010D7 RID: 4311
    public GUIStyle font;

    // Token: 0x040010D8 RID: 4312
    public float messageLifeTime = 10f;

    // Token: 0x040010D9 RID: 4313
    private List<GUIConsole.Message> messages;

    // Token: 0x040010DA RID: 4314
    public int maxMessages = 10;

    // Token: 0x04001130 RID: 4400
    private bool consoleVisible;

    // Token: 0x0200021F RID: 543
    private struct Message
    {
        // Token: 0x060016C9 RID: 5833
        public Message(string txt)
        {
            this.text = txt;
            this.time = Time.unscaledTime;
        }

        // Token: 0x040010DB RID: 4315
        public string text;

        // Token: 0x040010DC RID: 4316
        public float time;
    }
}
