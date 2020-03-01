using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkingUpdater : MonoBehaviour
{
    //float INC = 0f;
    //float MAX = 0.1f;
    // Update is called once per frame
    void Update()
    {
        if (NetworkingScript.CONNECTION)
        {
            NetworkingScript.ProcessPackets();

            //INC += Time.deltaTime;
            //if (INC > MAX && NetworkingScript.MY_ID >= 0)
            //{
            //    NetworkingScript.SendPlayerData();
            //    while (INC > MAX)
            //        INC -= MAX;
            //}
        }
    }
}
