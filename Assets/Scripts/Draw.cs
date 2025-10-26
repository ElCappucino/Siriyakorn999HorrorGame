using UnityEngine;
using System.Collections.Generic;

public class Draw : MonoBehaviour
{
    [SerializeField] private float timerDelay;
    [SerializeField] private float lineWidth;
    [SerializeField] private GameObject card;

    private List<Vector3> linePoints;
    private float timer;
    private GameObject newLine;
    private LineRenderer drawLine;
    private bool onCard = false;
    private Ray ray;

    void Start()
    {
        linePoints = new List<Vector3>();
        timer = timerDelay;
    }
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.transform.tag == "Tailsman")
        {
            onCard = true;
        }

        else
        {
            onCard = false;
            linePoints.Clear();
        }

        if (onCard) 
        {
            if (Input.GetMouseButtonDown(0))
            {
                newLine = new GameObject();
                drawLine = newLine.AddComponent<LineRenderer>();
                drawLine.material = new Material(Shader.Find("Sprites/Default"));
                drawLine.startColor = Color.red;
                drawLine.endColor = Color.red;
                drawLine.startWidth = lineWidth;
                drawLine.endWidth = lineWidth;
                drawLine.positionCount = linePoints.Count;
                drawLine.SetPositions(linePoints.ToArray());
            }

            if (Input.GetMouseButton(0))
            {
                //Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), GetMousePosition(), Color.red);
                //Debug.Log("Draw");
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    linePoints.Add(GetMousePosition());
                    drawLine.positionCount = linePoints.Count;
                    drawLine.SetPositions(linePoints.ToArray());
                    timer = timerDelay;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                //newLine.transform.parent = card.transform;
                //Debug.Log(linePoints);
                linePoints.Clear();
                onCard = false;
            }
        }
    }

    Vector3 GetMousePosition()
    {
        return ray.origin + ray.direction * 10;
    }
}
