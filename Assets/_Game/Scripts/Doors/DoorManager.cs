using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Doors
{
    public class DoorManager : MonoBehaviour
    {

        #region Singleton

        public static DoorManager Instance { get { return instance; } }
        private static DoorManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                doorAreas = FindObjectsOfType<DoorArea>();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #endregion

        private DoorArea[] doorAreas;

        public void UpdateDoorsPieceValues(int widthValue, int lengthValue, int column)
        {
            foreach (DoorArea doorArea in doorAreas)
            {
                if (doorArea.RightDoor.DoorType == DoorType.LENGTH)
                {
                    doorArea.RightDoor.PieceCountText.SetText("+" + (doorArea.RightDoor.Times * column + lengthValue));
                }
                else
                {
                    doorArea.RightDoor.PieceCountText.SetText("+" + doorArea.RightDoor.Times * widthValue);
                }

                if (doorArea.LeftDoor.DoorType == DoorType.LENGTH)
                {
                    doorArea.LeftDoor.PieceCountText.SetText("+" + (doorArea.LeftDoor.Times * column + lengthValue));
                }
                else
                {
                    doorArea.LeftDoor.PieceCountText.SetText("+" + doorArea.LeftDoor.Times * widthValue);
                }

            }
        }
    }
}
