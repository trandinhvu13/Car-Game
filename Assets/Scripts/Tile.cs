using System;
using System.Collections;
using System.Collections.Generic;
using Shapes2D;
using UnityEditor;
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

   [SerializeField] private bool canBeAddedToPath;
   [SerializeField] private bool canBeRemovedFromPath;
   [SerializeField] private bool canBeSelected;

   [Header("Visual")] 
   [SerializeField] private Shape shape;

   [SerializeField] private GameObject upArrow;
   [SerializeField] private GameObject downArrow;
   [SerializeField] private GameObject leftArrow;
   [SerializeField] private GameObject rightArrow;

   [Header("Other Components")] [SerializeField]
   private BoxCollider2D col;
   #endregion

   #region Mono

   private void OnEnable()
   {
      GameEvent.Instance.OnHighlightAssignedTile += HightlightTile;
      GameEvent.Instance.OnUnHighlightAssignedTile += UnHighlightTile;
      GameEvent.Instance.OnShowDirectionArrow += ShowAvailablePathArrow;
      GameEvent.Instance.OnChangeCanBeAddedToPath += ChangeCanBeAddedToPath;
      GameEvent.Instance.OnHideDirectionArrow += HideAvailablePathArrow;
   }

   private void OnDisable()
   {
      GameEvent.Instance.OnHighlightAssignedTile -= HightlightTile;
      GameEvent.Instance.OnUnHighlightAssignedTile -= UnHighlightTile;
      GameEvent.Instance.OnShowDirectionArrow -= ShowAvailablePathArrow;
      GameEvent.Instance.OnChangeCanBeAddedToPath -= ChangeCanBeAddedToPath;
      GameEvent.Instance.OnHideDirectionArrow -= HideAvailablePathArrow;
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

   private void ShowAvailablePathArrow(Vector2Int id)
   {
      if (id != this.id) return;
      HideAvailablePathArrow(this.id);
      if(canMoveDown) downArrow.SetActive(true);
      if(canMoveUp) upArrow.SetActive(true);
      if(canMoveLeft) leftArrow.SetActive(true);
      if(canMoveRight) rightArrow.SetActive(true);
   }

   private void HideAvailablePathArrow(Vector2Int id)
   {
      if (id != this.id) return;
      upArrow.SetActive(false);
      downArrow.SetActive(false);
      leftArrow.SetActive(false);
      rightArrow.SetActive(false);
   }

   private void ChangeCanBeAddedToPath(Vector2Int id, bool canBeAddedToPath)
   {
      if (id != this.id) return;
      this.canBeAddedToPath = canBeAddedToPath;
   }

   private void ChangeCanBeSelected(bool canBeSelected)
   {
      col.enabled = canBeSelected;
   }

   private void OnSelectedPathPicker()
   {
      if(canbesel)
   }
   #endregion

   #region Getter/Setter
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
   
   #endregion
}
