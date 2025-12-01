using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    private UserPartyNetwork _partyNetwork;
    public UserPartyNetwork PartyNetwork
    {
        get
        {
            if (_partyNetwork == null)
            { 
                _partyNetwork = new UserPartyNetwork();
            }
            return _partyNetwork;
        }
    }
    private GachaNetwork _gachaNetwork;
    public GachaNetwork GachaNetwork
    {
        get
        {
            if (_gachaNetwork == null)
                _gachaNetwork = new GachaNetwork();
            return _gachaNetwork;
        }
    }
    public const int BATTLE_ADVENTURE = 1;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
