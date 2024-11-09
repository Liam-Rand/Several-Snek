using CobaPlatinum.DebugTools;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ButtonListSelectUI : MonoBehaviour
{
    [Header("Options List")]
    [SerializeField] private List<string> options = new List<string>();

    [Header("UI Elements")]
    [SerializeField] private GameObject rightButton;
    [SerializeField] private GameObject leftButton;
    [SerializeField] private TextMeshProUGUI selectedOptionText;

    private int defaultOption = 0;

    private int selectedOptionIndex = 0;

    [Header("Events")]
    public UnityEvent<int> OnSelectionChanged;

    // Start is called before the first frame update
    void Awake()
    {
        if (options.Count > defaultOption)
            SelectOption(defaultOption);
        else if (options.Count > 0)
            SelectOption(0);
    }

    public void SelectOption(int _optionIndex)
    {
        if(options.Count < _optionIndex)
        {
            SelectOption(0);
            return;
        }

        CP_DebugWindow.Log(this, "Selecting option: " + _optionIndex + "| " + options[selectedOptionIndex]);

        selectedOptionIndex = _optionIndex;
        selectedOptionText.text = options[selectedOptionIndex];

        leftButton.SetActive(true);
        rightButton.SetActive(true);

        if (selectedOptionIndex == 0)
            leftButton.SetActive(false);
        
        if(selectedOptionIndex == options.Count - 1)
            rightButton.SetActive(false);

        OnSelectionChanged.Invoke(selectedOptionIndex);
    }

    public void SelectNextOption()
    {
        if(selectedOptionIndex < options.Count - 1)
            SelectOption(selectedOptionIndex + 1);
    }

    public void SelectPreviousOption()
    {
        if (selectedOptionIndex > 0)
            SelectOption(selectedOptionIndex - 1);
    }

    public List<string> GetOptions() { return options; }
    public void SetOptions(List<string> _options) { options = _options; SelectOption(defaultOption); }
    public int GetSelectedOption() { return selectedOptionIndex; }
    public void SetDefaultOption(int _optionIndex) { defaultOption = _optionIndex; }
}
