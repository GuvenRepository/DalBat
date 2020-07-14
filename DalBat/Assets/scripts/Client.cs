using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Client : MonoBehaviour {

    public string clientName = "Client";
    public static int playerPosition = -1;
    public List<GameClient> otherPlayers = new List<GameClient>();

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public gameManager GM;
    cardDistrubuter CD;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
            return false;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error " + e.Message);
        }

        return socketReady;
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    //Sending messages to the server
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        string[] aData = data.Split('|');

        Debug.Log(data);

        switch (aData[0])
        {
            case "SWHO":
                for (int i = 1; i < aData.Length - 1; i = i + 2)
                {
                    GameClient c = new GameClient();
                    c.name = aData[i];
                    c.playerPosition = int.Parse(aData[i + 1]);
                    otherPlayers.Add(c);
                }
                
                Send("CWHO|" + clientName);
                break;

            case "SCNN":
                GameClient cl = new GameClient();
                cl.name = aData[1];
                cl.playerPosition = int.Parse(aData[2]);
                otherPlayers.Add(cl);
                try
                {
                    GM.printNames();
                }
                catch { }
                break;
            case "SPSTN":
                playerPosition = int.Parse(aData[1]);
                Send("CSTRT");
                break;
            case "SSTRT":
                try
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;

            case "STURN":
                if (playerPosition==int.Parse(aData[1]))
                {
                    gameManager.yourTurn = true;
                }
                break;

            case "STCRD":
                CD = FindObjectOfType<cardDistrubuter>();
                int playerPos = int.Parse(aData[1]);
                if (playerPos != playerPosition)
                {
                    CD.takeCard(playerPos);
                }
                break;

            case "STHRW":
                GM = FindObjectOfType<gameManager>();
                int throwPlayerPos = int.Parse(aData[1]);
                if (throwPlayerPos != playerPosition)
                {
                    GM.throwCard(throwPlayerPos,short.Parse(aData[2]));
                    if (throwPlayerPos % 4 + 1 == playerPosition)
                    {
                        gameManager.yourTurn = true;
                        cardDistrubuter.cardTaken = false;
                    } 
                }
                
                break;

            case "STGND":
                CD = FindObjectOfType<cardDistrubuter>();
                int playerIndex = int.Parse(aData[1]);
                if (playerIndex != playerPosition)
                {
                    CD.takeGround(playerIndex);
                }
                break;


            case "SOPNC":
                playerIndex = int.Parse(aData[1]);
                for (int i = 2; i < aData.Length && aData[i] != ""; i++)
                {
                    miniCard cardInstance = Instantiate(GM.miniCard).GetComponent<miniCard>();
                    cardInstance.cardIndex = short.Parse(aData[i]);
                    cardInstance.showFace();
                    gameManager.openedCards[playerIndex].Add(cardInstance);
                    GM.openedArranger(playerIndex);
                }
                break;
        }

        
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        stream.Close();
        socket.Close();
        socketReady = false;
    }

}

public class GameClient
{
    public string name;
    public int playerPosition;
    public bool isHost;
}