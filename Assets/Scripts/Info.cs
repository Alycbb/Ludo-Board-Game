using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Info : MonoBehaviour
{
    public static Info instance;

    public Text infoText;

    void Awake()
    {
        instance = this;
        infoText.text = "";
    }

    public void ShowMessage(string _text)
    {
        infoText.text = _text;
    }

}
