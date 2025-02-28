﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MFO
{
    public Individual bestInd;
    public List<Individual> Population = new List<Individual>();
    public List<Individual> PreviousPopulation = new List<Individual>();
    private List<Individual> BestFlames = new List<Individual>();
    public int populationSize, generation, mapSize;
    private bool isTree; // aku simpen isTree supaya pas initialize if nya engga berat jadi langsung akses bool
    private int IndSize; // ukuran individu karen kalo tree dan NN ukuran nya beda
    private int numLayer, numNeuron;
    public ArchitectureOption architecture;

    private float ngens; // number of max generations
    private float a;
    private int FlameNo;

    public MFO(int populationSize, ArchitectureOption architecture, int mapSize, float max_gen, int layer, int neuron)
    {
        generation = 0;
        numLayer = layer; numNeuron = neuron;
        ngens = max_gen;
        this.mapSize = mapSize;
        this.populationSize = populationSize;
        this.architecture = architecture;
        for (int i = 0; i < populationSize; i++)
        {
            List<float> tempW = new List<float>();
            // ctrSize ini ukuran individu kalau tree pasti 6 kalau NN harus di itung dulu
            if (architecture == ArchitectureOption.Tree)
            {
                isTree = true; IndSize = 6; // ini 6 input layer
                //isTree = true; IndSize = mapSize * mapSize; // ini coba input layer map size
            }
            else
            {
                isTree = false;
                // (layer + 1)->bias + (6 * neuron)->input layer + ((layer-1)*neuron*neuron)->hidden layer + (4 * neuron)->output layer
                IndSize = (layer + 1) + (6 * neuron) + ((layer - 1) * neuron * neuron) + (4 * neuron); // ini 6 input layer
                //IndSize = (layer + 1) + (mapSize * mapSize * neuron) + ((layer - 1) * neuron * neuron) + (4 * neuron); // ini coba input layer map size
            }
            for (int j = 0; j < IndSize; j++)
            {
                float rndVal;
                // kalau tree random nya cuma 0-1 kalau NN maka random range dari -1 sampe 1
                if (isTree)
                    rndVal = Random.value;
                else
                    rndVal = Random.Range(-1f, 1f);
                tempW.Add(rndVal);
            }
            Population.Add(new Individual(tempW, architecture, AlgorithmOption.MFO, layer, neuron));
        }
        bestInd = Population[0].InitialiseCopy(layer, neuron);
    }

    public void UpdateMothPosition()
    {
        CalculateFitness();
        GetBestFlame();

        // mendapatkan previous population yaitu populasi sebelum posisi dirubah
        PreviousPopulation.Clear();
        foreach (Individual Ind in Population)
        {
            PreviousPopulation.Add(Ind.InitialiseCopy(numLayer, numNeuron));
        }

        // a nanti akan digunakan untuk menghitung t yang ada di Eq. (3.12)
        a = -1.0f + (float)(generation + 1) * (-1.0f / ngens);
        // FlameNo digunakan dalam metode update posisi sesuai Eq. (3.14)
        FlameNo = Mathf.RoundToInt((populationSize - 1) - (generation + 1) * ((float)(populationSize - 1) / (float)ngens));
        updatePosition();
    }
    private void updatePosition()
    {
        float distanceToFlame, t;
        float b = 1.0f;
        List<Individual> TempPopulation = new List<Individual>();
        for (int agentIndex = 0; agentIndex < populationSize; agentIndex++)
        {
            List<float> bestFlamesVariables;
            if (agentIndex <= FlameNo)
            {
                // jika index < FlameNo maka pakai agentIndex sebagai pacuan flame untuk mengubah posisi
                bestFlamesVariables = new List<float>(BestFlames[agentIndex].Weights);
            }
            else
            {
                // else  maka pakai index FlameNo sebagai pacuan flame untuk mengubah posisi
                bestFlamesVariables = new List<float>(BestFlames[FlameNo].Weights);
            }

            List<float> newIndWeight = new List<float>();
            for (int j = 0; j < IndSize; j++)
            {
                // D in Eq. (3.13)
                distanceToFlame = Mathf.Abs(bestFlamesVariables[j] - Population[agentIndex].Weights[j]);
                // t yang ada di Eq. (3.12)
                t = (a - 1.0f) * Random.value + 1.0f;
                newIndWeight.Add(distanceToFlame * Mathf.Exp(b * t) * Mathf.Cos(t * 2.0f * Mathf.PI) + bestFlamesVariables[j]);
            }
            TempPopulation.Add(new Individual(newIndWeight, architecture, AlgorithmOption.MFO, numLayer, numNeuron));
        }
        Population.Clear();
        foreach (Individual p in TempPopulation)
        {
            Population.Add(p.InitialiseCopy(numLayer, numNeuron));
        }
        generation++;
    }
    private void CalculateFitness()
    {
        int HighScore = PlayerPrefs.GetInt($"HighScore{mapSize}");
        // calculate every individual's fitness
        for (int i = 0; i < Population.Count; i++)
        {
            // calculate fitness highest tile saja karena saat endgame biasanya sudah berantakan jadi second highest tile dll pindah"
            float temp = (((float)Population[i].HighestTile / (float)2048) + ((float)Population[i].Score / (float)HighScore)) / 2;
            Population[i].Fitness = temp;
            if (Population[i].Score > bestInd.Score)
            {
                bestInd = Population[i].InitialiseCopy(numLayer, numNeuron);
                bestInd.Score = Population[i].Score;
                bestInd.Fitness = Population[i].Fitness;
                bestInd.HighestTile = Population[i].HighestTile;
                bestInd.GameTime = Population[i].GameTime;
            }
        }
        AIController.PrintPopulation(Population, generation, mapSize);
        Population.Sort(AIController.SortFunc);
    }
    private void GetBestFlame()
    {
        if (generation == 0)
        {
            foreach (Individual Ind in Population)
            {
                BestFlames.Add(Ind.InitialiseCopy(numLayer, numNeuron));
            }
            // jika generasi 1 maka best flame ambil dari populasi saja
            BestFlames.Sort(AIController.SortFunc);
        }
        else
        {
            // else best flame ambil dari gabungan populasi lama dan best flame sebelumnya
            List<Individual> TempJoinPopulation = new List<Individual>();
            foreach (Individual Ind in BestFlames)
            {
                TempJoinPopulation.Add(Ind.InitialiseCopy(numLayer, numNeuron));
            }
            foreach (Individual Ind in PreviousPopulation)
            {
                TempJoinPopulation.Add(Ind.InitialiseCopy(numLayer, numNeuron));
            }
            TempJoinPopulation.Sort(AIController.SortFunc);

            BestFlames.Clear();
            for (int i = 0; i < populationSize; i++)
            {
                BestFlames.Add(TempJoinPopulation[i].InitialiseCopy(numLayer, numNeuron));
            }
        }
    }
}
