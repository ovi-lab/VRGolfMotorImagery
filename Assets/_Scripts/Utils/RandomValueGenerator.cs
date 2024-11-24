using System;
using System.Collections.Generic;

public static class RandomValueGenerator
{
    public static List<float> GenerateValues(int count, float mean, float min, float max, Random random)
    {
        if (count <= 0) throw new ArgumentException("Number of values must be greater than 0.");
        if (min > mean || mean > max) throw new ArgumentException("The target average must be between the lower and upper bounds.");

        List<float> numbers = new List<float>();

        if (count % 2 != 0)
        {
            numbers.Add(mean);
            count--;
        }

        for (int i = 0; i < count/2; i++)
        {
            float n = random.NextFloat(min, max);
            numbers.Add(n);
            float diff = n - mean;
            numbers.Add(mean - diff);
        }

        numbers.Shuffle(random);
        return numbers;
    }

}