using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordWall : MonoBehaviour {
    static Dictionary<Vector2, WordWallText> generatedWalls = new Dictionary<Vector2, WordWallText>();
    public List<WordWallText> texts;

    // Tuning

    [SerializeField] private float fadeSpeed;

    // References
    private Renderer rend;
    private UnityEngine.UI.Text screenText;

    private bool displaying = false;
    private bool generated = false;
    private WordWallText chosenText;


    // Use this for initialization
    void Start () {
        rend = transform.FindChild("model").GetComponent<Renderer>();
        screenText = GameObject.Find("WordWallText").GetComponent<UnityEngine.UI.Text>();
        screenText.color = new Color(0,0,0,0);

        // If we have already generated this one, load it
        if (generatedWalls.ContainsKey(GenerationManager.worldToChunk(transform.position))){
            chosenText = generatedWalls[GenerationManager.worldToChunk(transform.position)];
            generated = true;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!generated && Vector3.Distance(transform.position, Globals.Player.transform.position) < 100)
        {
            generateText();
        }
	}

    void OnMouseDown()
    {
        if(!displaying && Vector3.Distance(transform.position, Globals.Player.transform.position) < 10)
        {
            displaying = true;
            StartCoroutine("displayText");
        }
    }

    private void generateText()
    {
        // Choose a text to show that hasn't been shown
        chosenText = texts[Random.Range(0, texts.Count)];

        int tries = 0;
        while (chosenText.chosen && tries < 30)
        {
            chosenText = texts[Random.Range(0, texts.Count)];
            tries++;
        }
        chosenText.chosen = true;
        // Apply that text's material to the mural
        Material[] newMats = rend.sharedMaterials;
        newMats[1] = chosenText.renderedText;
        rend.sharedMaterials = newMats;
        generatedWalls[GenerationManager.worldToChunk(transform.position)] = chosenText;
        generated = true;   
    }

    private IEnumerator displayText()
    {
        displaying = true;
        screenText.text = chosenText.text;
        float a = 0;
        // Fade the text in
        while (a < 1)
        {
            screenText.color = new Color(255, 255, 255, a);
            a += Time.deltaTime * fadeSpeed;
            a = Mathf.Clamp(a, 0, 1);
            yield return null;
        }
        yield return new WaitForSeconds(10f);
        // Fade the text out
        while (a > 0)
        {
            screenText.color = new Color(255, 255, 255, a);
            a -= Time.deltaTime * fadeSpeed;
            a = Mathf.Clamp(a, 0, 1);
            yield return null;
        }
        screenText.color = new Color(255, 255, 255, 0);
        displaying = false;
    }

    
}
