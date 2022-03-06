using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    #region Singleton

    public static CameraManager Instance { get { return instance; } }
    private static CameraManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            currentCamera = gameInCamera;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion


    [SerializeField] CinemachineVirtualCamera gameInCamera, winCamera, loseCamera;

    private CinemachineVirtualCamera currentCamera;

    public Vector3 GetCurrentCameraPosition()
    {
        return currentCamera.gameObject.transform.position;
    }

    public void WinTheGame()
    {
        currentCamera = winCamera;

        //gameInCamera.gameObject.SetActive(false);
        //winCamera.gameObject.SetActive(true);
        //loseCamera.gameObject.SetActive(false);
    }

    public void LoseTheGAme()
    {
        currentCamera = loseCamera;

        //gameInCamera.gameObject.SetActive(false);
        //winCamera.gameObject.SetActive(false);
        //loseCamera.gameObject.SetActive(true);
    }

    public void SetFollowTarget(Transform target)
    {
        currentCamera.Follow = target;
    }

    public void SetLookAtTarget(Transform target)
    {
        currentCamera.LookAt = target;
    }

    public void SetFollowDistance(float distance)
    {

    }
}
