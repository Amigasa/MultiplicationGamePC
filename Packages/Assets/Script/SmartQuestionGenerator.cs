using System.Collections.Generic;
using UnityEngine;

public class SmartQuestionGenerator
{
    public QuestionData GenerateQuestion(DifficultySettings difficulty, int[] weakNumbers = null)
    {
        int number1, number2;

        // 40% шанс использовать слабые числа на сложных уровнях
        float weakNumberChance = difficulty.useTrickyAnswers ? 0.4f : 0.2f;

        if (weakNumbers != null && weakNumbers.Length > 0 && Random.Range(0f, 1f) > (1f - weakNumberChance))
        {
            number1 = weakNumbers[Random.Range(0, weakNumbers.Length)];
            number2 = Random.Range(difficulty.minNumber, difficulty.maxNumber + 1);
        }
        else
        {
            number1 = Random.Range(difficulty.minNumber, difficulty.maxNumber + 1);
            number2 = Random.Range(difficulty.minNumber, difficulty.maxNumber + 1);
        }

        int correctAnswer = number1 * number2;
        int[] wrongAnswers = GenerateWrongAnswers(correctAnswer, difficulty, number1, number2);

        return new QuestionData(number1, number2, correctAnswer, wrongAnswers);
    }

    int[] GenerateWrongAnswers(int correctAnswer, DifficultySettings difficulty, int number1, int number2)
    {
        int[] wrongAnswers = new int[3];
        HashSet<int> usedAnswers = new HashSet<int> { correctAnswer };

        for (int i = 0; i < 3; i++)
        {
            int wrongAnswer;
            int attempts = 0;

            do
            {
                if (difficulty.useTrickyAnswers)
                {
                    wrongAnswer = GetTrickyWrongAnswer(correctAnswer, number1, number2, difficulty);
                }
                else
                {
                    wrongAnswer = GetSimpleWrongAnswer(correctAnswer, difficulty);
                }
                attempts++;
            } while (usedAnswers.Contains(wrongAnswer) || wrongAnswer <= 0 || wrongAnswer > 100 || attempts < 10);

            wrongAnswers[i] = wrongAnswer;
            usedAnswers.Add(wrongAnswer);
        }

        return wrongAnswers;
    }

    int GetTrickyWrongAnswer(int correctAnswer, int number1, int number2, DifficultySettings difficulty)
    {
        int choice = Random.Range(0, 4); // 4 типа ошибок

        switch (choice)
        {
            case 0: // Перестановка цифр (12 -> 21, 18 -> 81)
                if (correctAnswer > 9 && correctAnswer < 100)
                {
                    int tens = correctAnswer / 10;
                    int units = correctAnswer % 10;
                    int swapped = units * 10 + tens;
                    if (swapped != correctAnswer && swapped >= 10 && swapped <= 99)
                        return swapped;
                }
                break;

            case 1: // Сложение вместо умножения (3×4=12, но 3+4=7)
                int sum = number1 + number2;
                if (sum != correctAnswer && sum >= 4 && sum <= 20)
                    return sum;
                break;

            case 2: // Ошибка в таблице (близкое число)
                int offset = Random.Range(1, 3) * (Random.Range(0, 2) == 0 ? 1 : -1);
                int closeNumber = correctAnswer + offset;
                if (closeNumber > 0 && closeNumber <= 100)
                    return closeNumber;
                break;

            case 3: // Умножение на соседнее число (6×7=42, но 6×8=48)
                int neighbor = number2 + (Random.Range(0, 2) == 0 ? 1 : -1);
                if (neighbor >= difficulty.minNumber && neighbor <= difficulty.maxNumber)
                {
                    int neighborAnswer = number1 * neighbor;
                    if (neighborAnswer != correctAnswer && neighborAnswer > 0 && neighborAnswer <= 100)
                        return neighborAnswer;
                }
                break;
        }

        // Fallback - простая ошибка
        return GetSimpleWrongAnswer(correctAnswer, difficulty);
    }

    int GetSimpleWrongAnswer(int correctAnswer, DifficultySettings difficulty)
    {
        int wrongAnswer;
        int attempts = 0;

        do
        {
            wrongAnswer = correctAnswer + Random.Range(-difficulty.wrongAnswerRange, difficulty.wrongAnswerRange + 1);
            attempts++;
        } while ((wrongAnswer == correctAnswer || wrongAnswer <= 0 || wrongAnswer > 100) && attempts < 10);

        return wrongAnswer;
    }
}
