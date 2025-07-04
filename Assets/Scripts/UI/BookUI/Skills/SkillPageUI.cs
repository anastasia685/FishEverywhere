using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class SkillsPageUI : MonoBehaviour, IBookPageUI
{
    public static SkillsPageUI Instance;

    [SerializeField] public GameObject UI;
    [SerializeField] RectTransform overviewGraphUI;
    [SerializeField] Transform graphNodesUI;
    [SerializeField] Transform graphLinesUI;


    [SerializeField] GameObject entryButtonPrefab;
    [SerializeField] GameObject linePrefab;

    //buttons
    //public Button[] Buttons;
    Dictionary<string, GameObject> entryButtons = new();
    string initialEntryId;

    // page parts
    [SerializeField] Image image;
    [SerializeField] Text skill;
    [SerializeField] Text description;
    [SerializeField] Text cost;
    [SerializeField] Button activateButton;

    //public SkillSO root;

    //SFX
    [SerializeField] AudioClip BookOpenSFX;
    [SerializeField] AudioClip PageTurnSFX;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize()
    {
        initialEntryId = SkillManager.Instance.rootSO.id;

        GenerateSkillTreeUI();
    }

    public void Show()
    {
        //SFX
        AudioSource.PlayClipAtPoint(BookOpenSFX, Camera.main.transform.position, 1f);

        SetPage();

        UI.SetActive(true);
    }

    public void Hide()
    {
        UI.SetActive(false);
    }

    public void SetAsActive()
    {
        BookUI.Instance.SetActivePage(Instance);
    }

    /*Dictionary<int, List<string>> levels = new();
    void CollectLevels(SkillSO skill, int depth = 0)
    {
        if (!levels.ContainsKey(depth))
            levels[depth] = new List<string>();

        levels[depth].Add(skill.id);

        foreach (var child in skill.children)
        {
            CollectLevels(child, depth + 1);
        }
    }*/

    public void ToggleSkills()
    {
        // going from off to on -> update content first
        if (!UI.activeSelf)
        {
            //SFX
            AudioSource.PlayClipAtPoint(BookOpenSFX, Camera.main.transform.position, 1f);

            //SetPage(root);
            SetPage();

            //get rid of other pages
            BestiaryPageUI.Instance.UI.SetActive(false);
            AchievementsPageUI.Instance.UI.SetActive(false);
        }
        UI.SetActive(!UI.activeSelf);
    }

    public void SetPage(/*SkillSO details*/)
    {
        //SFX
        AudioSource.PlayClipAtPoint(PageTurnSFX, Camera.main.transform.position, 1f);

        //set ui elements to desplay info
        /*image.sprite = details.skillIcon;
        skill.text = details.skillName;
        description.text = details.description;
        cost.text = details.cost + "p";
        activateButton.GetComponent<ActivateSkillButtonUI>().skillSO = details;*/

        //SetBackground(details);
        SetDetails(initialEntryId);
        //SetButtons();
    }


    Dictionary<string, RectTransform> nodeUIRefs = new();
    Vector2 min = Vector2.zero;
    Vector2 max = Vector2.zero;
    void GenerateSkillTreeUI()
    {
        foreach (var entry in entryButtons)
        {
            Destroy(entry.Value);
        }
        entryButtons.Clear();
        nodeUIRefs.Clear();

        min = Vector2.zero;
        max = Vector2.zero;

        float buttonSize = 50f;
        float spacing = 20f;
        float horizontalSpacing = buttonSize + spacing;
        float verticalSpacing = buttonSize + spacing;

        // Start recursive layout
        LayoutNodeRecursive(SkillManager.Instance.rootSO, 0, 0, horizontalSpacing, verticalSpacing);

        max = max + new Vector2(buttonSize, buttonSize);

        // Resize content bounds
        Vector2 size = max - min;


        //var rectTransform = overviewGraphUI.GetComponent<RectTransform>();

        // Respect minimum width configured in the Inspector
        //size.x = Mathf.Max(size.x, overviewGraphUI.sizeDelta.x);
        //size.y = Mathf.Max(size.y, overviewGraphUI.sizeDelta.y);

        overviewGraphUI.sizeDelta = size;
    }
    Vector2 LayoutNodeRecursive(SkillSO node, int depth, float xStart, float hSpacing, float vSpacing)
    {
        float y = -depth * vSpacing;

        // Base case: no children -> just place this node
        if (node.children == null || node.children.Count == 0)
        {
            Vector2 pos = new Vector2(xStart, y);
            CreateNodeUI(node.id, pos);
            return pos;
        }

        // Layout children left to right
        List<Vector2> childPositions = new();
        float currentX = xStart;

        foreach (var child in node.children)
        {
            var childPos = LayoutNodeRecursive(child, depth + 1, currentX, hSpacing, vSpacing);
            childPositions.Add(childPos);
            currentX = childPos.x + hSpacing;
        }

        // Center parent above children
        float minX = childPositions.First().x;
        float maxX = childPositions.Last().x;
        float centerX = (minX + maxX) / 2f;

        Vector2 myPos = new Vector2(centerX, y);
        CreateNodeUI(node.id, myPos);

        // Draw connection lines
        foreach (var child in node.children)
        {
            DrawUILine(nodeUIRefs[node.id], nodeUIRefs[child.id]);
        }

        return myPos;
    }
    void CreateNodeUI(string nodeId, Vector2 position)
    {
        var buttonObj = Instantiate(entryButtonPrefab, graphNodesUI);
        var buttonScript = buttonObj.GetComponent<SkillEntryButtonUI>();
        buttonScript.skillId = nodeId;
        buttonScript.clickCallback = SetDetails;
        var rect = buttonObj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;

        var script = buttonObj.GetComponent<SkillEntryButtonUI>();
        script.skillId = nodeId;

        entryButtons.Add(nodeId, buttonObj);
        nodeUIRefs[nodeId] = rect;

        // Update bounds
        min = Vector2.Min(min, position);
        max = Vector2.Max(max, position);
    }
    void DrawUILine(RectTransform from, RectTransform to)
    {
        var line = Instantiate(linePrefab, graphLinesUI);
        var rect = line.GetComponent<RectTransform>();

        // Adjust positions based on pivot + height
        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;

        // Offset to center-top of the from and to RectTransforms
        fromPos += new Vector2(from.rect.width * 0.5f, -from.rect.height * 0.5f);
        toPos += new Vector2(to.rect.width * 0.5f, -to.rect.height * 0.5f);

        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;

        float lineThickness = 4f;

        rect.sizeDelta = new Vector2(distance, lineThickness);
        rect.anchoredPosition = fromPos + new Vector2(lineThickness / 2, 0); // vertical center offset
        rect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }


    /*/public void SetBackground(SkillSO details)
    {
        //change color of sprite based on if youve learnt it
        Image background = image.transform.parent.GetComponent<Image>();
        if (details.active)
        {
            background.color = Color.green;
        }
        else if (details.available)
        {
            background.color = Color.white;
        }
        else
        {
            background.color = Color.gray;
        }
    }
    removed as there is no longer a background sprite to change/*/

    public void SetDetails(string skillId)
    {
        var entryData = SkillManager.Instance.GetEntry(skillId);

        // set ui elements to desplay info
        image.sprite = entryData.skill.skillIcon;
        skill.text = entryData.skill.skillName;
        description.text = entryData.skill.description;
        cost.text = entryData.skill.cost.ToString();
        activateButton.GetComponent<ActivateSkillButtonUI>().skillId = skillId;
    }
    public void SetButtons(string skillId)
    {
        // update activated skill button and its immediate children

        var button = entryButtons.TryGetValue(skillId, out var btn) ? btn : null;
        if (button == null) return;

        button.GetComponent<SkillEntryButtonUI>().SetButton();

        var entryData = SkillManager.Instance.GetEntry(skillId);
        foreach(var child in entryData.skill.children)
        {
            button = entryButtons.TryGetValue(child.id, out btn) ? btn : null;
            if (button == null) continue;

            button.GetComponent<SkillEntryButtonUI>().SetButton();
        }
    }

    /*public void SetButtons()
    {
        //set buttons based on if youve learnt that skill
        for (int i = 0; i < Buttons.Length; i++)
        {
            SetSkillsButtonUI ButtonScript = Buttons[i].GetComponent<SetSkillsButtonUI>();
            ButtonScript.SetButton();
        }
    }*/
}
