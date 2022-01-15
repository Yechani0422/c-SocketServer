using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField]
    GameObject scoreBoardItem;
    [SerializeField]
    Transform scoreBoardPlayerList;

    [SerializeField]
    ConnectionManager manager;
    private void OnEnable()
    {
        ArrayList playerList = manager.playerLists;
        foreach(GameObject player in playerList)
        {
            Debug.Log(player.GetComponent<PlayerCtrl>().name);
            GameObject itemGO=Instantiate(scoreBoardItem, scoreBoardPlayerList);
            ScoreBoardItem item = itemGO.GetComponent<ScoreBoardItem>();
            if(item!=null)
            {
                item.SetUp(player.GetComponent<PlayerCtrl>().playerName.text, player.GetComponent<PlayerCtrl>().Kills, player.GetComponent<PlayerCtrl>().Deaths);
                
            }
        }
    }

    private void OnDisable()
    {
        foreach(Transform child in scoreBoardPlayerList)
        {
            Destroy(child.gameObject);
        }
    }
}
