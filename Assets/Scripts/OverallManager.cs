using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverallManager : MonoBehaviour
{
    public TMP_InputField typing;
    public TMP_InputField mainChat;
    public Button SEND;
    public Button LEAVE;
    
    public TMP_InputField[] textboxes;
    public Button[] butts;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < NetworkingScript.MAX_USERS; ++i)
        {
            bool SKIP = NetworkingScript.allClients[i].SKIPPED;
            butts[i].gameObject.SetActive(!SKIP);
            textboxes[i].gameObject.SetActive(!SKIP);
            if (!SKIP)
                butts[i].GetComponentInChildren<TMP_Text>().SetText(NetworkingScript.allClients[i].USERNAME);
            if (SKIP)
            {
                textboxes[i].SetTextWithoutNotify("-offline");
            }
            else if (((1 << i) & NetworkingScript.allClients[NetworkingScript.MY_ID].requestedPartners) > 0)
            {
                textboxes[i].SetTextWithoutNotify("-request game with " + NetworkingScript.allClients[i].USERNAME);
            }
            else if (((1 << NetworkingScript.MY_ID) & NetworkingScript.allClients[i].requestedPartners) > 0)
            {
                textboxes[i].SetTextWithoutNotify("-wants a game with you");
            }
            else if (NetworkingScript.allClients[i].currentPartner >= 0)
            {
                textboxes[i].SetTextWithoutNotify("-ingame with " + 
                    NetworkingScript.allClients[NetworkingScript.allClients[i].currentPartner].USERNAME);
            }
            else
            {
                textboxes[i].SetTextWithoutNotify("-online");
            }
        }

        string allText = "";
        for (int i = 0; i < NetworkingScript.texts.Count; ++i)
        {
            allText += NetworkingScript.texts[i];
            if (i < NetworkingScript.texts.Count - 1)
                allText += "\n";
        }

        mainChat.text = allText;
    }

    public void LEAVE_SESSION()
    {
        if (NetworkingScript.allClients[NetworkingScript.MY_ID].currentPartner >= 0)
        {
            NetworkingScript.SendJoinRequest(-1);
        }
    }

    public void SendJoinReq0()
    {
        NetworkingScript.SendJoinRequest(0);
    }
    public void SendJoinReq1()
    {
        NetworkingScript.SendJoinRequest(1);
    }
    public void SendJoinReq2()
    {
        NetworkingScript.SendJoinRequest(2);
    }
    public void SendJoinReq3()
    {
        NetworkingScript.SendJoinRequest(3);
    }
    public void SendJoinReq4()
    {
        NetworkingScript.SendJoinRequest(4);
    }

    public void SendJoinReq5()
    {
        NetworkingScript.SendJoinRequest(5);
    }

    public void SendJoinReq6()
    {
        NetworkingScript.SendJoinRequest(6);
    }
    public void SendJoinReq7()
    {
        NetworkingScript.SendJoinRequest(7);
    }
    public void SendJoinReq8()
    {
        NetworkingScript.SendJoinRequest(8);
    }
    public void SendJoinReq9()
    {
        NetworkingScript.SendJoinRequest(9);
    }
    public void SendJoinReq10()
    {
        NetworkingScript.SendJoinRequest(10);
    }

    public void SendJoinReq11()
    {
        NetworkingScript.SendJoinRequest(11);
    }

    public void SEND_MSG()
    {
        if (typing.text.Length > 0)
            NetworkingScript.SendMessage(typing.text);
        //Debug.Log(typing.text.Length);
    }

    private void OnApplicationQuit()
    {
        NetworkingScript.CleanUpWINSOCK();
    }
}
