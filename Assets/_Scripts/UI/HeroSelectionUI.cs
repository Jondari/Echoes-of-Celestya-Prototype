using UnityEngine;

public class HeroSelectionUI : MonoBehaviour
{
    [SerializeField] private HeroData barkyn;
    [SerializeField] private HeroData cryil;
    [SerializeField] private HeroData draleth;

    public void SelectBarkyn()
    {
        SelectHero(barkyn);
    }

    public void SelectCryil()
    {
        SelectHero(cryil);
    }

    public void SelectDraleth()
    {
        SelectHero(draleth);
    }

    private void SelectHero(HeroData hero)
    {
        GameSession.Instance.SelectHero(hero);
    }
}
