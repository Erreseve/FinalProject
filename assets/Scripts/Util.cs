using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util {

	public static Queue<T> ShuffleQueue<T>(Queue<T> q)
	{
		T[] qAsArray = q.ToArray ();

		for (int i = 0; i < qAsArray.Length; i++)
		{
			int pos = Random.Range (i, qAsArray.Length);
			T temp = qAsArray [pos];
			qAsArray [pos] = qAsArray [i];
			qAsArray [i] = temp; 
		}
		//return shuffled array as queue
		return new Queue<T> (qAsArray);
	}
}
