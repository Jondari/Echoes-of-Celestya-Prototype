using UnityEngine;
using System.Collections.Generic;
using SerializeReferenceEditor;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Description {  get; private set; }
    [field: SerializeField] public int Mana {  get; private set; }
    [field: SerializeField] public Sprite Image {  get; private set; }
    // each card can have one where you pick a target and can also have multiple other effects where the target is selected automatically
    [field: SerializeReference, SR] public Effect ManualTargetEffect { get; private set; } = null;
    [field: SerializeField] public List<AutoTargetEffect> OtherEffects { get; private set; }
}
