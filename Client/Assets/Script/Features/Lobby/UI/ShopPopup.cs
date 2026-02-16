using Game.Lobby;
using Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPopup : UIPopup
{
    [Header("Shop")]
    [SerializeField] private CurrencyUI currencyUI;
    [SerializeField] private Transform[] TabBar_Areas;
    [SerializeField] private ObjectPool tabPool;

    [Header("Product")]
    [SerializeField] private ObjectPool productPool;

    private void OnEnable()
    {
        // [1] 재화 최신화 
        // 컴포넌트 패턴으로 재화 항상 팔로우 하게 하자. 


        // [2] 서버로 부터 활성화되어 있는 상점을 받아오기 

        
        
    }
}
