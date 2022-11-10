using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    private static System.Random rng = new System.Random();  

    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static IEnumerator MoveTo(Transform target, Vector3 posFrom, Vector3 posTo, Vector3 rotFrom, Vector3 rotTo, float time)
    {
        float start = Time.time;

        while(true)
        {
            float t = Mathf.Clamp01((Time.time - start) / time);
            target.position = Vector3.Lerp(posFrom, posTo, t);
            target.localEulerAngles = Vector3.Lerp(rotFrom, rotTo, t);
            yield return new WaitForEndOfFrame();

            if(t >= 1)
            {
                break;
            }
        }
    }
}