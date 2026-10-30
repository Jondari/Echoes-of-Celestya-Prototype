using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy Intent")]
public class EnemyIntentData : ScriptableObject
{
    [field: SerializeField]
    public EnemyIntentType Type { get; private set; }

    [field: SerializeField]
    public Sprite Icon { get; private set; }
}
