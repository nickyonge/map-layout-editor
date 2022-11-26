using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorComparator
{

/// <summary> Returns a value from 0-1 based on how different the two colors are, 
/// with 0 being a perfect match </summary>
    public static float GetColorDifference(Color colorA, Color colorB) {

        float ar = colorA.r;
        float ag = colorA.g;
        float ab = colorA.b;

        float br = colorB.r;
        float bg = colorB.g;
        float bb = colorB.b;
        
        float rDiff = Mathf.Abs(ar - br);
        float gDiff = Mathf.Abs(ag - bg);
        float bDiff = Mathf.Abs(ab - bb);

        // Debug.Log("AR: " + ar + ", Ag: " + ag + ", Ab: " + ab);
        // Debug.Log("BR: " + br + ", Bg: " + bg + ", Bb: " + bb);
        // Debug.Log("R: " + rDiff + ", g: " + gDiff + ", b: " + bDiff);

        return (rDiff + gDiff + bDiff) / 3;

    }


}