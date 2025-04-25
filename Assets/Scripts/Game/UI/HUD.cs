using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class HUD : MonoBehaviour
{
    public static HUD Instance;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    public VisualElement root;
    Label tooltip;
    VisualElement chargeBar;
    VisualElement chargeBarValue;

    private void Start() {
        root = GetComponent<UIDocument>().rootVisualElement;
        tooltip = root.Q<Label>("Tooltip");
        chargeBar = root.Q<VisualElement>("ChargeBarBackground");
        chargeBarValue = root.Q<VisualElement>("ChargeBarValue");
    }

    public void ToggleVisibility(bool visible) {
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetTooltip(string text) {
        tooltip.text = text;
    }
    public void HideTooltip() {
        SetTooltip("");
    }
    public void UpdateChargeBar(float percentage) {
        UpdateChargeBar(percentage, Color.green);
    }

    public void UpdateChargeBar(float percentage, Color color) {
        if (percentage < 0.01) {
            chargeBar.style.display = DisplayStyle.None;
        }
        else {
            chargeBar.style.display = DisplayStyle.Flex;
        }
        chargeBarValue.transform.scale = new Vector3(percentage, 1, 1);
        chargeBarValue.style.backgroundColor = color;
    }
}
