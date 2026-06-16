using UnityEngine;

[System.Serializable]
public class DifficultySettings
{
    public int minNumber = 2;
    public int maxNumber = 10;
    public int wrongAnswerRange = 5;
    public int totalQuestions = 5;
    public bool useTrickyAnswers = false;
    public float timeBonusThreshold = 4f; // Секунд для бонуса
}

public class ProgressiveLevelManager : MonoBehaviour
{
    [Header("Настройки сложности для 8 уровней")]
    public DifficultySettings[] difficultyLevels = new DifficultySettings[]
    {
        // Уровень 1: Знакомство
        new DifficultySettings {
            minNumber = 1,
            maxNumber = 5,
            wrongAnswerRange = 2,
            totalQuestions = 5,
            useTrickyAnswers = false,
            timeBonusThreshold = 15f
        },
        
        // Уровень 2: Основы
        new DifficultySettings {
            minNumber = 2,
            maxNumber = 6,
            wrongAnswerRange = 3,
            totalQuestions = 6,
            useTrickyAnswers = false,
            timeBonusThreshold = 13f
        },
        
        // Уровень 3: Развитие
        new DifficultySettings {
            minNumber = 3,
            maxNumber = 7,
            wrongAnswerRange = 3,
            totalQuestions = 6,
            useTrickyAnswers = true,
            timeBonusThreshold = 11.5f
        },
        
        // Уровень 4: Уверенность
        new DifficultySettings {
            minNumber = 4,
            maxNumber = 8,
            wrongAnswerRange = 4,
            totalQuestions = 7,
            useTrickyAnswers = true,
            timeBonusThreshold = 10f
        },
        
        // Уровень 5: Сложность
        new DifficultySettings {
            minNumber = 5,
            maxNumber = 9,
            wrongAnswerRange = 4,
            totalQuestions = 7,
            useTrickyAnswers = true,
            timeBonusThreshold = 8.5f
        },
        
        // Уровень 6: Мастерство
        new DifficultySettings {
            minNumber = 6,
            maxNumber = 9,
            wrongAnswerRange = 5,
            totalQuestions = 8,
            useTrickyAnswers = true,
            timeBonusThreshold = 7.5f
        },
        
        // Уровень 7: Эксперт
        new DifficultySettings {
            minNumber = 7,
            maxNumber = 9,
            wrongAnswerRange = 5,
            totalQuestions = 8,
            useTrickyAnswers = true,
            timeBonusThreshold = 6.5f
        },
        
        // Уровень 8: Грандмастер
        new DifficultySettings {
            minNumber = 1,
            maxNumber = 9,
            wrongAnswerRange = 6,
            totalQuestions = 10,
            useTrickyAnswers = true,
            timeBonusThreshold = 5f
        }
    };

    // Публичный метод для получения настроек сложности
    public DifficultySettings GetDifficultyForLevel(string levelId)
    {
        int levelNumber = ExtractLevelNumber(levelId);
        int difficultyIndex = Mathf.Clamp(levelNumber - 1, 0, difficultyLevels.Length - 1);
        return difficultyLevels[difficultyIndex];
    }

    // Публичный метод для извлечения номера уровня
    public int ExtractLevelNumber(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
            return 1;

        // Ищем цифры в строке levelId
        foreach (char c in levelId)
        {
            if (char.IsDigit(c))
            {
                return int.Parse(c.ToString());
            }
        }
        return 1;
    }

    // Дополнительный метод для получения количества уровней
    public int GetTotalLevels()
    {
        return difficultyLevels.Length;
    }

    // Метод для получения настроек по индексу уровня
    public DifficultySettings GetDifficultyByIndex(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < difficultyLevels.Length)
        {
            return difficultyLevels[levelIndex];
        }
        return difficultyLevels[0]; // fallback
    }
}
