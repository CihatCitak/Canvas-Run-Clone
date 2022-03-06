using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Doors
{
    public class Door : MonoBehaviour
    {
        [SerializeField] DoorArea doorArea;
        public TextMeshProUGUI PieceCountText;
        public DoorType DoorType;
        public int Times;
        [SerializeField] Collider boxCollider;

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("CanvasRoot"))
            {
                doorArea.ADoorTrigger(this);
            }
        }

        public void NoMoreTrigger()
        {
            boxCollider.enabled = false;
        }
    }
}



