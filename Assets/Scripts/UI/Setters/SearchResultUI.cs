using Search.Core;
using Search.Levels;
using Search.Utils;

public class SearchResultUI : Singleton<SearchResultUI>
{
    private void OnEnable()
    {
        LevelManager.Instance.onSearchLoaded.AddListener(DisableUI);
    }

    private void OnDisable()
    {
        LevelManager.Instance.onSearchLoaded.RemoveListener(DisableUI);
    }

    public void DisplayResult(SearchResult result)
    {
        UpdateUI(result);
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void UpdateUI(SearchResult result)
    {
        
    }
    
    private void DisableUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
