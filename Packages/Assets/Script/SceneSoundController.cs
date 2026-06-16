using UnityEngine;

public class SceneSoundController : MonoBehaviour
{
    void Start()
    {
        // AudioManager автоматически обрабатывает музыку при смене сцен
        // Ётот скрипт можно оставить пустым или удалить из всех сцен

        Debug.Log($"SceneSoundController: —цена {gameObject.scene.name} загружена");

        // –езервна€ проверка - если AudioManager почему-то не сработал
        if (AudioManager.Instance != null)
        {
            Debug.Log("AudioManager активен и управл€ет музыкой");
        }
        else
        {
            Debug.LogWarning("AudioManager не найден!");
        }
    }
}
