using System;
using System.Collections;
using System.Collections.Generic;
using Shapes2D;
using UnityEngine;

public class Tile : MonoBehaviour
{
   #region Variables

   [Header("Pos")] 
   [SerializeField] private Vector2Int id;
   [Header("Stats")]
   [SerializeField] private bool isAvailable;

   [SerializeField] private bool isSelected;
   [SerializeField] private bool isGate;
   [SerializeField] private bool isParkingSlot;
   [SerializeField] private bool canMoveLeft;
   [SerializeField] private bool canMoveRight;
   [SerializeField] private bool canMoveUp;
   [SerializeField] private bool canMoveDown;

   [Header("Visual")] 
   [SerializeField] private Shape shape;
   #endregion

   #region Mono

   private void OnEnable()
   {
      GameEvent.Instance.OnHighlightAssignedTile += HightlightTile;
      GameEvent.Instance.OnUnHighlightAssignedTile += UnHighlightTile;
   }

   private void OnDisable()
   {
      GameEvent.Instance.OnHighlightAssignedTile -= HightlightTile;
      GameEvent.Instance.OnUnHighlightAssignedTile -= UnHighlightTile;
   }

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

   public void SetTileAvailable(bool isAvailable)
   {
      this.isAvailable = isAvailable;
   }
   public void SetTileIsSelected(bool isSelected)
   {
      this.isSelected = isSelected;
   }
   public bool IsAvailable()
   {
      return isAvailable;
   }

   private void HightlightTile(Vector2Int tileID)
   {
      if (tileID != id) return;
      if(isSelected) return;
      ChangeColor(EffectData.Instance.tileHighlightColor);
   }

   private void UnHighlightTile(Vector2Int tileID)
   {
      if (tileID != id) return;
      if (!isSelected) return;
      if (isParkingSlot)
      {
         ChangeColor(EffectData.Instance.tileParkingSlotColor);
      }
      else
      {
         ChangeColor(EffectData.Instance.tileNormalColor);
      }
   }

   private void ChangeColor(Color32 color)
   {
      shape.settings.fillColor = color;
   }
   #endregion
}
