using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMotion : MonoBehaviour {

    public enum LoopType
    {
        None,
        PingPong,
        Repeat

    }

    public enum MoveType {
        Time,
        Speed
    }

    public bool autoStart = true;
    public bool doTranslate;
    public bool doRotate;
    public Vector3 startPosition = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 endPosition = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 rotateDegrees = new Vector3(0.0f, 0.0f, 0.0f);
    public float translateSpeed = 1.0f;
    public float rotateSpeed = 1.0f;
    public LoopType loopType;
    public MoveType moveType;
    
   
    // Use this for initialization
    void Start () {
		
        if (autoStart)
        {
            if (doTranslate)
            {
                StartCoroutine(Translation(transform, startPosition, endPosition, translateSpeed, moveType));
            }
            if (doRotate)
            {
                StartCoroutine(Rotation(transform, rotateDegrees, rotateSpeed));
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator TranslateTo(Transform thisTransform, Vector3 endPos, float value, MoveType mt)
    {
        yield return Translation(thisTransform, thisTransform.position, endPos, value, mt);
    }

    public IEnumerator Translation(Transform thisTransform, Vector3 endPos, float value, MoveType mt)
    {
        yield return Translation(thisTransform, thisTransform.position, thisTransform.position + endPos, value, mt);
    }

    public IEnumerator Translation(Transform thisTransform, Vector3 startPos, Vector3 endPos, float value, MoveType mt)
    {
        float rate = (mt == MoveType.Time) ? 1.0f / value : 1.0f / Vector3.Distance(startPos, endPos) * value;
        float t = 0.0f;
        while (t < 1.0)
        {
            t += Time.deltaTime * rate;
            thisTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
            yield return null;
        }
        if (loopType == LoopType.PingPong)
        {
            StartCoroutine(Translation(transform, endPos, startPos, value, mt));
        } else if (loopType == LoopType.Repeat)
        {
            StartCoroutine(Translation(transform, startPos, endPos, value, mt));
        }
    }

    public IEnumerator Rotation(Transform thisTransform, Vector3 degrees, float time)
    {
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(degrees);
        float rate = 1.0f / time;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        if (loopType == LoopType.PingPong)
        {
            StartCoroutine(Rotation(transform, degrees, time));
        }
        else if (loopType == LoopType.Repeat)
        {
            StartCoroutine(Rotation(transform, degrees, time));
        }
    }

}
