using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    // Public inspector variables
    [Header("UI Manager Refererences")]
    public TransformAnimator[] uiPanels;
    public Button[] uiButtons;
    public TextMeshProUGUI[] uiTexts;
    public TextMeshPro[] ui3DTexts;

    // Private variables
    void Start()
    {
        // Smoothly display the main menu panel
        DisplayPanel(0);

        // Bind a button click handler to transition between "Game Over" to "Main Menu"
        uiButtons[3].onClick.AddListener(() =>
        {
            // Hide the game over panel and display the main menu panel
            AudioManager.Instance.PlayUIClick();
            HidePanel(2, () => DisplayPanel(0));
        });

        // Bind button click handlers for start game and play again buttons
        uiButtons[0].onClick.AddListener(() => PlayGame(0));
        uiButtons[2].onClick.AddListener(() => PlayGame(2));
    }

    void PlayGame(int fromPanelIndex)
    {
        // Hide the main menu panel and show the game panel
        AudioManager.Instance.PlayUIClick();
        HidePanel(fromPanelIndex, () => GameManager.Instance.StartGame());
        AudioManager.Instance.Play(0, 0);
    }

    public void DisplayPanel(int panelIndex, Action onComplete = null)
    {
        // Retrieve the panel
        TransformAnimator panel = uiPanels[panelIndex];

        // Smoothly animate the panel to the 0,0,0 position
        panel.targetPosition = Vector3.zero;

        // Pass the animation callback if one is specified
        if (onComplete != null) panel.OnceAnimationComplete(TransformAnimator.Properties.Position, onComplete);
    }

    public void HidePanel(int panelIndex, Action onComplete = null)
    {
        // Retrieve the panel
        TransformAnimator panel = uiPanels[panelIndex];

        // Smoothly animate the panel to the start position
        panel.targetPosition = panel.GetStartPosition();

        // Pass the animation callback if one is specified
        if (onComplete != null) panel.OnceAnimationComplete(TransformAnimator.Properties.Position, onComplete);
    }

    public void SetText(int textIndex, string text)
    {
        // Retrieve the text
        TextMeshProUGUI textMesh = uiTexts[textIndex];

        // Set the text
        textMesh.text = text;
    }

    public void Set3DText(int textIndex, string text)
    {
        // Retrieve the text
        TextMeshPro textMesh = ui3DTexts[textIndex];

        // Set the text
        textMesh.text = text;
    }
}
