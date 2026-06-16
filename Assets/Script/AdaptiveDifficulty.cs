using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdaptiveDifficulty
{
    private Dictionary<int, int> playerWeaknesses = new Dictionary<int, int>();

    public void RecordMistake(int number1, int number2)
    {
        int key = Mathf.Min(number1, number2) * 10 + Mathf.Max(number1, number2);

        if (playerWeaknesses.ContainsKey(key))
            playerWeaknesses[key]++;
        else
            playerWeaknesses[key] = 1;
    }

    public int[] GetWeakNumbers(int count = 3)
    {
        if (playerWeaknesses.Count == 0) return new int[0];

        return playerWeaknesses
            .OrderByDescending(x => x.Value)
            .Take(count)
            .SelectMany(x => new int[] { x.Key / 10, x.Key % 10 })
            .Distinct()
            .ToArray();
    }
}