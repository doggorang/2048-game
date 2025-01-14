﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum AlgorithmOption
{
    Genetic, WOA, MFO
}
public enum ArchitectureOption
{
    NN, Tree
}

public class AIController : MonoBehaviour
{
    public static AlgorithmOption algorithm = AlgorithmOption.Genetic;
    public static ArchitectureOption architecture = ArchitectureOption.Tree;
    public static int ngens = 100;
    public static int populationSize = 50;
    public static int layer = 1;
    public static int neuron = 10;
    public static string path;

    public static int SortFunc(Individual a, Individual b)
    {
        if (a.Fitness < b.Fitness)
        {
            return 1;
        }
        else if (a.Fitness > b.Fitness)
        {
            return -1;
        }
       return 0;
    }

    public static void PrintPopulation(List<Individual> Population, int generation, int mapSize, int index = -1)
    {
        int ctr, maxCtr;
        if (index >= 0)
        {
            ctr = index; maxCtr = index + 1;
            int HighScore = PlayerPrefs.GetInt($"HighScore{mapSize}");
            Population[index].Fitness = (((float)Population[index].HighestTile / (float)2048) + ((float)Population[index].Score / (float)HighScore)) / 2;
            string WinInd = JsonUtility.ToJson(Population[index], true);

            string winnerPath = $"{Application.dataPath}/Winner/";
            if (!Directory.Exists(winnerPath))
            {
                Directory.CreateDirectory(winnerPath);
            }
            string pathInd = $"{winnerPath}{algorithm} {architecture} {mapSize}x{mapSize}.json";
            File.WriteAllText(pathInd, WinInd);
        }
        else
        {
            ctr = 0; maxCtr = Population.Count;
        }
        string content = "";
        for (int i = ctr; i < maxCtr; i++)
        {
            content += $"Generation: {generation} Population: {i} Fitness: {Population[i].Fitness} Score: {Population[i].Score} Tile: {Population[i].HighestTile} Time: {Population[i].GameTime}\nWeight: [ ";
            foreach (float w in Population[i].Weights)
            {
                content += w + ", ";
            }
            content += "]\n";
        }
        content += "\n";
        string directoryPath = $"{Application.dataPath}/Log/{algorithm}/{architecture}/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string path = $"{directoryPath}{mapSize}x{mapSize}.txt";
        if (!File.Exists(path))
            File.WriteAllText(path, content);
        else
            File.AppendAllText(path, content);
    }

    public static Individual LoadInd()
    {
        string json = File.ReadAllText(path);
        Individual ret = JsonUtility.FromJson<Individual>(json);
        return ret;
    }
    public static void PrintBestInd(Individual bestInd, int generation, int mapSize)
    {
        string content = "";
        content += $"Generation: {generation} Fitness: {bestInd.Fitness} Score: {bestInd.Score} Tile: {bestInd.HighestTile} Time: {bestInd.GameTime}\nWeight: [ ";
        foreach (float w in bestInd.Weights)
        {
            content += w + ", ";
        }
        content += "]\n";
        content += "\n";
        string directoryPath = $"{Application.dataPath}/Log/{algorithm}/{architecture}/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string path = $"{directoryPath}{mapSize}x{mapSize}.txt";
        if (!File.Exists(path))
            File.WriteAllText(path, content);
        else
            File.AppendAllText(path, content);
    }
}
