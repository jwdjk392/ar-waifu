using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string originName;
    public string description;
    [TextArea(10, 100)]
    public string systemPrompt;
    public GameObject model;
}
