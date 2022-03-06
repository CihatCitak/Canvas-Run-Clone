using Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;
using TMPro;

public class CanvasController : MonoBehaviour
{
    #region Singleton

    public static CanvasController Instance { get { return instance; } }
    private static CanvasController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    #region CanvasState

    public enum CanvasStates
    {
        START,
        RUN,
        GAMEEND
    }
    private CanvasStates canvasState;

    public void StartRun()
    {
        canvasState = CanvasStates.RUN;
    }

    public void LoseTheGame()
    {
        canvasState = CanvasStates.GAMEEND;
        totalCountUI.gameObject.SetActive(false);
    }

    public void WinTheGame()
    {
        canvasState = CanvasStates.GAMEEND;
    }

    #endregion

    [SerializeField] int row, column;
    [SerializeField] float pieceOffset = 0.3f;
    [SerializeField] InputSettings inputSettings;
    [SerializeField] RectTransform totalCountUI;
    [SerializeField] TextMeshProUGUI totalCountText;

    [SerializeField]
    [BoxGroup("Color Settings")]
    Color topColor, bottomColor;

    [SerializeField]
    [BoxGroup("Speed Settings")]
    float sideMovementSensitivity = 1f,
        forwardMovementSpeed = 1f, sideFollowSpeed = 0.3f,
        rotationSpeed = 1f, rotationFollowSpeed;

    [SerializeField]
    [BoxGroup("Limit")]
    Transform leftLimitTransform, rightLimitTransform;

    private MaterialPropertyBlock propertyBlock;
    private List<List<GameObject>> canvasMatris = new List<List<GameObject>>();
    private List<Transform> rowTransforms = new List<Transform>();

    private float lastConnectTime = 0f, afterConnectShiftingStartTime = 1f;
    private bool isNeedShifting = false;
    private int firstColumnPieceCount, lastColumnPieceCount, pieceWidthAddValue, lostPieceLenthAddValue, totalPieceCount;

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();

        CreateTile();

        CameraSettings();

        totalCountText.SetText(totalPieceCount.ToString());
        totalCountUI.parent = rowTransforms[0];

        UpdateAllDoorsValues();
    }

    private void Update()
    {
        if (canvasState == CanvasStates.RUN)
        {
            if (isNeedShifting && (lastConnectTime + afterConnectShiftingStartTime) < Time.time)
            {
                ShiftMatris();
                isNeedShifting = false;
            }
        }

    }

    void FixedUpdate()
    {
        if (canvasState == CanvasStates.RUN)
        {
            HandleMovement();
            totalCountText.SetText(totalPieceCount.ToString());
        }
    }

    #region Movement

    private void HandleMovement()
    {
        //CanvasRoot movement
        transform.position += Vector3.forward * forwardMovementSpeed;

        FirstRowMovement();
        FirstRowRotation();

        RowsFollow();
    }

    private void FirstRowMovement()
    {
        var localPos = rowTransforms[0].localPosition;
        localPos += Vector3.right * inputSettings.InputDrag.x * sideMovementSensitivity;

        var limits = CalculateLimits();
        localPos.x = Mathf.Clamp(localPos.x, limits.x, limits.y);

        rowTransforms[0].localPosition = localPos;
    }

    private void FirstRowRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(CalculateMoveDirection(), Vector3.up);
        rowTransforms[0].localRotation = Quaternion.Lerp(rowTransforms[0].localRotation, targetRotation, rotationSpeed);
    }

    private void RowsFollow()
    {
        //Rows follow the previous one row
        for (int i = 1; i < rowTransforms.Count; i++)
        {
            //Movement Follow
            var position = Vector3.Lerp(rowTransforms[i].localPosition, rowTransforms[i - 1].localPosition, sideFollowSpeed);
            rowTransforms[i].localPosition = new Vector3(position.x, rowTransforms[i].localPosition.y, rowTransforms[i].localPosition.z);

            //Rotation Follow
            var rotation = Quaternion.Lerp(rowTransforms[i].localRotation, rowTransforms[i - 1].localRotation, rotationFollowSpeed);
            rowTransforms[i].localRotation = rotation;
        }
    }

    private Vector2 CalculateLimits()
    {
        return new Vector2(leftLimitTransform.localPosition.x + ((float)column / 2f) * pieceOffset,
            rightLimitTransform.localPosition.x - ((float)column / 2f) * pieceOffset);
    }

    private Vector3 CalculateMoveDirection()
    {
        Vector3 moveDirection = Vector3.forward * 0.5f;
        moveDirection += rowTransforms[0].right * inputSettings.InputDrag.x * sideMovementSensitivity;

        return moveDirection.normalized;
    }

    #endregion

    #region TileIssues

    private void CreateTile()
    {
        for (int r = 0; r < row; r++)
        {
            GameObject tempRow = new GameObject("Row");
            tempRow.transform.SetParent(transform);
            tempRow.transform.localPosition = new Vector3(0f, 0f, -pieceOffset * r);

            if (r == 0)
            {
                AddColliderToFirstRow(tempRow);
            }

            rowTransforms.Add(tempRow.transform);
            canvasMatris.Add(new List<GameObject>());

            for (int c = 0; c < column; c++)
            {
                GameObject tempGO = ObjectPooler.Instance.DequeueFromPool("CanvasPiece");

                PieceTransformSettings(tempGO, tempRow.transform, c);

                canvasMatris[r].Add(tempGO);

                SetColor(tempGO.GetComponent<Renderer>(), r);

                totalPieceCount++;
            }
        }
    }

    private void SetColor(Renderer renderer, int r)
    {
        float colmunRatio = 1f - ((float)(row - r) / (float)row);

        propertyBlock.SetColor("_Color", Color.Lerp(topColor, bottomColor, colmunRatio));
        renderer.SetPropertyBlock(propertyBlock);
    }

    private float PieceTransformSettings(GameObject go, Transform parent, int c)
    {
        var xPosition = ((float)(column / 2f) * -pieceOffset) + pieceOffset / 2f + (c * pieceOffset);

        go.transform.SetParent(parent);
        go.transform.localPosition = new Vector3(xPosition, 0f, 0f);

        return xPosition;
    }

    private void AddColliderToFirstRow(GameObject firstRow)
    {
        //For Door coliision
        firstRow.AddComponent<SphereCollider>();

        SphereCollider collider = firstRow.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.up;
        collider.radius = 0.5f;

        firstRow.layer = 7;
        firstRow.tag = "CanvasRoot";
    }

    public void AddPieceForLength(int times)
    {
        //Full empty space
        for (int r = 0; r < canvasMatris.Count; r++)
        {
            if (canvasMatris[r].Count < column)
            {
                var emptyCount = column - canvasMatris[r].Count;

                for (int i = 0; i < emptyCount; i++)
                {
                    GameObject tempGO = ObjectPooler.Instance.DequeueFromPool("CanvasPiece");

                    var xPosition = ((float)(column / 2f) * -pieceOffset) + pieceOffset / 2f + (canvasMatris[r].Count * pieceOffset);

                    tempGO.transform.parent = rowTransforms[r];

                    tempGO.transform.localPosition = -Vector3.forward * 10f + Vector3.right * xPosition;
                    tempGO.transform.DOLocalMoveZ(0f, 1f);

                    canvasMatris[r].Add(tempGO);
                    totalPieceCount++;
                }
            }
        }


        for (int i = 0; i < times; i++)
        {
            GameObject tempRow = new GameObject("Row");

            tempRow.transform.SetParent(transform);
            tempRow.transform.localPosition = Vector3.forward * -pieceOffset * row - Vector3.forward * 5f;
            tempRow.transform.DOLocalMoveZ(-pieceOffset * row, 1f);

            rowTransforms.Add(tempRow.transform);
            canvasMatris.Add(new List<GameObject>());

            for (int c = 0; c < column; c++)
            {
                GameObject tempGO = ObjectPooler.Instance.DequeueFromPool("CanvasPiece");

                PieceTransformSettings(tempGO, tempRow.transform, c);

                canvasMatris[row].Add(tempGO);
                totalPieceCount++;
            }

            row++;
        }

        ShiftMatris();
    }

    public void AddPieceForWidth(int times)
    {
        CalculateFirstAndLastColumnCount();

        int oldColumn = column;
        column += 4 * times;

        //Last column object add;
        for (int i = 0; i < times; i++)
        {
            for (int r = 0; r < lastColumnPieceCount; r++)
            {
                var tempGO = ObjectPooler.Instance.DequeueFromPool("CanvasPiece");

                tempGO.transform.SetParent(canvasMatris[r][oldColumn - 1].transform.parent);
                tempGO.transform.localPosition = canvasMatris[r][oldColumn - 1].transform.localPosition;

                SetColor(tempGO.GetComponent<Renderer>(), r);

                canvasMatris[r].Insert(oldColumn - 1, tempGO);
                totalPieceCount++;
            }
        }

        //First column object add;
        for (int i = 0; i < times; i++)
        {
            for (int r = 0; r < row; r++)
            {
                //New First Column creating
                var tempGO = ObjectPooler.Instance.DequeueFromPool("CanvasPiece");

                tempGO.transform.SetParent(canvasMatris[r][0].transform.parent);
                tempGO.transform.localPosition = canvasMatris[r][0].transform.localPosition;

                SetColor(tempGO.GetComponent<Renderer>(), r);

                canvasMatris[r].Insert(0, tempGO);
                totalPieceCount++;
            }
        }

        ShiftMatris();

    }

    private void CalculateFirstAndLastColumnCount()
    {
        lastColumnPieceCount = 0;
        for (int r = 0; r < canvasMatris.Count; r++)
        {
            if (canvasMatris[r].Count == column)
            {
                lastColumnPieceCount++;
            }
        }
        firstColumnPieceCount = row;

        pieceWidthAddValue = firstColumnPieceCount + lastColumnPieceCount;
    }

    public void UpdateAllDoorsValues()
    {
        CalculateFirstAndLastColumnCount();
        CalculataLostPieceCount();

        Doors.DoorManager.Instance.UpdateDoorsPieceValues(pieceWidthAddValue, lostPieceLenthAddValue, column);
    }
    #endregion

    #region Shifting

    private void ShiftMatris()
    {
        int[] columnObjectCount = new int[column];

        RowShifting(columnObjectCount);

        ClearMatrisColumns();

        int newColumnCount = FindnewColumnCount(columnObjectCount);
        column = newColumnCount;

        StartCoroutine(ColumnShifting(0f));

        ClearMatrisRows();

        UpdateAllDoorsValues();
    }

    private void RowShifting(int[] columnObjectCount)
    {
        var pieceCountInColums = PieceCountInColumns();

        //CanvasPiece Row Shifting
        for (int r = 0; r < canvasMatris.Count; r++)
        {
            for (int c = 0; c < canvasMatris[r].Count; c++)
            {
                if (!canvasMatris[r][c].activeSelf)
                {
                    for (int i = r; i < pieceCountInColums[c]; i++)
                    {
                        if (canvasMatris[i][c].activeSelf)
                        {
                            var gameObj = canvasMatris[i][c];
                            var parent = canvasMatris[i][c].transform.parent;
                            var localPosition = canvasMatris[i][c].transform.localPosition;

                            canvasMatris[i][c].transform.parent = canvasMatris[r][c].transform.parent;
                            canvasMatris[i][c].transform.DOLocalMoveZ(canvasMatris[r][c].transform.localPosition.z, 0.5f)
                                .SetEase(Ease.Linear);

                            canvasMatris[r][c].transform.parent = parent;
                            canvasMatris[r][c].transform.localPosition = localPosition;

                            canvasMatris[i][c] = canvasMatris[r][c];
                            canvasMatris[r][c] = gameObj;

                            columnObjectCount[c]++;

                            break;
                        }
                    }
                }
                else
                {
                    columnObjectCount[c]++;
                }
            }
        }
    }

    private IEnumerator ColumnShifting(float waitRowShiftingTime)
    {
        yield return new WaitForSeconds(waitRowShiftingTime);

        //Boþ columnlar ortadan kaldýrýldý
        for (int r = 0; r < canvasMatris.Count; r++)
        {
            for (int c = 0; c < canvasMatris[r].Count; c++)
            {
                //Column Transform Settings
                GameObject tempGO = canvasMatris[r][c];
                var xPosiiton = ((float)(column / 2f) * -pieceOffset) + pieceOffset / 2f + (c * pieceOffset);
                tempGO.transform.DOLocalMoveX(xPosiiton, 0.5f).SetEase(Ease.Linear);
                SetColor(tempGO.GetComponent<Renderer>(), r);
            }
        }
    }

    private void ClearMatrisColumns()
    {
        foreach (List<GameObject> row in canvasMatris.ToArray())
        {
            foreach (GameObject column in row.ToArray())
            {
                if (!column.activeSelf)
                {
                    ObjectPooler.Instance.EnqueueToPool("CanvasPiece", column);
                    row.Remove(column);
                }

            }
        }
    }

    private void ClearMatrisRows()
    {
        foreach (List<GameObject> row in canvasMatris.ToArray())
        {
            if (row.Count == 0)
            {
                int rowIndex = canvasMatris.IndexOf(row);
                Destroy(rowTransforms[rowIndex].gameObject);
                rowTransforms.RemoveAt(rowIndex);
                canvasMatris.Remove(row);
            }
        }

        row = canvasMatris.Count;
    }

    private int FindnewColumnCount(int[] columnObjectCount)
    {
        int newColumnCount = column;

        //Calculate which column empty
        for (int i = 0; i < columnObjectCount.Length; i++)
        {
            if (columnObjectCount[i] == 0)
            {
                newColumnCount--;
            }
        }

        return newColumnCount;
    }

    private int[] PieceCountInColumns()
    {
        int[] pieceCountInColums = new int[column];

        for (int r = 0; r < canvasMatris.Count; r++)
        {
            for (int c = 0; c < canvasMatris[r].Count; c++)
            {
                pieceCountInColums[c]++;
            }
        }

        return pieceCountInColums;
    }

    private void CalculataLostPieceCount()
    {
        lostPieceLenthAddValue = 0;
        for (int r = 0; r < canvasMatris.Count; r++)
        {
            if (canvasMatris[r].Count < column)
            {
                lostPieceLenthAddValue += column - canvasMatris[r].Count;
            }
        }
    }

    #endregion

    private void CameraSettings()
    {
        CameraManager.Instance.SetFollowTarget(rowTransforms[0]);
        CameraManager.Instance.SetLookAtTarget(rowTransforms[0]);
    }

    public void CanvasPieceConnect()
    {
        lastConnectTime = Time.time;
        isNeedShifting = true;

        totalPieceCount--;

        if(totalPieceCount == 0)
        {
            GameManagement.Instance.LoseTheGame();
        }
    }
}
