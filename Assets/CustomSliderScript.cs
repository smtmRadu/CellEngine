using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomSliderScript : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMPro.TMP_Text value_text;
    [SerializeField] string value_text_format = "0.0";

    public void ChangeTextValue() => value_text.text = slider.value.ToString(value_text_format);
}
