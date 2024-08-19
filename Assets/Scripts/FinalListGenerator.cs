using System;
using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;

public class FinalListGenerator : MonoBehaviour
{
    private readonly Random random = new Random(); 
    private List<int> errorList= new List<int>();
    private List<int> errorListShuffled= new List<int>();
    private List<int> mediumList= new List<int>();
    public List<Vector2> finalList= new List<Vector2>();
    public Vector2 holeTransformation;    
    
    [SerializeField]
    private float gridSize = 1;
    

    void Awake()
    {
        holeTransformation = new Vector2(transform.position.x, transform.position.z);
        int feedbackGroup=FindObjectOfType<GolfBallController>().feedbackGroup;
        GetFinalListBasedOnFeedbackGroup(feedbackGroup);
    }

    public void GetFinalListBasedOnFeedbackGroup(int feedbackGroup)
    {
        if (feedbackGroup==1)
        {
            finalList = GeneratePerfectFeedbackList();
        }

        if (feedbackGroup==2)
        {
            RandomCounts.GenerateCounts();
            List<int> countList = new List<int> { RandomCounts.countA, RandomCounts.countB, RandomCounts.countC, 
                RandomCounts.countD, RandomCounts.countE, RandomCounts.countF };
            errorList = new List<int> { 5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1 };
            errorListShuffled = RandomCounts.ShuffleIntList(errorList);
            mediumList = RandomCounts.GenerateMediumList(errorListShuffled);
            finalList = GenerateRandomFeedbackList(mediumList,countList);
        }
        if (feedbackGroup==3)
        {
            errorList = new List<int> { 5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1 };
            finalList = GenerateAdaptiveFeedbackList(errorList);
        }
    }

    public List<Vector2> GeneratePerfectFeedbackList()
    {
        int numOfTrails = 90;
        List<Vector2> perfectFeedbackList = new List<Vector2>();
        for (int i = 0; i < numOfTrails; i++)
        {
            perfectFeedbackList.Add(holeTransformation);
        }
        return perfectFeedbackList;
    }

    public List<Vector2> GenerateRandomFeedbackList(List<int> mediumList,List<int> countList)
    {
        
        List<Vector2> resultList = new List<Vector2>();

        foreach (int value in mediumList)
        {
            if (value == 0)
            {
                resultList.Add(new Vector2(holeTransformation.x, holeTransformation.y));
            }
            else if (value == 1)
            {
                int index = SelectRandomNonZeroCount(countList);
                
                switch (index)
                {
                    case 0:
                        countList[0]--;
                        resultList.Add(GetA());
                        break;
                    case 1:
                        countList[1]--;
                        resultList.Add(GetB());
                        break;
                    case 2:
                        countList[2]--;
                        resultList.Add(GetC());
                        break;
                    case 3:
                        countList[3]--;
                        resultList.Add(GetD());
                        break;
                    case 4:
                        countList[4]--;
                        resultList.Add(GetE());
                        break;
                    case 5:
                        countList[5]--;
                        resultList.Add(GetF());
                        break;
                }
            }
        }
        if (resultList.Count!=90)
        {
            throw new Exception("There should be 90 elements");
        }
        return resultList;
    }

    private int SelectRandomNonZeroCount(List<int> countList)
    {
        int randomIndex = random.Next(0, countList.Count);
        if (countList[randomIndex] > 0)
        {
            return randomIndex;
        }
        return SelectRandomNonZeroCount(countList);
    }
    public List<Vector2> GenerateAdaptiveFeedbackList(List<int> errorList)
    {
        List<Vector2> finalAdaptiveList = new List<Vector2>();
        int elementEachBlock = 6;
        float initialMRE = CalculateMRE(new List<Vector2> { GetE(), GetF(), GetE(), GetE(), GetE(), holeTransformation });
        float MRELastTime = initialMRE;

        if (errorList.Count != 15)
        {
            throw new Exception("The errorList should have exactly 15 elements.");
        }

        for (int i = 0; i < errorList.Count; i++)
        {
            bool findLowerMRE = false;
            List<Vector2> block = new List<Vector2>();
            int numberOfErrorsThisBlock = errorList[i];
            for (int j = 0; j < elementEachBlock - numberOfErrorsThisBlock; j++)
            {
                block.Add(holeTransformation);
            }

            while (!findLowerMRE)
            {
                List<Vector2> errorListInOneBlock = GenerateErrorsInOneBlock(numberOfErrorsThisBlock);
                block.AddRange(errorListInOneBlock);
                float MRE_ThisTempBlockList = CalculateMRE(block);
                if (MRE_ThisTempBlockList < MRELastTime)
                {
                    block = RandomCounts.ShuffleVector2List(block);
                    finalAdaptiveList.AddRange(block);
                    MRELastTime = MRE_ThisTempBlockList;
                    findLowerMRE = true;
                }
            }
        }
        return finalAdaptiveList;
    }
    
    private float CalculateMRE(List<Vector2> coordinateList)
    {
        float totalError = 0f;
        int count = coordinateList.Count;

        if (count == 0)
            return 0f;

        foreach (Vector2 coordinate in coordinateList)
        {
            Vector2 errorVector = coordinate - holeTransformation;

            totalError += errorVector.magnitude;
        }
        float MRE = totalError / count;
        return MRE;
    }
    

    private List<Vector2> GenerateErrorsInOneBlock(int numberOfErrorsThisBlock)
    {
        List<Vector2> errorListInOneBlock = new List<Vector2>(numberOfErrorsThisBlock);
        int numberOfErrors = 6;
        for (int i = 0; i < numberOfErrors; i++)
        {
            int randomNumber=random.Next(0, numberOfErrors);
            switch (randomNumber)
            {
                case 0:
                    errorListInOneBlock.Add(GetA());
                    break;
                case 1:
                    errorListInOneBlock.Add(GetB());
                    break;
                case 2:
                    errorListInOneBlock.Add(GetC());
                    break;
                case 3:
                    errorListInOneBlock.Add(GetD());
                    break;
                case 4:
                    errorListInOneBlock.Add(GetE());
                    break;
                case 5:
                    errorListInOneBlock.Add(GetF());
                    break;
            }
        }

        return errorListInOneBlock;
    }
        private Vector2 GetA()
    {
        var ranges = new List<(float xMin, float xMax, float zMin, float zMax)>
        {
            (holeTransformation.x + 0.5f * gridSize, holeTransformation.x + 1.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y + 0.5f * gridSize),
            (holeTransformation.x - 1.5f * gridSize, holeTransformation.x - 0.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y + 0.5f * gridSize),
            (holeTransformation.x - 0.5f * gridSize, holeTransformation.x + 0.5f * gridSize, holeTransformation.y + 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize),
            (holeTransformation.x - 0.5f * gridSize, holeTransformation.x + 0.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y - 1.5f * gridSize)
        };

        var selectedRange = ranges[random.Next(ranges.Count)];

        float x = (float)(random.NextDouble() * (selectedRange.xMax - selectedRange.xMin) + selectedRange.xMin);
        float z = (float)(random.NextDouble() * (selectedRange.zMax - selectedRange.zMin) + selectedRange.zMin);

        return new Vector2(x,  z);
    }

    private Vector2 GetB()
    {
        var ranges = new List<(float xMin, float xMax, float zMin, float zMax)>
        {
            (holeTransformation.x + 0.5f * gridSize, holeTransformation.x + 1.5f * gridSize, holeTransformation.y + 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize),
            (holeTransformation.x - 1.5f * gridSize, holeTransformation.x - 0.5f * gridSize, holeTransformation.y + 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize),
            (holeTransformation.x + 0.5f * gridSize, holeTransformation.x + 1.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y - 1.5f * gridSize),
            (holeTransformation.x - 1.5f * gridSize, holeTransformation.x - 0.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y - 1.5f * gridSize)
        };

        var selectedRange = ranges[random.Next(ranges.Count)];

        float x = (float)(random.NextDouble() * (selectedRange.xMax - selectedRange.xMin) + selectedRange.xMin);
        float z = (float)(random.NextDouble() * (selectedRange.zMax - selectedRange.zMin) + selectedRange.zMin);

        return new Vector2(x,  z);
    }

    private Vector2 GetC()
    {
        var ranges = new List<(float xMin, float xMax, float zMin, float zMax)>
        {
            (holeTransformation.x + 0.5f * gridSize, holeTransformation.x + 1.5f * gridSize, holeTransformation.y + 1.5f * gridSize, holeTransformation.y + 2.5f * gridSize),
            (holeTransformation.x - 1.5f * gridSize, holeTransformation.x - 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize, holeTransformation.y + 2.5f * gridSize),
            (holeTransformation.x + 0.5f * gridSize, holeTransformation.x + 1.5f * gridSize, holeTransformation.y - 2.5f * gridSize, holeTransformation.y - 1.5f * gridSize),
            (holeTransformation.x - 1.5f * gridSize, holeTransformation.x - 0.5f * gridSize, holeTransformation.y - 2.5f * gridSize, holeTransformation.y - 1.5f * gridSize),
            (holeTransformation.x + 1.5f * gridSize, holeTransformation.x + 2.5f * gridSize, holeTransformation.y + 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize),
            (holeTransformation.x - 2.5f * gridSize, holeTransformation.x - 1.5f * gridSize, holeTransformation.y + 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize),
            (holeTransformation.x + 1.5f * gridSize, holeTransformation.x + 2.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y - 1.5f * gridSize),
            (holeTransformation.x - 2.5f * gridSize, holeTransformation.x - 1.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y - 1.5f * gridSize)
        };

        var selectedRange = ranges[random.Next(ranges.Count)];

        float x = (float)(random.NextDouble() * (selectedRange.xMax - selectedRange.xMin) + selectedRange.xMin);
        float z = (float)(random.NextDouble() * (selectedRange.zMax - selectedRange.zMin) + selectedRange.zMin);

        return new Vector2(x,  z);
    }

    private Vector2 GetD()
    {
        var ranges = new List<(float xMin, float xMax, float zMin, float zMax)>
        {
            (holeTransformation.x + 1.5f * gridSize, holeTransformation.x + 2.5f * gridSize, holeTransformation.y + 1.5f * gridSize, holeTransformation.y + 2.5f * gridSize),
            (holeTransformation.x + 1.5f * gridSize, holeTransformation.x + 2.5f * gridSize, holeTransformation.y - 2.5f * gridSize, holeTransformation.y - 1.5f * gridSize),
            (holeTransformation.x - 2.5f * gridSize, holeTransformation.x - 1.5f * gridSize, holeTransformation.y + 1.5f * gridSize, holeTransformation.y + 2.5f * gridSize),
            (holeTransformation.x - 2.5f * gridSize, holeTransformation.x - 1.5f * gridSize, holeTransformation.y - 2.5f * gridSize, holeTransformation.y - 1.5f * gridSize)
        };

        var selectedRange = ranges[random.Next(ranges.Count)];

        float x = (float)(random.NextDouble() * (selectedRange.xMax - selectedRange.xMin) + selectedRange.xMin);
        float z = (float)(random.NextDouble() * (selectedRange.zMax - selectedRange.zMin) + selectedRange.zMin);

        return new Vector2(x,  z);
    }

    private Vector2 GetE()
    {
        var ranges = new List<(float xMin, float xMax, float zMin, float zMax)>
        {
            (holeTransformation.x + 1.5f * gridSize, holeTransformation.x + 2.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y + 0.5f * gridSize),
            (holeTransformation.x - 2.5f * gridSize, holeTransformation.x - 1.5f * gridSize, holeTransformation.y - 0.5f * gridSize, holeTransformation.y + 0.5f * gridSize),
            (holeTransformation.x - 0.5f * gridSize, holeTransformation.x + 0.5f * gridSize, holeTransformation.y - 2.5f * gridSize, holeTransformation.y - 1.5f * gridSize),
            (holeTransformation.x - 0.5f * gridSize, holeTransformation.x + 0.5f * gridSize, holeTransformation.y + 1.5f * gridSize, holeTransformation.y + 2.5f * gridSize)
        };

        var selectedRange = ranges[random.Next(ranges.Count)];

        float x = (float)(random.NextDouble() * (selectedRange.xMax - selectedRange.xMin) + selectedRange.xMin);
        float z = (float)(random.NextDouble() * (selectedRange.zMax - selectedRange.zMin) + selectedRange.zMin);

        return new Vector2(x,  z);
    }

    private Vector2 GetF()
    {
        var ranges = new List<(float xMin, float xMax, float zMin, float zMax)>
        {
            (holeTransformation.x - 2.5f * gridSize, holeTransformation.x + 2.5f * gridSize, holeTransformation.y + 2.5f * gridSize, holeTransformation.y + 4.5f * gridSize)
        };

        var selectedRange = ranges[random.Next(ranges.Count)];

        float x = (float)(random.NextDouble() * (selectedRange.xMax - selectedRange.xMin) + selectedRange.xMin);
        float z = (float)(random.NextDouble() * (selectedRange.zMax - selectedRange.zMin) + selectedRange.zMin);

        return new Vector2(x,  z);
    }
}