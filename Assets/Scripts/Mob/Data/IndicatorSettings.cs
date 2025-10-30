using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// ScriptableObject that defines indicator visual settings
    /// Allows designers to configure indicator positioning and appearance
    /// </summary>
    [CreateAssetMenu(fileName = "IndicatorSettings", menuName = "Horror Game/Indicator Settings")]
    public class IndicatorSettings : ScriptableObject
    {
        [Header("Position Settings")]
        [Tooltip("Base position offset from mob center (X, Y, Z)")]
        public Vector3 baseOffset = new Vector3(0f, 2.5f, 0f);
        
        [Header("Spacing Settings")]
        [Tooltip("Gap between indicators when there are multiple (horizontal spacing)")]
        public float indicatorGap = 0.5f;
        
        [Header("Optional Adjustments")]
        [Tooltip("Additional rotation for indicators (if needed to face camera)")]
        public Vector3 rotation = Vector3.zero;
        
        [Tooltip("Scale multiplier for indicators")]
        public float scale = 1f;

        [Header("Animation Settings (Optional)")]
        [Tooltip("Enable bobbing animation for indicators")]
        public bool enableBobbing = false;
        
        [Tooltip("Bobbing speed")]
        public float bobbingSpeed = 1f;
        
        [Tooltip("Bobbing amount (Y axis)")]
        public float bobbingAmount = 0.1f;

        /// <summary>
        /// Get the position for an indicator at a specific index
        /// </summary>
        /// <param name="index">Index of the indicator (0-based)</param>
        /// <param name="totalCount">Total number of indicators</param>
        /// <returns>Local position offset</returns>
        public Vector3 GetIndicatorPosition(int index, int totalCount)
        {
            if (totalCount == 1)
            {
                // Single indicator, use base offset
                return baseOffset;
            }
            else
            {
                // Multiple indicators, spread them out horizontally
                float totalWidth = (totalCount - 1) * indicatorGap;
                float startX = -totalWidth / 2f;
                
                Vector3 position = baseOffset;
                position.x += startX + (index * indicatorGap);
                return position;
            }
        }
    }
}

