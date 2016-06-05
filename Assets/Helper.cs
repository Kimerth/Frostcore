using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Helper : MonoBehaviour 
{
    /// <summary>
    /// Generic method that swaps two objects
    /// </summary>
    /// <typeparam name="T"> Object type </typeparam>
    /// <param name="lhs"> The reference of the first object </param>
    /// <param name="rhs"> The reference of the second obhect </param>
    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
	
    /// <summary>
    /// Generic method that swaps tho objects in a list
    /// </summary>
    /// <typeparam name="T"> Object type </typeparam>
    /// <param name="list"> The list in which we swap the objects </param>
    /// <param name="il"> The index of the first object </param>
    /// <param name="ir"> The index of the second object </param>
    public static void Swap4List<T>(List<T> list, int il, int ir)
    {
        T temp;
        temp = list[il];
        list[il] = list[ir];
        list[ir] = temp;
    }

    /// <summary>
    /// Sets every element in the given array to the given value
    /// </summary>
    /// <typeparam name="T"> Object type </typeparam>
    /// <param name="arr"> The array to populate </param>
    /// <param name="value"> The value every element should have</param>
    public static void Populate<T>(T[] arr, T value)
    {
        for (int index = 0; index < arr.Length; index++)
            arr[index] = value;
    }

    /// <summary>
    /// Finds the first occurrence of value in the given array, then returns it's index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"> The array in which to look </param>
    /// <param name="value"> The value to search </param>
    /// <returns> The index of the first occurrence </returns>
    public static int FindIndex<T>(T[] arr, T value)
    {
        for (int index = 0; index < arr.Length; index++)
            if (value.Equals(arr[index]))
                return index;

        return -1;
    }

    /// <summary>
    /// Checks if the mouse is over an UI object
    /// </summary>
    /// <returns> true if yes </returns>
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

}
