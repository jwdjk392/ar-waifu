using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] ARTapToPlace aRTapToPlace;
    [SerializeField] CharacterData characterData;
    void Start()
    {
        aRTapToPlace.placeModel(characterData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
