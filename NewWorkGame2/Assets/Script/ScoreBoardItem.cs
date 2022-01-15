using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardItem : MonoBehaviour
{
    [SerializeField]
    Text usernameText;

    [SerializeField]
    Text killsText;

    [SerializeField]
    Text deathText;

    public void SetUp(string username, int kills, int deaths)
    {
        usernameText.text = username;
        killsText.text = "Kills:" + kills;
        deathText.text = "Deaths:" + deaths;
    }
}
