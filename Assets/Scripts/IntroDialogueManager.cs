using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;

public class IntroDialogueManager : MonoBehaviour
{
    [SerializeField] private List<string> monologues = new List<string>();
    [SerializeField] private MMF_Player dialogueFeedback;
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private GameObject GotoTutorialButton;
    private int currentMonologueIndex = 0;
    private bool isAbleToClick = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RevealNextText();
        StartCoroutine(EnableClick());
    }
    IEnumerator EnableClick()
    {
        yield return new WaitForSeconds(0.2f);
        isAbleToClick = true;
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed && isAbleToClick)
        {
            Debug.Log("OnClick");
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
            currentMonologueIndex = 0;
            isAbleToClick = false ;
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
