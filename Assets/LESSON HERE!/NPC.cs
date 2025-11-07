using System.Collections.Generic;
using System.IO;
using UnityEngine;

//This script represents an NPC that has a name, specific ID, and some status
//When interact,it called dialogue loader to present its speech
public class NPC : MonoBehaviour
{
    public string myName;
    //Protected basically means private
    //However, it allows its child class to read this property
    protected string currentGID = "testing";
    protected bool talked = false;
    protected bool interactable = false;

    private void Start()
    {
    }

    private void Update()
    {
        InteractDetect();
    }

    public void InteractDetect() 
    {
        if (Input.GetKeyDown(KeyCode.E) && !talked && interactable)
        {
            talked = true;
            DialogueLoader.Instance.StartDialogue(currentGID, this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        interactable = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        interactable = false;
    }


}
