﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    public GameState State;
    [Range(0, 0.5f)]
    public float delay;
    public int mapSize;
    private bool moveMade;
    // bolean to check if move is done because it has delay so it doesnt move around
    private bool[] lineMoveComplete;
    //    highest tile tau" berubah jadi kecil ?
    //simulation ini rasanya ada yang salah update empty tile mungkin ?

    public Text TextDescriptionAlgorithm, TextDescriptionArchitecture;
    public Text TextIterationPopulation, TextIterationGeneration;
    public Text GameOverText;
    public Text GameTimeText;
    public GameObject GameOverPanel;
    private Tile[,] AllTiles;
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

    private float GameTime = 0;
    private Genetic genetic;
    private WOA woa;
    private MFO mfo;
    private int populationSize = 10;
    private int iterPopulation = 0;
    private bool IsGameOver = false;
    private int numLayer = 1;
    private int numNeuron = 10;

    private bool IsLoad = false;
    private Individual LoadedIndividual;
    private int ctr = 0;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(218116692);
        lineMoveComplete = new bool[mapSize];
        for (int i = 0; i < mapSize; i++)
        {
            lineMoveComplete[i] = true;
        }
        AllTiles = new Tile[mapSize, mapSize];

        if (AIController.path == null)
        {
            InitAlgo(AIController.algorithm, AIController.architecture);
            TextDescriptionAlgorithm.text = "Algorithm  - <b>" + AIController.algorithm + "</b>";
            TextDescriptionArchitecture.text = "Architecture - <b>" + AIController.architecture + "</b>";
        }
        else
        {
            IsLoad = true;
            LoadedIndividual = AIController.LoadInd();
        }

        RestartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsGameOver)
        {
            if (State == GameState.Playing)
            {
                Debug.Log($"sebelum Highest Tile: {HighestTile.Number} {HighestTile.indCol} {HighestTile.indRow}           {ctr++}");
                MoveAgent(AIController.algorithm);
                Debug.Log($"sesudah Highest Tile: {HighestTile.Number} {HighestTile.indCol} {HighestTile.indRow}           {ctr++}");
            }
            GameTime += Time.deltaTime;
            System.TimeSpan time = System.TimeSpan.FromSeconds(GameTime);
            GameTimeText.text = time.ToString(@"mm\:ss\:fff");
        }
    }

    private MoveDirection TreeSimulation(List<float> Weights)
    {
        int[,] TempAllTiles = new int[mapSize, mapSize];
        foreach (Tile t in AllTiles)
        {
            TempAllTiles[t.indRow, t.indCol] = t.Number;
        }

        float Highscore = 0;
        MoveDirection ret = MoveDirection.Left;
        MoveDirection[] arrMD = new MoveDirection[4] { MoveDirection.Left, MoveDirection.Up, MoveDirection.Right, MoveDirection.Down };
        foreach (MoveDirection md in arrMD)
        {
            float score = 0;
            moveMade = false;
            ResetMergedFlags();
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
                // hitung score
                score = (HighestTile.Number * Weights[0]) +
                    (SequenceTile * Weights[1]) +
                    ((IsHighestTileCorner ? 1 : 0) * Weights[2]) +
                    (SequenceMerge * Weights[3]) +
                    (CountSmallTile * Weights[4]) +
                    ((IsHighestTileDense ? 1 : 0) * Weights[5]);
            }
            // reset ulang map untuk kembali di simulasi
            foreach (Tile t in AllTiles)
            {
                AllTiles[t.indRow, t.indCol].Number = TempAllTiles[t.indRow, t.indCol];
            }
            if (score > Highscore)
            {
                Highscore = score;
                ret = md;
            }
        }
        return ret;
    }
    private MoveDirection NNSimulation(Individual Ind)
    {
        int[,] TempAllTiles = new int[mapSize, mapSize];
        foreach (Tile t in AllTiles)
        {
            TempAllTiles[t.indRow, t.indCol] = t.Number;
        }
        MoveDirection ret = MoveDirection.Left;
        MoveDirection[] arrMD = Ind.nn.Move(HighestTile.Number, SequenceTile, IsHighestTileCorner ? 1 : 0, SequenceMerge, CountSmallTile, IsHighestTileDense ? 1 : 0);
        foreach (MoveDirection md in arrMD)
        {
            moveMade = false;
            ResetMergedFlags();
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
            // reset ulang map
            foreach (Tile t in AllTiles)
            {
                AllTiles[t.indRow, t.indCol].Number = TempAllTiles[t.indRow, t.indCol];
            }
            if (moveMade)
            {
                ret = md;
                break;
            }
        }
        return ret;
    }

    private void InitAlgo(AlgorithmOption algorithmOption, ArchitectureOption architectureOption)
    {
        if (algorithmOption == AlgorithmOption.Genetic)
        {
            genetic = new Genetic(populationSize, architectureOption, mapSize, numLayer, numNeuron);
        }
        else if (algorithmOption == AlgorithmOption.MFO)
        {
            mfo = new MFO(populationSize, architectureOption, mapSize, numLayer, numNeuron);
        }
        else if (algorithmOption == AlgorithmOption.WOA)
        {
            woa = new WOA(populationSize, architectureOption, mapSize, numLayer, numNeuron);
        }
    }
    private void MoveAgent(AlgorithmOption algorithmOption)
    {
        MoveDirection ret = MoveDirection.Left;
        if (IsLoad)
        {
            if (AIController.architecture == ArchitectureOption.Tree)
            {
                ret = TreeSimulation(LoadedIndividual.Weights);
            }
            else
            {
                ret = NNSimulation(LoadedIndividual);
            }
        }
        else
        {
            if (algorithmOption == AlgorithmOption.Genetic)
            {
                if (genetic.architecture == ArchitectureOption.Tree)
                {
                    ret = TreeSimulation(genetic.Population[iterPopulation].Weights);
                }
                else
                {
                    ret = NNSimulation(genetic.Population[iterPopulation]);
                }
            }
            else if (algorithmOption == AlgorithmOption.MFO)
            {
                if (mfo.architecture == ArchitectureOption.Tree)
                {
                    ret = TreeSimulation(mfo.Population[iterPopulation].Weights);
                }
                else
                {
                    ret = NNSimulation(mfo.Population[iterPopulation]);
                }
            }
            else if (algorithmOption == AlgorithmOption.WOA)
            {
                if (woa.architecture == ArchitectureOption.Tree)
                {
                    ret = TreeSimulation(woa.Population[iterPopulation].Weights);
                }
                else
                {
                    ret = NNSimulation(woa.Population[iterPopulation]);
                }
            }
        }
        Move(ret);
    }
    private void EvaluateGame(AlgorithmOption algorithmOption)
    {
        if (algorithmOption == AlgorithmOption.Genetic)
        {
            // setting achivement individual untuk nanti bantu hitung fitness
            genetic.Population[iterPopulation].Score = ScoreTracker.Instance.Score;
            genetic.Population[iterPopulation].HighestTile = HighestTile.Number;
            genetic.Population[iterPopulation].GameTime = GameTime;
            // kalau iter populasi masih ada lanjut else repopulasi
            if (iterPopulation < populationSize - 1)
            {
                TextIterationPopulation.text = "" + ++iterPopulation;
            }
            else
            {
                iterPopulation = 0;
                genetic.RePopulate();
                TextIterationGeneration.text = "" + genetic.generation;
            }
            RestartGame();
        }
        else if (algorithmOption == AlgorithmOption.MFO)
        {
            // setting achivement individual untuk nanti bantu hitung fitness
            mfo.Population[iterPopulation].Score = ScoreTracker.Instance.Score;
            mfo.Population[iterPopulation].HighestTile = HighestTile.Number;
            mfo.Population[iterPopulation].GameTime = GameTime;
            // kalau iter populasi masih ada lanjut else repopulasi
            if (iterPopulation < populationSize - 1)
            {
                TextIterationPopulation.text = "" + ++iterPopulation;
            }
            else
            {
                iterPopulation = 0;
                mfo.UpdateMothPosition();
                TextIterationGeneration.text = "" + mfo.generation;
            }
            RestartGame();
        }
        else if (algorithmOption == AlgorithmOption.WOA)
        {
            // setting achivement individual untuk nanti bantu hitung fitness
            woa.Population[iterPopulation].Score = ScoreTracker.Instance.Score;
            woa.Population[iterPopulation].HighestTile = HighestTile.Number;
            woa.Population[iterPopulation].GameTime = GameTime;
            // kalau iter populasi masih ada lanjut else repopulasi
            if (iterPopulation < populationSize - 1)
            {
                TextIterationPopulation.text = "" + ++iterPopulation;
            }
            else
            {
                iterPopulation = 0;
                woa.Optimize();
                TextIterationGeneration.text = "" + woa.generation;
            }
            RestartGame();
        }
    }
    private void EvaluateWinGame(AlgorithmOption algorithmOption)
    {
        if (algorithmOption == AlgorithmOption.Genetic)
        {
            genetic.Population[iterPopulation].Score = ScoreTracker.Instance.Score;
            genetic.Population[iterPopulation].HighestTile = HighestTile.Number;
            genetic.Population[iterPopulation].GameTime = GameTime;
            AIController.PrintPopulation(genetic.Population, genetic.generation, mapSize, iterPopulation);
        }
        else if (algorithmOption == AlgorithmOption.MFO)
        {
            mfo.Population[iterPopulation].Score = ScoreTracker.Instance.Score;
            mfo.Population[iterPopulation].HighestTile = HighestTile.Number;
            mfo.Population[iterPopulation].GameTime = GameTime;
            AIController.PrintPopulation(mfo.Population, mfo.generation, mapSize, iterPopulation);
        }
        else if (algorithmOption == AlgorithmOption.WOA)
        {
            woa.Population[iterPopulation].Score = ScoreTracker.Instance.Score;
            woa.Population[iterPopulation].HighestTile = HighestTile.Number;
            woa.Population[iterPopulation].GameTime = GameTime;
            AIController.PrintPopulation(woa.Population, woa.generation, mapSize, iterPopulation);
        }
    }

    private void UpdateEmptyTiles()
    {
        CountSmallTile = 0;
        EmptyTiles.Clear();
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
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

                if (i == mapSize - 1)
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

                if (j == mapSize - 1)
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
            (HighestTile.indRow == mapSize - 1 && HighestTile.indCol == mapSize - 1) ||
            (HighestTile.indRow == mapSize - 1 && HighestTile.indCol == 0) ||
            (HighestTile.indRow == 0 && HighestTile.indCol == mapSize - 1)
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

        if (mapSize == 4)
        {
            while (!(lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3]))
            {
                yield return null;
            }
        }
        else if (mapSize == 5)
        {
            while (!(lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3] && lineMoveComplete[4]))
            {
                yield return null;
            }
        }
        else if (mapSize == 6)
        {
            while (!(lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3] && lineMoveComplete[4] && lineMoveComplete[5]))
            {
                yield return null;
            }
        }

        if (moveMade)
        {
            UpdateEmptyTiles();
            GetInputLayer();
            Generate();
            if (!CanMove())
            {
                GameOver("You Lose");
            }
        }
        State = GameState.Playing;
        StopAllCoroutines();
    }

    private void RestartGame()
    {
        ScoreTracker.Instance.Score = 0;
        GameTime = 0;
        columns.Clear();
        rows.Clear();
        EmptyTiles.Clear();
        Tile[] AllTilesOneDim = GameObject.FindObjectsOfType<Tile>();
        foreach (Tile t in AllTilesOneDim)
        {
            t.Number = 0;
            AllTiles[t.indRow, t.indCol] = t;
            EmptyTiles.Add(t);
        }
        HighestTile = AllTiles[0, 0];
        InitColumnsRows();
        Generate(); Generate();
        TextIterationPopulation.text = "" + iterPopulation;
    }
    private void GameOver(string text, bool IsWin = false)
    {
        IsGameOver = true;
        if (IsWin)
        {
            GameOverText.text = text;
            GameOverPanel.SetActive(true);
            EvaluateWinGame(AIController.algorithm);
        }
        else
        {
            EvaluateGame(AIController.algorithm);
            IsGameOver = false;
        }
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
                ScoreTracker.Instance.Score += LineOfTiles[i].Number;
                if (LineOfTiles[i].Number == 2048)
                {
                    GameOver("You Win", true);
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
                ScoreTracker.Instance.Score += LineOfTiles[i].Number;
                if (LineOfTiles[i].Number == 2048)
                {
                    GameOver("You Win", true);
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

    private void ResetMergedFlags()
    {
        foreach (Tile t in AllTiles)
            t.mergeThisTurn = false;
    }
    private void Generate()
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
    private void InitColumnsRows()
    {
        if (mapSize == 4)
        {
            columns.Add(new Tile[] { AllTiles[0, 0], AllTiles[1, 0], AllTiles[2, 0], AllTiles[3, 0] });
            columns.Add(new Tile[] { AllTiles[0, 1], AllTiles[1, 1], AllTiles[2, 1], AllTiles[3, 1] });
            columns.Add(new Tile[] { AllTiles[0, 2], AllTiles[1, 2], AllTiles[2, 2], AllTiles[3, 2] });
            columns.Add(new Tile[] { AllTiles[0, 3], AllTiles[1, 3], AllTiles[2, 3], AllTiles[3, 3] });
            rows.Add(new Tile[] { AllTiles[0, 0], AllTiles[0, 1], AllTiles[0, 2], AllTiles[0, 3] });
            rows.Add(new Tile[] { AllTiles[1, 0], AllTiles[1, 1], AllTiles[1, 2], AllTiles[1, 3] });
            rows.Add(new Tile[] { AllTiles[2, 0], AllTiles[2, 1], AllTiles[2, 2], AllTiles[2, 3] });
            rows.Add(new Tile[] { AllTiles[3, 0], AllTiles[3, 1], AllTiles[3, 2], AllTiles[3, 3] });
        }
        else if (mapSize == 5)
        {
            columns.Add(new Tile[] { AllTiles[0, 0], AllTiles[1, 0], AllTiles[2, 0], AllTiles[3, 0], AllTiles[4, 0] });
            columns.Add(new Tile[] { AllTiles[0, 1], AllTiles[1, 1], AllTiles[2, 1], AllTiles[3, 1], AllTiles[4, 1] });
            columns.Add(new Tile[] { AllTiles[0, 2], AllTiles[1, 2], AllTiles[2, 2], AllTiles[3, 2], AllTiles[4, 2] });
            columns.Add(new Tile[] { AllTiles[0, 3], AllTiles[1, 3], AllTiles[2, 3], AllTiles[3, 3], AllTiles[4, 3] });
            columns.Add(new Tile[] { AllTiles[0, 4], AllTiles[1, 4], AllTiles[2, 4], AllTiles[3, 4], AllTiles[4, 4] });
            rows.Add(new Tile[] { AllTiles[0, 0], AllTiles[0, 1], AllTiles[0, 2], AllTiles[0, 3], AllTiles[0, 4] });
            rows.Add(new Tile[] { AllTiles[1, 0], AllTiles[1, 1], AllTiles[1, 2], AllTiles[1, 3], AllTiles[1, 4] });
            rows.Add(new Tile[] { AllTiles[2, 0], AllTiles[2, 1], AllTiles[2, 2], AllTiles[2, 3], AllTiles[2, 4] });
            rows.Add(new Tile[] { AllTiles[3, 0], AllTiles[3, 1], AllTiles[3, 2], AllTiles[3, 3], AllTiles[3, 4] });
            rows.Add(new Tile[] { AllTiles[4, 0], AllTiles[4, 1], AllTiles[4, 2], AllTiles[4, 3], AllTiles[4, 4] });
        }
        else if (mapSize == 6)
        {
            columns.Add(new Tile[] { AllTiles[0, 0], AllTiles[1, 0], AllTiles[2, 0], AllTiles[3, 0], AllTiles[4, 0], AllTiles[5, 0] });
            columns.Add(new Tile[] { AllTiles[0, 1], AllTiles[1, 1], AllTiles[2, 1], AllTiles[3, 1], AllTiles[4, 1], AllTiles[5, 1] });
            columns.Add(new Tile[] { AllTiles[0, 2], AllTiles[1, 2], AllTiles[2, 2], AllTiles[3, 2], AllTiles[4, 2], AllTiles[5, 2] });
            columns.Add(new Tile[] { AllTiles[0, 3], AllTiles[1, 3], AllTiles[2, 3], AllTiles[3, 3], AllTiles[4, 3], AllTiles[5, 3] });
            columns.Add(new Tile[] { AllTiles[0, 4], AllTiles[1, 4], AllTiles[2, 4], AllTiles[3, 4], AllTiles[4, 4], AllTiles[5, 4] });
            columns.Add(new Tile[] { AllTiles[0, 5], AllTiles[1, 5], AllTiles[2, 5], AllTiles[3, 5], AllTiles[4, 5], AllTiles[5, 5] });
            rows.Add(new Tile[] { AllTiles[0, 0], AllTiles[0, 1], AllTiles[0, 2], AllTiles[0, 3], AllTiles[0, 4], AllTiles[0, 5] });
            rows.Add(new Tile[] { AllTiles[1, 0], AllTiles[1, 1], AllTiles[1, 2], AllTiles[1, 3], AllTiles[1, 4], AllTiles[1, 5] });
            rows.Add(new Tile[] { AllTiles[2, 0], AllTiles[2, 1], AllTiles[2, 2], AllTiles[2, 3], AllTiles[2, 4], AllTiles[2, 5] });
            rows.Add(new Tile[] { AllTiles[3, 0], AllTiles[3, 1], AllTiles[3, 2], AllTiles[3, 3], AllTiles[3, 4], AllTiles[3, 5] });
            rows.Add(new Tile[] { AllTiles[4, 0], AllTiles[4, 1], AllTiles[4, 2], AllTiles[4, 3], AllTiles[4, 4], AllTiles[4, 5] });
            rows.Add(new Tile[] { AllTiles[5, 0], AllTiles[5, 1], AllTiles[5, 2], AllTiles[5, 3], AllTiles[5, 4], AllTiles[5, 5] });
        }
    }
}