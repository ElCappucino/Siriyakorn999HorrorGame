using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class IntroDialogueManager : MonoBehaviour
{
    [SerializeField] private List<string> monologues = new List<string>();
    [SerializeField] private MMF_Player dialogueFeedback;
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private GameObject GotoTutorialButton;
    private int currentMonologueIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RevealNextText();
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RevealNextText();
        }
    }
    public void RevealNextText()
    {
        if (currentMonologueIndex > monologues.Count - 1)
        {
            var textReveal = dialogueFeedback.GetFeedbackOfType<MoreMountains.Feedbacks.MMF_TMPTextReveal>();
            textReveal.ReplaceText = true;
            textReveal.HideTextOnInitialization = true;
            textReveal.NewText = "";
            dialogueFeedback.PlayFeedbacks();

            cameraControl.isRoundStart = true;
            GotoTutorialButton.SetActive(true);
        }
        else
        {
            var textReveal = dialogueFeedback.GetFeedbackOfType<MoreMountains.Feedbacks.MMF_TMPTextReveal>();
            textReveal.ReplaceText = true;
            textReveal.HideTextOnInitialization = true;
            textReveal.NewText = monologues[currentMonologueIndex];
            dialogueFeedback.PlayFeedbacks();

            currentMonologueIndex++;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
