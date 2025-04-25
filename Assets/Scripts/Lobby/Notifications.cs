using UnityEngine;
using UnityEngine.UIElements;

public class Notifications : MonoBehaviour
{
    //singleton
    public static Notifications Instance;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void ShowNotification(string text, Color color, float time=5.0f) {
        VisualElement anchor = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Anchor");
        Label notification = new Label(text);
        notification.style.color = color;
        // add a style class
        notification.AddToClassList("NotificationText");
        this.Invoke(() => {
            anchor.Remove(notification);
        }, time);
        anchor.Add(notification);
        
    }
}
