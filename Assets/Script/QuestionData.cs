using System;

[Serializable]
public class QuestionData
{
    public int number1;
    public int number2;
    public int correctAnswer;
    public int[] wrongAnswers;

    public QuestionData(int n1, int n2, int correct, int[] wrong)
    {
        number1 = n1;
        number2 = n2;
        correctAnswer = correct;
        wrongAnswers = wrong;
    }
}
