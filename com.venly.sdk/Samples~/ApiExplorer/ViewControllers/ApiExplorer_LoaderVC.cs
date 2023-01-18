using UnityEngine;
using UnityEngine.UIElements;

public class ApiExplorer_LoaderVC : MonoBehaviour
{
    public bool IsActive = true;

    private UIDocument _ui;
    private float _totalRotation = 0;
    private VisualElement _loaderVisual;
    private Label _loaderText;

    // Start is called before the first frame update
    void OnEnable()
    {
        _ui = GetComponent<UIDocument>();

        _loaderVisual = _ui.rootVisualElement.Q<VisualElement>("loader-visual");
        _loaderText = _ui.rootVisualElement.Q<Label>("loader-text");
    }

    public void SetLoaderText(string txt)
    {
        _loaderText.ToggleVisibility(!string.IsNullOrEmpty(txt));
        _loaderText.text = txt;
    }

    public void Show(string txt = null)
    {
        gameObject.SetActive(true);
        SetLoaderText(txt);

        IsActive = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive)
        {
            _totalRotation += 90 * Time.deltaTime;
            _totalRotation %= 360;

            _loaderVisual.style.rotate = new Rotate(_totalRotation);
        }
    }
}
