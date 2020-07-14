using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class netManager : MonoBehaviour {

    public static netManager Instance { set; get; }

    public GameObject mainMenu,connectingMenu;
    public InputField hostIPInput,nameInput;
    public GameObject serverPrefab;
    public GameObject clientPrefab;

	
	void Start () {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //hostIPInput.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
	}
	
    public void ConnectButton()
    {
        string hostAddress = hostIPInput.text;//"127.0.0.1";

        if (hostAddress == null || hostAddress == "")
        {
            hostAddress = "127.0.0.1";
        }

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            if (c.clientName == null || c.clientName == "")
            {
                c.clientName = "Client";
            }
            c.ConnectToServer(hostAddress, 5565);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        connectingMenu.SetActive(true);
    }

    public void HostButton()
    {
        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = "Host";
            c.ConnectToServer("127.0.0.1", 5565);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
    }


    public void CancelButton()
    {
        Client c = FindObjectOfType<Client>();

        if (c != null)
            Destroy(c.gameObject);

        mainMenu.SetActive(true);
        connectingMenu.SetActive(false);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
