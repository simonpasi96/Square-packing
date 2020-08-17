using System.Collections.Generic;
using UnityEngine;

public class RectPacker
{
    /// <summary>
    /// Pack rects inside of a square. Rects should be ordered descending from their volume.
    /// </summary>
    public static RectPack PackOrderedRects(List<Rect> rects)
    {
        // Find the best packing.
        Rect drawingArea = new Rect(0, 0, 1, 1);
        float resizeStep = Mathf.Min(rects[rects.Count - 1].width, rects[rects.Count - 1].height);
        RectPack finalResult = null;
        RectPack previousResult = null;
        while (finalResult == null)
        {
            RectPack tempResult = TryPackingRects(rects, drawingArea);

            if (tempResult.rects.Count == rects.Count)
            {
                // Success : retry a pack with a smaller area.
                previousResult = tempResult;
                drawingArea = new Rect(drawingArea.position, drawingArea.size - Vector2.one * resizeStep);
            }
            else
            {
                // Failure: keep the previous result if it was a success, else retry with a bigger area.
                if (previousResult != null && previousResult.rects.Count == rects.Count)
                    finalResult = previousResult;
                else
                    drawingArea = new Rect(drawingArea.position, drawingArea.size + Vector2.one * resizeStep);
            }
        }

        // Trim the edges of the packing area.
        float maxWidth = 0;
        float maxHeight = 0;
        foreach (Rect rect in finalResult.rects)
        {
            if (rect.position.x + rect.width > maxWidth)
                maxWidth = rect.position.x + rect.width;
            if (rect.position.y + rect.width > maxHeight)
                maxHeight = rect.position.y + rect.height;
        }
        finalResult.area.size = Vector2.one * Mathf.Max(maxWidth, maxHeight);

        return finalResult;
    }

    static RectPack TryPackingRects(List<Rect> rects, Rect drawingArea)
    {
        // Try to pack all rects.
        List<Rect> emptySpaces = new List<Rect>();
        emptySpaces.Add(drawingArea);
        List<Rect> storedRects = new List<Rect>();
        bool couldntPackRect = false;
        for (int i = 0; i < rects.Count && !couldntPackRect; i++)
        {
            PackedRect packedRect = null;
            // Try to pack the rect in an empty space.
            for (int j = emptySpaces.Count - 1; j >= 0 && packedRect == null; j--)
            {
                packedRect = StoreRect(rects[i], emptySpaces[j]);
                if (packedRect != null)
                {
                    storedRects.Add(new Rect(packedRect.storedRect));
                    emptySpaces.RemoveAt(j);
                    emptySpaces.Add(packedRect.biggerEmptyRect);
                    emptySpaces.Add(packedRect.smallerEmptyRect);
                }
            }

            // If we can't pack this rect, stop.
            if (packedRect == null)
                couldntPackRect = true;
        }

        // Return the rects that we could pack.
        return new RectPack(drawingArea, storedRects);
    }

    static PackedRect StoreRect(Rect target, Rect destination)
    {
        if (target.width > destination.width || target.height > destination.height)
            return null;

        Rect storedRect = new Rect(destination.x, destination.y, target.width, target.height);
        bool shouldUseWidth = target.width >= target.height;
        Rect smallerEmptyRect = new Rect(
            destination.x + target.width, destination.y,
            destination.width - target.width, target.height);
        Rect biggerEmptyRect = new Rect(
            destination.x, destination.y + target.height,
            destination.width, destination.height - target.height);
        return new PackedRect(storedRect, smallerEmptyRect, biggerEmptyRect);
    }
}

public class RectPack
{
    public Rect area;
    public List<Rect> rects;

    public RectPack(Rect area, List<Rect> rects)
    {
        this.area = area;
        this.rects = rects;
    }
}

class PackedRect
{
    public Rect storedRect;
    public Rect smallerEmptyRect, biggerEmptyRect;

    public PackedRect(Rect storedRect, Rect smallerEmptyRect, Rect biggerEmptyRect)
    {
        this.storedRect = storedRect;
        this.smallerEmptyRect = smallerEmptyRect;
        this.biggerEmptyRect = biggerEmptyRect;
    }
}