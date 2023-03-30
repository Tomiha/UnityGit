using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ApiExplorer_InfoVC : MonoBehaviour
{
    public bool IsActive = true;

    private UIDocument _ui;
    private Label _infoMsg;

    // Start is called before the first frame update
    void OnEnable()
    {
        _ui = GetComponent<UIDocument>();

        _infoMsg = _ui.rootVisualElement.Q<Label>("lbl-info");

        _ui.rootVisualElement.Q<Button>("btn-ok")
            .clickable.clicked += Hide;
    }

    public void Show(string msg)
    {
        gameObject.SetActive(true);

        (_infoMsg).text = msg;

        IsActive = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }
}
