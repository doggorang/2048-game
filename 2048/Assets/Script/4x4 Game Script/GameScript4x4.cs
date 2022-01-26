﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Playing,
    GameOver,
    WaitingForMoveToEnd
}

public class GameScript4x4 : MonoBehaviour
{
    public GameState State;
    [Range(0, 2f)]
    public float delay;
    private bool moveMade;
    // bolean to check if move is done because it has delay so it doesnt move around
    private bool[] lineMoveComplete = new bool[4] { true, true, true, true };

    public Text TextDescriptionAlgorithm, TextDescriptionArchitecture;
    public Text GameOverText;
    public GameObject GameOverPanel;
    private Tile[,] AllTiles = new Tile[4, 4];
    private List<Tile[]> columns = new List<Tile[]>();
    private List<Tile[]> rows = new List<Tile[]>();
    private List<Tile> EmptyTiles = new List<Tile>();

    // variable untuk simpan 6 input layer
    private Tile HighestTile;
    private int SequenceTile = 0;
    private bool IsHighestTileCorner = false;
    private int SequenceMerge = 0;
    private int CountSmallTile = 0;
    private bool IsHighestTileDense = false;

    // Start is called before the first frame update
    void Start()
    {
        TextDescriptionAlgorithm.text = "Algorithm  - <b>" + AIController.algorithm + "</b>";
        TextDescriptionArchitecture.text = "Architecture - <b>" + AIController.architecture + "</b>";
        Tile[] AllTilesOneDim = GameObject.FindObjectsOfType<Tile>();
        foreach (Tile t in AllTilesOneDim)
        {
            t.Number = 0;
            AllTiles[t.indRow, t.indCol] = t;
            EmptyTiles.Add(t);
        }
        HighestTile = AllTiles[0, 0];
        columns.Add(new Tile[] { AllTiles[0, 0], AllTiles[1, 0], AllTiles[2, 0], AllTiles[3, 0] });
        columns.Add(new Tile[] { AllTiles[0, 1], AllTiles[1, 1], AllTiles[2, 1], AllTiles[3, 1] });
        columns.Add(new Tile[] { AllTiles[0, 2], AllTiles[1, 2], AllTiles[2, 2], AllTiles[3, 2] });
        columns.Add(new Tile[] { AllTiles[0, 3], AllTiles[1, 3], AllTiles[2, 3], AllTiles[3, 3] });

        rows.Add(new Tile[] { AllTiles[0, 0], AllTiles[0, 1], AllTiles[0, 2], AllTiles[0, 3] });
        rows.Add(new Tile[] { AllTiles[1, 0], AllTiles[1, 1], AllTiles[1, 2], AllTiles[1, 3] });
        rows.Add(new Tile[] { AllTiles[2, 0], AllTiles[2, 1], AllTiles[2, 2], AllTiles[2, 3] });
        rows.Add(new Tile[] { AllTiles[3, 0], AllTiles[3, 1], AllTiles[3, 2], AllTiles[3, 3] });
        Generate(); Generate();
    }

    private void GameOver(string text)
    {
        GameOverText.text = text;
        GameOverPanel.SetActive(true);
    }

    bool CanMove()
    {
        // if there is an empty tile that means you can still move
        if (EmptyTiles.Count > 0)
        {
            return true;
        }
        else
        {
            // if there are no move check if there are any tile that can merge
            // check columns
            for (int i = 0; i < columns.Count; i++)
                for (int j = 0; j < rows.Count - 1; j++)
                    if (AllTiles[j, i].Number == AllTiles[j + 1, i].Number)
                        return true;
            // check rows
            for (int i = 0; i < rows.Count; i++)
                for (int j = 0; j < columns.Count - 1; j++)
                    if (AllTiles[i, j].Number == AllTiles[i, j + 1].Number)
                        return true;
        }
        return false;
    }

    bool MakeOneMoveDownIndex(Tile[] LineOfTiles)
    {
        for (int i = 0; i < LineOfTiles.Length - 1; i++)
        {
            // check 1 block away if this tile empty and next tile has number then swicth place
            if (LineOfTiles[i].Number == 0 && LineOfTiles[i + 1].Number != 0)
            {
                LineOfTiles[i].Number = LineOfTiles[i + 1].Number;
                LineOfTiles[i + 1].Number = 0;
                // input layer 1
                if (LineOfTiles[i].Number > HighestTile.Number)
                {
                    HighestTile = LineOfTiles[i];
                }
                return true;
            }
            // merge tile if 2 colliding tile has the same number also check if the tile hasn't merge because only can merge once
            if (LineOfTiles[i].Number != 0 && LineOfTiles[i].Number == LineOfTiles[i + 1].Number && !LineOfTiles[i].mergeThisTurn && !LineOfTiles[i + 1].mergeThisTurn)
            {
                LineOfTiles[i].Number *= 2;
                LineOfTiles[i].mergeThisTurn = true;
                LineOfTiles[i + 1].Number = 0;
                LineOfTiles[i].PlayMergeAnimation();
                ScoreTracker4x4.Instance.Score += LineOfTiles[i].Number;
                if (LineOfTiles[i].Number == 2048)
                {
                    GameOver("You Win");
                }
                // input layer 1
                if (LineOfTiles[i].Number > HighestTile.Number)
                {
                    HighestTile = LineOfTiles[i];
                }
                return true;
            }
        }
        return false;
    }
    bool MakeOneMoveUpIndex(Tile[] LineOfTiles)
    {
        for (int i = LineOfTiles.Length - 1; i > 0; i--)
        {
            if (LineOfTiles[i].Number == 0 && LineOfTiles[i - 1].Number != 0)
            {
                LineOfTiles[i].Number = LineOfTiles[i - 1].Number;
                LineOfTiles[i - 1].Number = 0;
                // input layer 1
                if (LineOfTiles[i].Number > HighestTile.Number)
                {
                    HighestTile = LineOfTiles[i];
                }
                return true;
            }
            if (LineOfTiles[i].Number != 0 && LineOfTiles[i].Number == LineOfTiles[i - 1].Number && !LineOfTiles[i].mergeThisTurn && !LineOfTiles[i - 1].mergeThisTurn)
            {
                LineOfTiles[i].Number *= 2;
                LineOfTiles[i].mergeThisTurn = true;
                LineOfTiles[i - 1].Number = 0;
                LineOfTiles[i].PlayMergeAnimation();
                ScoreTracker4x4.Instance.Score += LineOfTiles[i].Number;
                if (LineOfTiles[i].Number == 2048)
                {
                    GameOver("You Win");
                }
                // input layer 1
                if (LineOfTiles[i].Number > HighestTile.Number)
                {
                    HighestTile = LineOfTiles[i];
                }
                return true;
            }
        }
        return false;
    }

    void Generate()
    {
        if (EmptyTiles.Count > 0)
        {
            int idxnewnumber = Random.Range(0, EmptyTiles.Count);
            int randomnum = Random.Range(0, 10);
            if (randomnum == 0)
                EmptyTiles[idxnewnumber].Number = 4;
            else
                EmptyTiles[idxnewnumber].Number = 2;

            EmptyTiles[idxnewnumber].PlayAppearAnimation();
            EmptyTiles.RemoveAt(idxnewnumber);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ResetMergedFlags()
    {
        foreach (Tile t in AllTiles)
            t.mergeThisTurn = false;
    }

    private void UpdateEmptyTiles()
    {
        CountSmallTile = 0;
        EmptyTiles.Clear();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // step 1 input layer 2 & 4
                Tile left, up, right, down;
                if (i == 0)
                {
                    up = null;
                }
                else
                {
                    up = AllTiles[i - 1, j];
                }

                if (i == 3)
                {
                    down = null;
                }
                else
                {
                    down = AllTiles[i + 1, j];
                }

                if (j == 0)
                {
                    left = null;
                }
                else
                {
                    left = AllTiles[i, j - 1];
                }

                if (j == 3)
                {
                    right = null;
                }
                else
                {
                    right = AllTiles[i, j + 1];
                }
                AllTiles[i, j].TileAround = new Tile[4] { left, up, right, down };

                // input layer 5
                if (AllTiles[i, j].Number > 0 && AllTiles[i, j].Number <= 32)
                    CountSmallTile++;

                if (AllTiles[i, j].Number == 0)
                    EmptyTiles.Add(AllTiles[i, j]);

            }
        }
    }

    private void GetInputLayer()
    {
        // step 2 & akhir input layer 2
        SequenceTile = 0;
        Tile tempSequenceTile = HighestTile;
        while (tempSequenceTile != null)
        {
            Tile nextTempTile = null;
            foreach (Tile item in tempSequenceTile.TileAround)
            {
                if (item != null)
                {
                    if (item.Number < tempSequenceTile.Number)
                    {
                        if (nextTempTile == null)
                        {
                            nextTempTile = item;
                        }
                        else if (item.Number < tempSequenceTile.Number && item.Number > nextTempTile.Number)
                        {
                            nextTempTile = item;
                        }
                    }
                }
            }
            if (nextTempTile != null)
                SequenceTile++;

            tempSequenceTile = nextTempTile;
        }

        // step 2 & akhir input layer 4
        SequenceMerge = 0;
        Tile tempSequenceMerge = HighestTile;
        while (tempSequenceMerge != null)
        {
            Tile nextTempTile = null;
            foreach (Tile item in tempSequenceMerge.TileAround)
            {
                if (item != null)
                {
                    if (item.Number == tempSequenceMerge.Number / 2)
                    {
                        nextTempTile = item;
                    }
                }
            }
            if (nextTempTile != null)
                SequenceMerge++;

            tempSequenceMerge = nextTempTile;
        }

        // input layer 3
        if (
            (HighestTile.indRow == 0 && HighestTile.indCol == 0) ||
            (HighestTile.indRow == 3 && HighestTile.indCol == 3) ||
            (HighestTile.indRow == 3 && HighestTile.indCol == 0) ||
            (HighestTile.indRow == 0 && HighestTile.indCol == 3)
        )
        {
            IsHighestTileCorner = true;
        }
        else
        {
            IsHighestTileCorner = false;
        }

        // input layer 6
        bool IsDense = true;
        // check coloumn highest tile apakah dense
        foreach (Tile column in columns[HighestTile.indCol])
        {
            if (column.Number == 0)
            {
                IsDense = false;
                break;
            }
        }
        // kalau di coloumn sudah dense langsung set else check row
        if (IsDense)
        {
            IsHighestTileDense = IsDense;
        }
        else
        {
            IsDense = true;
            // check ulang pada row apakah dense
            foreach (Tile row in rows[HighestTile.indRow])
            {
                if (row.Number == 0)
                {
                    IsDense = false;
                    break;
                }
            }
            IsHighestTileDense = IsDense;
        }
    }

    public void Move(MoveDirection md)
    {
        moveMade = false;
        ResetMergedFlags();
        if (delay > 0)
        {
            StartCoroutine(MoveCaroutine(md));
        }
        else
        {
            for (int i = 0; i < rows.Count; i++)
            {
                switch (md)
                {
                    case MoveDirection.Left:
                        while (MakeOneMoveDownIndex(rows[i]))
                            moveMade = true;
                        break;
                    case MoveDirection.Right:
                        while (MakeOneMoveUpIndex(rows[i]))
                            moveMade = true;
                        break;
                    case MoveDirection.Up:
                        while (MakeOneMoveDownIndex(columns[i]))
                            moveMade = true;
                        break;
                    case MoveDirection.Down:
                        while (MakeOneMoveUpIndex(columns[i]))
                            moveMade = true;
                        break;
                    default:
                        break;
                }
            }
            if (moveMade)
            {
                UpdateEmptyTiles();
                GetInputLayer();
                Debug.Log("Highest Tile: " + HighestTile.Number);
                Debug.Log("SequenceTile: " + SequenceTile);
                Debug.Log("IsHighestTileCorner: " + IsHighestTileCorner);
                Debug.Log("SequenceMerge: " + SequenceMerge);
                Debug.Log("CountSmallTile: " + CountSmallTile);
                Debug.Log("IsHighestTileDense: " + IsHighestTileDense);
                Generate();
                if (!CanMove())
                {
                    GameOver("You Lose");
                }
            }
        }
    }

    IEnumerator MoveOneLineUpIndexCoroutine(Tile[] line, int index)
    {
        lineMoveComplete[index] = false;
        while (MakeOneMoveUpIndex(line))
        {
            moveMade = true;
            yield return new WaitForSeconds(delay);
        }
        lineMoveComplete[index] = true;
    }
    IEnumerator MoveOneLineDownIndexCoroutine(Tile[] line, int index)
    {
        lineMoveComplete[index] = false;
        while (MakeOneMoveDownIndex(line))
        {
            moveMade = true;
            yield return new WaitForSeconds(delay);
        }
        lineMoveComplete[index] = true;
    }

    IEnumerator MoveCaroutine(MoveDirection md)
    {
        State = GameState.WaitingForMoveToEnd;
        switch (md)
        {
            case MoveDirection.Left:
                for (int i = 0; i < rows.Count; i++)
                {
                    StartCoroutine(MoveOneLineDownIndexCoroutine(rows[i], i));
                }
                break;
            case MoveDirection.Right:
                for (int i = 0; i < rows.Count; i++)
                {
                    StartCoroutine(MoveOneLineUpIndexCoroutine(rows[i], i));
                }
                break;
            case MoveDirection.Up:
                for (int i = 0; i < columns.Count; i++)
                {
                    StartCoroutine(MoveOneLineDownIndexCoroutine(columns[i], i));
                }
                break;
            case MoveDirection.Down:
                for (int i = 0; i < columns.Count; i++)
                {
                    StartCoroutine(MoveOneLineUpIndexCoroutine(columns[i], i));
                }
                break;
            default:
                break;
        }

        while (!(lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3]))
        {
            yield return null;
        }

        if (moveMade)
        {
            UpdateEmptyTiles();
            GetInputLayer();
            Debug.Log("Highest Tile: " + HighestTile.Number);
            Debug.Log("SequenceTile: " + SequenceTile);
            Debug.Log("IsHighestTileCorner: "+IsHighestTileCorner);
            Debug.Log("SequenceMerge: "+SequenceMerge);
            Debug.Log("CountSmallTile: "+CountSmallTile);
            Debug.Log("IsHighestTileDense: "+IsHighestTileDense);
            Generate();
            if (!CanMove())
            {
                GameOver("You Lose");
            }
        }
        State = GameState.Playing;
        StopAllCoroutines();
    }
}
