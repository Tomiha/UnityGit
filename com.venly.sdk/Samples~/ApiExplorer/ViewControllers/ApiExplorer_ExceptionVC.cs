using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ApiExplorer_ExceptionVC : MonoBehaviour
{
    public bool IsActive = true;

    private UIDocument _ui;
    private Label _exceptionMsg;

    // Start is called before the first frame update
    void OnEnable()
    {
        _ui = GetComponent<UIDocument>();

        _exceptionMsg = _ui.rootVisualElement.Q<Label>("lbl-exception");

        _ui.rootVisualElement.Q<Button>("btn-ok")
            .clickable.clicked += Hide;
    }

    public void Show(Exception ex)
    {
        gameObject.SetActive(true);

        _exceptionMsg.text = ex.Message;

        Debug.Log(ex);

        IsActive = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }
}
