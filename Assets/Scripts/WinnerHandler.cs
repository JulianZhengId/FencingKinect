using TMPro;
using UnityEngine;

public class WinnerHandler : MonoBehaviour
{
    private TextMeshProUGUI winnerText;

    private void Awake()
    {
        winnerText = GetComponent<TextMeshProUGUI>();   
    }

    private void Start()
    {
        winnerText.text = "\"" + GameManager.instance.winner + "\"";
    }
}
