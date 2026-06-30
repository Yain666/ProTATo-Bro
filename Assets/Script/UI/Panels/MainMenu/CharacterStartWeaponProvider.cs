using System.Collections.Generic;

public static class CharacterStartWeaponProvider
{
    public static IReadOnlyList<int> GetStartingWeaponIds(CharacterData character)
    {
        if (character != null && character.startingWeaponIds != null && character.startingWeaponIds.Length > 0)
        {
            return character.startingWeaponIds;
        }

        return new[] { 201 };
    }
}
