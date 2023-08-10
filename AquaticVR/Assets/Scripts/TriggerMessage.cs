using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class TriggerEventMessage : UnityEvent<string>
{

}

public class TriggerMessage : MonoBehaviour {


    public enum TriggerMessageType
    {
        TriggerMessage,
        TriggerBroadcastMessage,
        TriggerMessageUpwards,
        TriggerEventMessage

    }

    public string[] receiverName; //Optional
    public GameObject receiverGameObject;
    public bool messageCollider;
    public TriggerMessageType triggerMessageType;
    public string[] enterMessage;
    public string[] enterData; //Optional
    public string[] exitMessage;
    public string[] exitData; //Optional
    public int sendCount; //After the message is send this number of times, it will not be send again. 0 IS UNLIMITED
    public string[] validColliders = new string[1] { "Player" };

    public TriggerEventMessage triggerEventMessage;

    private int enterCount;
    private int exitCount = 0;
    private GameObject receiver;

    void Awake()
    {
        if (GetComponent<Collider>())
        {
            GetComponent<Collider>().isTrigger = true;
        }

    }

    void Start()
    {

    }

    void OnTriggerEnter( Collider other )
    {
        //Debug.Log("[TriggerMessage] Enter ::::::::::::::::::::::::::::::>");
        bool isValid = ValidateOther(other);
        if (messageCollider)
        {
            SendEnterMessageToCollider(other);
        }
        if (isValid)
        {
            //Debug.Log("[TriggerMessage] Enter ::::::::::::::::::::::::::::::>");
            if (enterCount < sendCount || sendCount == 0)
            {
                if (triggerMessageType == TriggerMessageType.TriggerMessage)
                {
                    TriggerEnterMessage();
                }
                else if (triggerMessageType == TriggerMessageType.TriggerBroadcastMessage)
                {
                    TriggerEnterBroadcastMessage();
                }
                else if (triggerMessageType == TriggerMessageType.TriggerMessageUpwards)
                {
                    TriggerEnterMessageUpwards();
                }
                else if (triggerMessageType == TriggerMessageType.TriggerEventMessage)
                {
                    TriggerEventEnterMessage();
                }
            }
            enterCount++;
        }

    }
    
    void OnTriggerExit(Collider other )
    {
        //Debug.Log("[TriggerMessage] Exit ::::::::::::::::::::::::::::::<");
        bool isValid = ValidateOther(other);
        if (isValid)
        {
            //Debug.Log("[TriggerMessage] Exit ::::::::::::::::::::::::::::::<");
            if (exitCount < sendCount || sendCount == 0)
            {
                if (triggerMessageType == TriggerMessageType.TriggerMessage)
                {
                    TriggerExitMessage();
                }                
                else if (triggerMessageType == TriggerMessageType.TriggerMessageUpwards)
                {
                    TriggerExitMessageUpwards();
                }
                else if (triggerMessageType == TriggerMessageType.TriggerBroadcastMessage)
                {
                    TriggerExitBroadcastMessage();
                }
                else if (triggerMessageType == TriggerMessageType.TriggerEventMessage)
                {
                    TriggerEventExitMessage();
                }
            }
            exitCount++;
        }
    }

    void SendEnterMessageToCollider( Collider other )
    {
        GameObject go = other.gameObject;
        foreach (string em in enterMessage)
        {
            go.SendMessage(em, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TriggerEnterMessage()
    {
        

        if (receiverName.Length > 0)
        {
            foreach (string re in receiverName)
            {
                if (GameObject.Find(re))
                {
                    receiver = GameObject.Find(re);
                }
                else if (GameObject.FindWithTag(re))
                {
                    receiver = GameObject.FindWithTag(re);
                }

                foreach (string em in enterMessage)
                {
                    if (enterData.Length > 0)
                    {
                        receiver.SendMessage(em, enterData[0], SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        receiver.SendMessage(em, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
        else if (receiverGameObject)
        {
            receiver = receiverGameObject;
            foreach (string em in enterMessage)
            {
                //Debug.Log("Send Trigger Message : " + em);
                if (enterData.Length > 0)
                {
                    receiver.SendMessage(em, enterData[0], SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    receiver.SendMessage(em, SendMessageOptions.DontRequireReceiver);
                }
            }

        }
        else
        {
            foreach (string mess in enterMessage)
            {
                if (enterData.Length > 0)
                {
                    receiver.SendMessage(mess, enterData[0], SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    SendMessage(mess, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    void TriggerEnterBroadcastMessage()
    {
        
        if (receiverName.Length > 0)
        {
            foreach (string re in receiverName)
            {
                if (receiverGameObject)
                {
                    receiver = receiverGameObject;
                }
                else
                {
                    if (GameObject.Find(re))
                    {
                        receiver = GameObject.Find(re);
                    }
                    else if (GameObject.FindWithTag(re))
                    {
                        receiver = GameObject.FindWithTag(re);
                    }
                }
                foreach (string em in enterMessage)
                {
                    receiver.BroadcastMessage(em, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else if (receiverGameObject)
        {
            receiver = receiverGameObject;
            foreach (string em in enterMessage)
            {
                receiver.BroadcastMessage(em, SendMessageOptions.DontRequireReceiver);
            }

        }
        else
        {
            foreach (string mess in enterMessage)
            {
                BroadcastMessage(mess, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void TriggerEnterMessageUpwards()
    {
        //Debug.Log("[TriggerMessageUpwards] Enter ::::::::::::::::::::::::::::::<");
        foreach (string em in enterMessage)
        {

            gameObject.SendMessageUpwards(em, SendMessageOptions.DontRequireReceiver);
        }
    }

    void TriggerExitMessageUpwards()
    {
        //Debug.Log("[TriggerMessageUpwards] Exit ::::::::::::::::::::::::::::::<");
        foreach (string em in enterMessage)
        {
            gameObject.SendMessageUpwards(em, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void TriggerEventEnterMessage()
    {

        foreach (string em in enterMessage)
        {
            triggerEventMessage.Invoke(em);
        }


    }

    void TriggerEventExitMessage()
    {

        foreach (string em in exitMessage)
        {
            triggerEventMessage.Invoke(em);
        }


    }

    void TriggerExitMessage()
    {
        
        if (receiverName.Length > 0)
        {
            foreach (string re in receiverName)
            {

                if (GameObject.Find(re))
                {
                    receiver = GameObject.Find(re);
                }
                else if (GameObject.FindWithTag(re))
                {
                    receiver = GameObject.FindWithTag(re);
                }

                foreach (string em in exitMessage)
                {
                    if (exitData.Length > 0)
                    {
                        receiver.SendMessage(em, exitData[0], SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        receiver.SendMessage(em, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
        else if (receiverGameObject)
        {
            receiver = receiverGameObject;
            foreach (string em in exitMessage)
            {
                if (exitData.Length > 0)
                {
                    receiver.SendMessage(em, exitData[0], SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    receiver.BroadcastMessage(em, SendMessageOptions.DontRequireReceiver);
                }
            }

        }
        else
        {
            foreach (string mess in exitMessage)
            {
                if (exitData.Length > 0)
                {
                    receiver.SendMessage(mess, exitData[0], SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    SendMessage(mess, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    void TriggerExitBroadcastMessage()
    {
        
        if (receiverName.Length > 0)
        {
            foreach (string re in receiverName)
            {
                if (GameObject.Find(re))
                {
                    receiver = GameObject.Find(re);
                }
                else if (GameObject.FindWithTag(re))
                {
                    receiver = GameObject.FindWithTag(re);
                }

                foreach (string em in exitMessage)
                {
                    receiver.BroadcastMessage(em, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else if (receiverGameObject)
        {
            receiver = receiverGameObject;
            foreach (string em in exitMessage)
            {
                receiver.BroadcastMessage(em, SendMessageOptions.DontRequireReceiver);
            }

        }
        else
        {
            foreach (string em in exitMessage)
            {
                
                    BroadcastMessage(em, SendMessageOptions.DontRequireReceiver);
                
            }
        }
    }
    
    public bool ValidateOther(Collider other )
    {
	
	    bool valid = false;
        if (validColliders.Length == 0)
        {
            //Debug.Log("ValidateOther : " + (other.gameObject.name.ToString()) + " Is Valid : " + (valid.ToString()));
            valid = true;
            return valid;
        }

	    foreach(string val in validColliders){
		    /*
		    if(other.gameObject == GameObject.FindWithTag(val)){	
			    valid = true;
		    } 
		    */
		    if((other.gameObject.name.ToString()) == val){	
			    valid = true;
		    }
		    if((other.gameObject.tag.ToString()) == val){	
			    valid = true;
		    }
		
	    }
	    if (valid){
		    Debug.Log("ValidateOther : " + (other.gameObject.name.ToString()) + " Is Valid : " + (valid.ToString()));
	
	    }
	    return valid;	

    }

    [ContextMenu("PinTriggerToGround")]
    public void PinTriggerToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1000.0f))
        {
            float hitY = hit.point.y;
            Vector3 tPos = transform.position;
            tPos.y = hitY;
            transform.position = tPos;
        }
    }
}
