using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct OwnedWeapon { public int id; public int grade; public OwnedWeapon(int id, int grade) { this.id = id; this.grade = grade; } }

public class WeaponInventory
{
    private readonly List<OwnedWeapon> _owned = new List<OwnedWeapon>();
    private const int MaxSlots = 6;

    public IReadOnlyList<OwnedWeapon> Owned => _owned;
    public IEnumerable<int> OwnedWeaponIds => _owned.Select(w => w.id);
    public int SlotCount => _owned.Count;
    public bool IsFull => _owned.Count >= MaxSlots;

    public bool HasWeapon(int id) => _owned.Exists(w => w.id == id);

    public bool CanAccept(int id, int grade)
    {
        if (!IsFull) return true;
        return grade < WeaponGrade.Mythic && _owned.Exists(w => w.id == id && w.grade == grade);
    }

    public bool AddWeapon(int id, int grade)
    {
        grade = WeaponGrade.Clamp(grade);
        if (!CanAccept(id, grade)) return false;
        _owned.Add(new OwnedWeapon(id, grade));
        ResolveMerges();
        return true;
    }

    private void ResolveMerges()
    {
        bool merged = true;
        while (merged)
        {
            merged = false;
            for (int i = 0; i < _owned.Count && !merged; i++)
            {
                if (_owned[i].grade >= WeaponGrade.Mythic) continue;
                for (int j = i + 1; j < _owned.Count; j++)
                {
                    if (_owned[j].id != _owned[i].id || _owned[j].grade != _owned[i].grade) continue;
                    int id = _owned[i].id;
                    int upgraded = _owned[i].grade + 1;
                    _owned.RemoveAt(j);
                    _owned.RemoveAt(i);
                    _owned.Add(new OwnedWeapon(id, upgraded));
                    Debug.Log($"<color=cyan>[合体进化] 武器 {id} 升到品阶 {upgraded}</color>");
                    merged = true;
                    break;
                }
            }
        }
    }
}
