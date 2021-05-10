using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
   #region Variables

   [Header("Pos")] 
   [SerializeField] private Vector2Int id;
   [Header("Stats")]
   [SerializeField] private bool isAvailable;
   [SerializeField] private bool isGate;
   [SerializeField] private bool canMoveLeft;
   [SerializeField] private bool canMoveRight;
   [SerializeField] private bool canMoveUp;
   [SerializeField] private bool canMoveDown;

   #endregion

   #region Mono



   #endregion

   #region Methods
   public void AssignPos(int x, int y)
   {
      id.x = x;
      id.y = y;
   }
   public bool CanMoveDirection(string dir)
   {
      if (dir == "left")
      {
         return canMoveLeft;
      }
      if (dir == "right")
      {
         return canMoveRight;
      }
      if (dir == "up")
      {
         return canMoveUp;
      }
      if (dir == "down")
      {
         return canMoveDown;
      }

      return false;
   }

   public Transform GetCurrentTransform()
   {
      return gameObject.transform;
   }

   public void SetTileStatus(bool isAvailable)
   {
      this.isAvailable = isAvailable;
   }

   public bool IsAvailable()
   {
      return isAvailable;
   }
   #endregion
}
