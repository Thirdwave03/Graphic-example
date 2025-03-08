using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiNode : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    private Node node;
    public void SetNode(Node node)
    {
        this.node = node;
        SetColor(node.CanVisit ? Color.white : Color.gray);
        SetText($"ID: {node.id}\nWeight: {node.weight}");
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    
}
