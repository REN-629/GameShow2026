using System.Collections.Generic;
using UnityEngine;
//ギミックの本体

public class ReactionReceiver : MonoBehaviour
{
    public string objectName;
    public List<WorldAttributeReaction> reactions = new List<WorldAttributeReaction>();

    public void TryUseItem(ItemData item)
    {
        List<string> attrs = new List<string>();
        foreach (var a in item.attributes) attrs.Add(a.id);

        foreach (var reaction in reactions)
        {
            bool match = true;
            foreach (var req in reaction.requiredAttributes)
            {
                if (!attrs.Contains(req)) { match = false; break; }
            }
            if (match)
            {
                Execute(reaction);
                return;
            }
        }
        Debug.Log("何も起こらない");
    }

    void Execute(WorldAttributeReaction reaction)
    {
        Debug.Log(objectName + " → " + reaction.reactionName);

        if (!string.IsNullOrEmpty(reaction.message))
            Debug.Log(reaction.message);

        switch (reaction.reactionType)
        {
            case "break": Destroy(gameObject); break;
            case "disable": gameObject.SetActive(false); break;
            case "open": Debug.Log("開いた"); break;
            case "activate": Debug.Log("起動した"); break;
        }
    }
}
