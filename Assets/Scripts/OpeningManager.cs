using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpeningManager : MonoBehaviour
{
    public TMP_InputField IP;
    public TMP_InputField PORT;
    public TMP_InputField USERNAME;
    public static string uname = "";

    private void Awake()
    {
        NetworkingScript.StartUp();
    }

    public void CONNECT_TO_SERVER()
    {
        NetworkingScript.JoinServer(IP.text, PORT.text);

    }

    private void Update()
    {
        uname = USERNAME.text;
    }
    private void OnApplicationQuit()
    {
        NetworkingScript.CleanUpWINSOCK();
    }
}
