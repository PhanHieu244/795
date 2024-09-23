using System.Collections;
using System.Collections.Generic;
using OnefallGames;
using UnityEngine;

public class PurchasingManager : MonoBehaviour
{
   public void OnPressDown(int i)
   {
      switch (i)
      {
         case 1:
            AddDiamond(100);
             IAPManager.Instance.BuyProductID(IAPKey.PACK1);
            break;
         case 2:
            AddDiamond(300);
            IAPManager.Instance.BuyProductID(IAPKey.PACK2);
            break;
         case 3:
            AddDiamond(500);
            IAPManager.Instance.BuyProductID(IAPKey.PACK3);
            break;
         case 4:
            AddDiamond(1000);
            IAPManager.Instance.BuyProductID(IAPKey.PACK4);
            break;
      }
   }

   private void AddDiamond(int a)
   {
       CoinManager.Instance.AddCoins(a);
   }
}
