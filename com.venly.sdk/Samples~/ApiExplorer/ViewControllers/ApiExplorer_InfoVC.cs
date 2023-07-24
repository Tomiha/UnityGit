using UnityEngine;
using UnityEngine.UIElements;

public class ApiExplorer_InfoVC : MonoBehaviour
{
    public bool IsActive = true;

    private UIDocument _ui;
    private Label _infoMsg;
    private static Color _defaultColor = new Color(0.06f,0.9f,0.15f,1.0f);


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
        Show(msg, _defaultColor);
    }

    public void Show(string msg, Color c)
    {
        gameObject.SetActive(true);

        _infoMsg.text = msg;
        _infoMsg.style.color = new StyleColor(c);

        IsActive = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }
}
