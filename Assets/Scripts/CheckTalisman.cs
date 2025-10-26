using UnityEngine;

public class CheckTalisman : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public Texture2D SourceTexture; // 
    public Texture2D TemplateTexture; // player draw
    public float MinMatchScoreThreshold = 0.05f; // Threshold for a good match (SSD is usually low for a match)
    private LineRendererToTexture lineTexture;

    private void Start()
    {
        TemplateTexture = lineTexture.SaveLineRendererToTexture();
        if (SourceTexture == null || TemplateTexture == null)
        {
            Debug.LogError("SourceTexture and TemplateTexture must be assigned in the Inspector.");
            return;
        }

        // Ensure textures are readable
        if (!SourceTexture.isReadable || !TemplateTexture.isReadable)
        {
            Debug.LogError("Both textures must have 'Read/Write Enabled' set in their import settings.");
            return;
        }

        Vector2Int matchPosition = FindTemplateMatch(SourceTexture, TemplateTexture, out float bestScore);

        if (bestScore < MinMatchScoreThreshold)
        {
            Debug.Log($"Template found! Position: ({matchPosition.x}, {matchPosition.y}) with SSD score: {bestScore}");
            DrawMatchRectangle(matchPosition, TemplateTexture.width, TemplateTexture.height, Color.red);
        }
        else
        {
            Debug.Log($"No strong match found. Best SSD score: {bestScore} (Threshold: {MinMatchScoreThreshold})");
        }
    }

    /// <summary>
    /// Finds the best match position for the template in the source using Sum of Squared Differences (SSD).
    /// </summary>
    /// <param name="source">The larger image to search within.</param>
    /// <param name="template">The smaller image to find.</param>
    /// <param name="bestScore">The lowest SSD score found (lower is better).</param>
    /// <returns>The bottom-left pixel coordinate of the best match.</returns>
    private Vector2Int FindTemplateMatch(Texture2D source, Texture2D template, out float bestScore)
    {
        int sourceW = source.width;
        int sourceH = source.height;
        int templateW = template.width;
        int templateH = template.height;

        Color[] sourcePixels = source.GetPixels();
        Color[] templatePixels = template.GetPixels();

        bestScore = float.MaxValue;
        Vector2Int bestMatchPos = Vector2Int.zero;

        // Iterate through all possible starting positions (bottom-left corner) of the template in the source
        for (int x = 0; x <= sourceW - templateW; x++)
        {
            for (int y = 0; y <= sourceH - templateH; y++)
            {
                // Calculate SSD for the current position
                float currentScore = CalculateSSD(sourcePixels, templatePixels, sourceW, templateW, templateH, x, y);

                if (currentScore < bestScore)
                {
                    bestScore = currentScore;
                    bestMatchPos = new Vector2Int(x, y);
                }
            }
        }

        return bestMatchPos;
    }

    /// <summary>
    /// Calculates the Sum of Squared Differences (SSD) between a template and a patch of the source image.
    /// </summary>
    private float CalculateSSD(Color[] sourcePixels, Color[] templatePixels, int sourceW, int templateW, int templateH, int startX, int startY)
    {
        float ssd = 0f;

        // Iterate through the pixels of the template
        for (int ty = 0; ty < templateH; ty++)
        {
            for (int tx = 0; tx < templateW; tx++)
            {
                // Calculate the corresponding pixel index in the template
                int templateIndex = ty * templateW + tx;

                // Calculate the corresponding pixel index in the source (current patch)
                int sourceIndex = (startY + ty) * sourceW + (startX + tx);

                Color tempColor = templatePixels[templateIndex];
                Color sourceColor = sourcePixels[sourceIndex];

                // Calculate the squared difference for each color component (R, G, B)
                // Note: Alpha is usually ignored in standard template matching unless specifically needed.
                float diffR = tempColor.r - sourceColor.r;
                float diffG = tempColor.g - sourceColor.g;
                float diffB = tempColor.b - sourceColor.b;

                // Sum the squared differences
                ssd += (diffR * diffR) + (diffG * diffG) + (diffB * diffB);
            }
        }

        // Normalize the score by dividing by the number of pixels in the template.
        // This makes the threshold value less dependent on the template size.
        return ssd / (templateW * templateH);
    }

    /// <summary>
    /// Utility to draw a rectangle on the source texture for visualization.
    /// </summary>
    private void DrawMatchRectangle(Vector2Int startPos, int width, int height, Color color)
    {
        if (SourceTexture == null) return;

        // Draw horizontal lines
        for (int x = startPos.x; x < startPos.x + width; x++)
        {
            if (x >= 0 && x < SourceTexture.width)
            {
                if (startPos.y >= 0 && startPos.y < SourceTexture.height)
                    SourceTexture.SetPixel(x, startPos.y, color); // Bottom
                if (startPos.y + height - 1 >= 0 && startPos.y + height - 1 < SourceTexture.height)
                    SourceTexture.SetPixel(x, startPos.y + height - 1, color); // Top
            }
        }

        // Draw vertical lines
        for (int y = startPos.y; y < startPos.y + height; y++)
        {
            if (y >= 0 && y < SourceTexture.height)
            {
                if (startPos.x >= 0 && startPos.x < SourceTexture.width)
                    SourceTexture.SetPixel(startPos.x, y, color); // Left
                if (startPos.x + width - 1 >= 0 && startPos.x + width - 1 < SourceTexture.width)
                    SourceTexture.SetPixel(startPos.x + width - 1, y, color); // Right
            }
        }

        SourceTexture.Apply(); // Apply changes to the texture
    }
}
