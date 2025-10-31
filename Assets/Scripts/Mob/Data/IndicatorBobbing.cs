using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Simple component that makes indicators bob up and down
    /// Attached dynamically if bobbing is enabled in IndicatorSettings
    /// </summary>
    public class IndicatorBobbing : MonoBehaviour
    {
        [HideInInspector] public float bobbingSpeed = 1f;
        [HideInInspector] public float bobbingAmount = 0.1f;

        private Vector3 startPosition;
        private float time;

        private void Start()
        {
            startPosition = transform.localPosition;
            time = Random.Range(0f, 100f); // Random start time for variety
        }

        private void Update()
        {
            time += Time.deltaTime * bobbingSpeed;
            
            Vector3 newPosition = startPosition;
            newPosition.y += Mathf.Sin(time) * bobbingAmount;
            
            transform.localPosition = newPosition;
        }
    }
}

