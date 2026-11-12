using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] AIManager aIManager;
    [SerializeField] GameManager gameManager;
    [SerializeField] ARModelManager aRModelManager;
    [SerializeField] GameObject historyPanel;
    [SerializeField] TextMeshProUGUI historyTextBox;
    [SerializeField] Button sendMessageButton;
    [SerializeField] TMP_InputField messageInputField;
    [SerializeField] Button toggleHistoryButton;
    [SerializeField] Button placeModelButton;
    [SerializeField] Button setAIServerButton;
    [SerializeField] TMP_InputField aiServerAddressField;
    [SerializeField] TMP_InputField aiModelInputField;
    [SerializeField] Slider scaleSlider;
    
    private string history;


    void Start()
    {
        sendMessageButton.onClick.AddListener(SendMessageButtonClicked);
        toggleHistoryButton.onClick.AddListener(ToggleHistoryPanel);
        placeModelButton.onClick.AddListener(PlaceModelButtonClicked);
        setAIServerButton.onClick.AddListener(SetAIServerButtonClicked);
        scaleSlider.onValueChanged.AddListener(delegate { ScaleSliderValueChanged(); });
        historyTextBox.text = "";
    }

    private void SendMessageButtonClicked()
    {
        string userMessage = messageInputField.text;
        Debug.Log("Send Message button!" + userMessage);
        if (userMessage.Trim().Length >= 1)
        {
            history += "> " + userMessage + "\n";
            historyTextBox.text = history;
            aIManager.Send(userMessage);
        }
    }

    private void PlaceModelButtonClicked()
    {
        gameManager.PlaceModel();
    }

    public void AddNewMessageToHistory(string message)
    {
        history += message + "\n";
        historyTextBox.text = history;
    }

    public void AlertMessageFailure()
    {
        history += "<color=#ff0000><i>메시지 전송 실패. 다시 시도하십시오.</i></color>";
        historyTextBox.text = history;
    }

    public void ToggleHistoryPanel()
    {
        historyPanel.SetActive(!historyPanel.activeSelf);
        Debug.Log("history toggle");
    }

    public void SetAIServerButtonClicked()
    {
        aIManager.endpoint = aiServerAddressField.text;
        aIManager.model = aiModelInputField.text;
        Debug.Log("AI Endpoint set");
    }

    public void ScaleSliderValueChanged()
    {
        aRModelManager.ChangeModelScale(scaleSlider.value);
    }
}