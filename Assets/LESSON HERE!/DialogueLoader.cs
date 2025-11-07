using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


//This script manage all the dialogue data and play the related dialogues when function called
//It's about how to use the data after you read the file, you only need to care about the file read part
[System.Serializable]
public class DialogueLine
{
    public string groupID;
    public int lineID;
    public string speaker;
    public string text;
    public string condition;
    public string next;
    public List<Choice> choices = new();
}

[System.Serializable]
public class Choice
{
    public string text;
    public int nextID;
}
public class DialogueLoader : MonoBehaviour
{
    public static DialogueLoader Instance { get; private set; }
    public TextAsset testDialogue;
    //Dictionary means many pairs of key & value
    //Apartment example: use the right "key" to access the right value
    //Here a specific string represents a dialogueID, and contains many dialogueLines
    private Dictionary<string, List<DialogueLine>> allDialogues = new();
    
    [Header("Dialogue")]
    [SerializeField] private TextMeshProUGUI speaker;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choicesLabel;
    [SerializeField] private GameObject choiceButtonPrefab;
    private string currentGroupID;
    private NPC currentNPC;
    private int currentIndex;
    public GameObject dialogueBox;
    private List<DialogueLine> currentDialogueLines;
    private bool speaking = false;

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        LoadDialogues(testDialogue);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && speaking) OnNextLine();
    }


    public void LoadDialogues(TextAsset txt) 
    {
       

    
    }


    //Part about managing the dialogue
    //But we don't care, all we need to do is correctly read the file 
    //And the dialogue system would do the rest of the jobs
    #region dialogue System
    void ShowCurrentLine()
    {
        if (currentIndex >= currentDialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogueLines[currentIndex];
        speaker.text = line.speaker;
        dialogueText.text = line.text;

        if (line.next == "END")
        {
            choicesLabel.SetActive(false);
        }
        else if (line.next == "CHOICE")
        {
            ShowChoices(line.choices);
        }
        else
        {
            choicesLabel.SetActive(false);
        }
    }
    //Show options in the dialogue
    void ShowChoices(List<Choice> choices)
    {
        foreach (Transform child in choicesLabel.transform)
        {
            if (child != choiceButtonPrefab.transform)
                Destroy(child.gameObject);
        }

        choicesLabel.SetActive(true);
        foreach (var choice in choices)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, Vector3.zero, Quaternion.identity);
            btn.transform.SetParent(choicesLabel.transform);
            btn.SetActive(true);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            btn.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    //change dialogue or status after selecting a option
    void OnChoiceSelected(Choice choice)
    {
        int targetIndex = currentDialogueLines.FindIndex(d => d.lineID == choice.nextID);
        if (targetIndex == -1) targetIndex = currentDialogueLines.Count;

        currentIndex = targetIndex;
        choicesLabel.SetActive(false);
        ShowCurrentLine();
    }

    //read next line in the dialogue
    public void OnNextLine()
    {
        DialogueLine line = currentDialogueLines[currentIndex];

        if (line.next == "CHOICE")
        {
            return;
        }

        if (line.next == "END")
        {
            EndDialogue();
            return;
        }

        if (int.TryParse(line.next, out int nextIndex))
        {
            currentIndex = nextIndex;
        }
        else
        {
            currentIndex++;
        }

        ShowCurrentLine();
    }

    //Hide dialogue Box
    void EndDialogue()
    {
        speaking = false;
        dialogueBox.SetActive(false);
        Player.Instance.PauseControl(true);
        Debug.Log("end dialogue");
        choicesLabel.SetActive(false);
        //no selling in this prototype
        //if (currentNPC.myName == "Mammon")
        //{
        //    Mammon.Instance.SellRandomItems();
        //}

    }

    //Start dialogue based on NPC and ID
    public void StartDialogue(string groupID, NPC NPC)
    {
        if (speaking) return;
        currentNPC = NPC;
        dialogueBox.SetActive(true);
        if (!allDialogues.ContainsKey(groupID))
        {
            Debug.LogError($"{NPC.name} can't talk---Dialogue no found:{groupID}");
            Debug.LogError($"Probably because there's no Dialogue.....");
            return;
        }
        speaking = true;
        Player.Instance.PauseControl(false);
        //Might move to other place
        SaveManager.Instance.dialogueFlags.Add(groupID);
        SaveManager.SaveGame();
        currentGroupID = groupID;
        currentDialogueLines = allDialogues[groupID];
        currentIndex = 0;
        choicesLabel.SetActive(false);
        ShowCurrentLine();
    }
    #endregion

}
