using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
   #region Variables

   [SerializeField] private int gateNum;
   [SerializeField] private string gateType;
   [SerializeField] private bool isAvailable;
   [SerializeField] private BoxCollider2D col;
   #endregion

   #region Mono

   private void OnEnable()
   {
      GameEvent.Instance.OnToggleGateSelectable += ToggleSelectable;
   }

   private void OnDisable()
   {
      GameEvent.Instance.OnToggleGateSelectable -= ToggleSelectable;
   }

   #endregion
   #region Methods

   public void OnClicked()
   {
      if (gateType == "In")
      {
         //spawn car
         return;
      }

      if (gateType == "Out")
      {
         return;
      }
   }

   private void ToggleSelectable(int gateNum, string type, bool isSelectable)
   {
      if (gateNum != this.gateNum) return;
      if (type != gateType) return;
      col.enabled = isSelectable;

   }
   #endregion

   #region Setter/getter

   public Vector2 GetCurrentTransform()
   {
      return transform.position;
   }
   
   

   #endregion
}
