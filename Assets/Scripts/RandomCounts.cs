using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

abstract class RandomCounts
{
    public static int countA { get; private set; }
    public static int countB { get; private set; }
    public static int countC { get; private set; }
    public static int countD { get; private set; }
    public static int countE { get; private set; }
    public static int countF { get; private set; }
    
    private static Random rand;

    public static void GenerateCounts()
    {
        int[] counts = new int[6];
        int sum = 0;
        int totalError = 30;
        for (int i = 0; i < counts.Length; i++)
        {
            counts[i] = 1;
            sum++;
        }
        Random random = new Random();
        while (sum < totalError)
        {
            int randomIndex = random.Next(0, counts.Length);
            counts[randomIndex]++;
            sum++;
        }
        countA = counts[0];
        countB = counts[1];
        countC = counts[2];
        countD = counts[3];
        countE = counts[4];
        countF = counts[5];
    }

    public static List<int> ShuffleIntList(List<int> list)
    {
        
        Random random = new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }
    public static List<Vector2> ShuffleVector2List(List<Vector2> list)
    {
        Random random = new Random();
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1); 

            Vector2 temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        return list;
    }
    public static List<int> GenerateMediumList(List<int> shuffledErrorList)
    {
        if (shuffledErrorList.Count != 15)
        {
            throw new Exception("The shuffledErrorList should have exactly 15 elements.");
        }
        List<int> mediumList = new List<int>();

        foreach (int errorCount in shuffledErrorList)
        {
            List<int> block = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                if (i < errorCount)
                {
                    block.Add(1); 
                }
                else
                {
                    block.Add(0); 
                }
            }
            block = ShuffleIntList(block);
            mediumList.AddRange(block);
        }
        return mediumList;
    }
}
