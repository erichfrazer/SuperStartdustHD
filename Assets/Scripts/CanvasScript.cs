    using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    public GameObject ShieldText;
    public GameObject ScoreText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetShieldLevel(bool bIsShield, float fLevel)
    {
        UnityEngine.UI.Text t = ShieldText.GetComponent<UnityEngine.UI.Text>();
        if (bIsShield)
        {
            t.text = "SHIELD LEVEL: " + fLevel;
        }
        else
        {
            t.text = "HULL LEVEL: " + fLevel;
        }
    }

    public void SetScore(int Score)
    {
        TMP_Text t = ScoreText.GetComponent<TMP_Text>();
        t.text = "SCORE:" + Score;
    }
}
