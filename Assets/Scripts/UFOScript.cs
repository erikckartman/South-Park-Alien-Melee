using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOScript : MonoBehaviour
{
    private Vector3 center = new Vector3(0, 0, 0);
    private float radius = 100f;
    private float speed = 1f;

    private float angle = 0f;
    private void Update()
    {
        angle += speed * Time.deltaTime;

        float x = center.x + Mathf.Cos(angle) * radius;
        float y = center.y + Mathf.Sin(angle) * radius;

        transform.position = new Vector3(x, transform.position.y, y);
    }
}
