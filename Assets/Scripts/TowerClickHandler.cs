using UnityEngine;

public class TowerClickHandler : MonoBehaviour
{
    public Tower tower;

    void OnMouseDown()
    {
        if (tower != null)
        {
            GameManager.Instance.OnTowerClicked(tower);
        }
    }

    void OnMouseEnter()
    {
        if (tower != null) tower.SetRangeVisible(true);
    }

    void OnMouseExit()
    {
        // Keep visible if selected, hide otherwise
        if (tower != null && GameManager.Instance.SelectedTower != tower)
            tower.SetRangeVisible(false);
    }
}
