using System.Collections.Generic;
using UnityEngine;
//ƒAƒCƒeƒ€‚É‚æ‚é”½‰ž(”j‰ó‚È‚Ç)

[System.Serializable]
public class WorldAttributeReaction
{
    public string reactionName;
    public string reactionType;
    public List<string> requiredAttributes = new List<string>();
    [TextArea] public string message;
}
