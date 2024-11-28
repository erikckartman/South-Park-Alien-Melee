using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct Spawner : ISystem
{
    //private EntityArchetype enemyArchetype;

    //// Ініціалізація архетипу при створенні системи
    //void OnCreate()
    //{
    //    enemyArchetype = this.EntityManager.CreateArchetype(
    //        typeof(Enemy),      // Ваш компонент
    //        typeof(Translation), // Позиція
    //        typeof(Rotation)     // Орієнтація
    //    );
    //}

    //// Оновлення на кожному кадрі
    //void OnUpdate()
    //{
    //    if (Time.deltaTime % 5.0f < 0.1f) // Створення ворога кожні 5 секунд
    //    {
    //        // Створення нового ворога
    //        Entity enemy = this.EntityManager.CreateEntity(enemyArchetype);

    //        // Встановлення позиції
    //        this.EntityManager.SetComponentData(enemy, new Translation { Value = new float3(0, 0, 0) });

    //        // Встановлення компонентів Enemy
    //        this.EntityManager.SetComponentData(enemy, new Enemy { hp = 100, speed = 2.0f });
    //    }
    //}
}
