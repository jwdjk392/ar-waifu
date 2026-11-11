using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] ARModelManager aRModelManager;
    [SerializeField] AIManager aIManager;
    [SerializeField] CharacterData characterData;
    void Start()
    {
        PlaceModel();
        aIManager.SetupCharacter(characterData);
    }


    public void PlaceModel()
    {
        Debug.Log("Place Model");
        aRModelManager.PlaceModel(characterData);
    }
}
