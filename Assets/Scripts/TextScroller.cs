using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Logging;

public class TextScroller : MonoBehaviour {


    private Queue text_queue = new Queue();
    private float base_x = -1050;
    private float base_y = -600;
    private int text_width = 400;
    private float text_height = 100;
    private int font_size = 20;
    private int history_size = 5;
    private Vector3 text_movement = new Vector3(0f, 30f, 0f);
    


    // Use this for initialization
    void Start() {

        Log.Debug("TextScroller", "Sending text text");
        addline("THINK", "joy");
        addline("THINK", "joy");
        addline("THINK", "joy");
        addline("THINK", "joy");
        addline("THINK", "joy");

    }

    // Update is called once per frame
    void Update() {

    }

    public void addline(string text, string emotion)
    {

        text = Truncate(text, 40);

        if (text_queue.Count > 0)
        {
            Moveup(text_queue, text_movement);
        }
        text_queue.Enqueue(CreateText(this.gameObject.transform, base_x, base_y, text, font_size, EmotionToColor(emotion)));
        if(text_queue.Count > history_size)
        {
            Destroy(((GameObject)text_queue.Dequeue()));
        }

    }

    private Color EmotionToColor(string emotion)
    {
        switch (emotion)
        {
            case "joy":
                return Color.yellow;
            case "sadness":
                return Color.blue;
            case "fear":
                return Color.magenta;
            case "disgust":
                return Color.green;
            case "anger":
                return Color.red;
            case "idle":
                return Color.white;
            default:
                return Color.white;
        }
        



    }

    private static void Moveup(IEnumerable myCollection, Vector3 text_movement)
    {
        foreach (GameObject obj in myCollection)
        {
            obj.transform.localPosition += text_movement;
        }

    }


        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value :( value.Substring(0, maxLength-3)+"...");
        }


    GameObject CreateText(Transform canvas_transform, float x, float y, string text_to_print, int font_size, Color text_color)
    {
        GameObject UItextGO = new GameObject("Text2");
        UItextGO.transform.SetParent(canvas_transform);

        RectTransform trans = UItextGO.AddComponent<RectTransform>();
        trans.anchoredPosition = new Vector2(x, y);

        Text text = UItextGO.AddComponent<Text>();
        text.text = text_to_print;
        text.fontSize = font_size;
        text.color = text_color;
        text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(text_width, text_height);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return UItextGO;
    }
}
