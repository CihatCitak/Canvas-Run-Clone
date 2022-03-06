using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Doors
{
    public class DoorArea : MonoBehaviour
    {
        public Door LeftDoor;
        public Door RightDoor;

        public bool IsTriggered = false;

        public void ADoorTrigger(Door door)
        {
            if (!IsTriggered)
            {
                IsTriggered = true;

                LeftDoor.NoMoreTrigger();
                RightDoor.NoMoreTrigger();

                if (door.DoorType == DoorType.LENGTH)
                    CanvasController.Instance.AddPieceForLength(door.Times);
                else
                    CanvasController.Instance.AddPieceForWidth(door.Times);

            }
        }

    }

    public enum DoorType
    {
        WIDTH,
        LENGTH
    }
}
