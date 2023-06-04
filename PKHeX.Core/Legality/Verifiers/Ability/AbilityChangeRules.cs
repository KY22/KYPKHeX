using System;
using static PKHeX.Core.AbilityPermission;

namespace PKHeX.Core;

/// <summary>
/// Rules for changing an entity's current ability index.
/// </summary>
public static class AbilityChangeRules
{
    /// <summary>
    /// Checks if the <see cref="abilityFlag"/> value is possible to obtain based on the original <see cref="enc"/> and game visiting.
    /// </summary>
    /// <param name="enc">Original Encounter</param>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="abilityFlag">Current ability index value</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible to obtain <see cref="abilityFlag"/></returns>
    public static bool IsAbilityStateValid(this IEncounterTemplate enc, EvolutionHistory evosAll, int abilityFlag, EntityContext current, EntityContext original) => (enc switch
    {
        IFixedAbilityNumber f => f.Ability,
        _ => Any12,
    }).IsAbilityStateValid(evosAll, abilityFlag, current, original);

    /// <summary>
    /// Checks if the current <see cref="abilityFlag"/> value is possible to obtain based on the original <see cref="ability"/> and game visiting.
    /// </summary>
    /// <param name="ability">Original Ability Permitted</param>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="abilityFlag">Current ability index value</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible to obtain <see cref="abilityFlag"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IsAbilityStateValid(this AbilityPermission ability, EvolutionHistory evosAll, int abilityFlag, EntityContext current, EntityContext original) => ability switch
    {
        Any12H     => true,
        Any12      => abilityFlag != 4 || IsAbilityPatchPossible(evosAll, current, original),
        OnlyHidden => abilityFlag == 4 || IsAbilityPatchRevertPossible(evosAll, abilityFlag, current, original),
        OnlyFirst  => abilityFlag == 1 || (abilityFlag == 4 && IsAbilityPatchPossible(evosAll, current, original)) || (abilityFlag != 4 && IsAbilityCapsulePossible(evosAll, current, original)),
        OnlySecond => abilityFlag == 2 || (abilityFlag == 4 && IsAbilityPatchPossible(evosAll, current, original)) || (abilityFlag != 4 && IsAbilityCapsulePossible(evosAll, current, original)),
        _ => throw new ArgumentOutOfRangeException(nameof(ability), ability, null),
    };

    /// <summary>
    /// Checks if the original <see cref="enc"/> ability value can be changed based on the games visited.
    /// </summary>
    /// <param name="enc">Original Encounter</param>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if the ability can be changed</returns>
    public static bool IsAbilityChangeAvailable(this IEncounterTemplate enc, EvolutionHistory evosAll, EntityContext current, EntityContext original) => (enc switch
    {
        IFixedAbilityNumber f => f.Ability,
        _ => Any12,
    }).IsAbilityChangeAvailable(evosAll, current, original);

    /// <summary>
    /// Checks if the original <see cref="ability"/> value can be changed based on the games visited.
    /// </summary>
    /// <param name="ability">Original Ability Permitted</param>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if the ability can be changed</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IsAbilityChangeAvailable(this AbilityPermission ability, EvolutionHistory evosAll, EntityContext current, EntityContext original) => ability switch
    {
        Any12H => true,
        Any12 => IsAbilityPatchAvailable(evosAll, current, original),
        OnlyHidden => IsAbilityPatchRevertAvailable(evosAll, current, original),
        OnlyFirst or OnlySecond => IsAbilityPatchAvailable(evosAll, current, original) || IsAbilityCapsuleAvailable(evosAll, current, original),
        _ => throw new ArgumentOutOfRangeException(nameof(ability), ability, null),
    };

    /// <summary>
    /// Checks if the Ability Capsule (1 &lt;-&gt; 2) item is available in any game visited.
    /// </summary>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible</returns>
    public static bool IsAbilityCapsuleAvailable(EvolutionHistory evosAll, EntityContext current, EntityContext original)
    {
        if (evosAll.HasVisitedSWSH)
            return true;
        if (evosAll.HasVisitedBDSP)
            return true;
        if (evosAll.HasVisitedGen7)
            return true;
        if (evosAll.HasVisitedGen6)
            return true;
        if (IsAbilityRestoredHOME(current, original))
            return false;
        if (evosAll.HasVisitedGen9)
            return true;
        return false;
    }

    /// <summary>
    /// Checks if any of the games visited allow applying an Ability Capsule (1 &lt;-&gt; 2) item.
    /// </summary>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible</returns>
    public static bool IsAbilityCapsulePossible(EvolutionHistory evosAll, EntityContext current, EntityContext original)
    {
        if (evosAll.HasVisitedSWSH && IsCapsulePossible<PersonalTable8SWSH, PersonalInfo8SWSH, EvoCriteria>(evosAll.Gen8, PersonalTable.SWSH))
            return true;
        if (evosAll.HasVisitedBDSP && IsCapsulePossible<PersonalTable8BDSP, PersonalInfo8BDSP, EvoCriteria>(evosAll.Gen8b, PersonalTable.BDSP))
            return true;
        if (evosAll.HasVisitedGen7 && IsCapsulePossible<PersonalTable7, PersonalInfo7, EvoCriteria>(evosAll.Gen7, PersonalTable.USUM))
            return true;
        if (evosAll.HasVisitedGen6 && IsCapsulePossible<PersonalTable6AO, PersonalInfo6AO, EvoCriteria>(evosAll.Gen6, PersonalTable.AO))
            return true;
        if (IsAbilityRestoredHOME(current, original))
            return false;
        if (evosAll.HasVisitedGen9 && IsCapsulePossible<PersonalTable9SV, PersonalInfo9SV, EvoCriteria>(evosAll.Gen9, PersonalTable.SV))
            return true;
        return false;
    }

    /// <summary>
    /// Checks if the Ability Patch (1/2-&gt; H) item is available in any game visited.
    /// </summary>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible</returns>
    public static bool IsAbilityPatchAvailable(EvolutionHistory evosAll, EntityContext current, EntityContext original)
    {
        if (evosAll.HasVisitedSWSH || evosAll.HasVisitedBDSP)
            return true;
        if (IsAbilityRestoredHOME(current, original))
            return false;
        if (evosAll.HasVisitedGen9)
            return true;
        return false;
    }

    /// <summary>
    /// Checks if any of the games visited allow applying an Ability Patch (1/2-&gt; H) item.
    /// </summary>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible</returns>
    public static bool IsAbilityPatchPossible(EvolutionHistory evosAll, EntityContext current, EntityContext original)
    {
        if (evosAll.HasVisitedSWSH && IsPatchPossible<PersonalTable8SWSH, PersonalInfo8SWSH, EvoCriteria>(evosAll.Gen8, PersonalTable.SWSH))
            return true;
        if (evosAll.HasVisitedBDSP && IsPatchPossible<PersonalTable8BDSP, PersonalInfo8BDSP, EvoCriteria>(evosAll.Gen8b, PersonalTable.BDSP))
            return true;
        if (IsAbilityRestoredHOME(current, original))
            return false;
        if (evosAll.HasVisitedGen9 && IsPatchPossible<PersonalTable9SV, PersonalInfo9SV, EvoCriteria>(evosAll.Gen9, PersonalTable.SV))
            return true;
        return false;
    }

    /// <summary>
    /// Checks if any of the games visited allow reverting an Ability Patch (1/2-&gt; H) item.
    /// </summary>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible</returns>
    public static bool IsAbilityPatchRevertAvailable(EvolutionHistory evosAll, EntityContext current, EntityContext original)
    {
        if (IsAbilityRestoredHOME(current, original))
            return false;

        if (evosAll.HasVisitedGen9)
            return true;
        return false;
    }

    private static bool IsAbilityRestoredHOME(EntityContext current, EntityContext original)
    {
        var originalGen = original.Generation();
        if (originalGen < 9)
        {
            // HOME restores abilities if it existed in the current format prior to transferring to a future game with reversion possible.
            if (originalGen != 8)
                original = EntityContext.Gen8;
            if (current == original)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if any of the games visited allow reverting an Ability Patch (1/2-&gt; H) item.
    /// </summary>
    /// <param name="evosAll">Evolution and game visitation</param>
    /// <param name="abilityIndex">Current ability index value</param>
    /// <param name="current">Current context</param>
    /// <param name="original">Original context</param>
    /// <returns>True if possible</returns>
    public static bool IsAbilityPatchRevertPossible(EvolutionHistory evosAll, int abilityIndex, EntityContext current, EntityContext original)
    {
        // With HOME 3.0, ability modifications are locked to sets of games -- modifying in SV won't change SWSH.
        if (current == EntityContext.Gen9)
        {
            if (IsAbilityRestoredHOME(current, original))
                return false;
            if (evosAll.HasVisitedGen9 && IsRevertPossible<PersonalTable9SV, PersonalInfo9SV, EvoCriteria>(evosAll.Gen9, PersonalTable.SV, abilityIndex))
                return true;
        }
        return false;
    }

    private static bool IsCapsulePossible<TTable, TInfo, TDex>(ReadOnlySpan<TDex> evos, TTable table)
        where TTable : IPersonalTable<TInfo>
        where TInfo : class, IPersonalInfo, IPersonalAbility12
        where TDex : ISpeciesForm
    {
        for (int i = evos.Length - 1; i >= 0; i--)
        {
            var evo = evos[i];
            var pi = table[evo.Species, evo.Form];
            if (!pi.GetIsAbility12Same())
                return true;
        }
        return false;
    }

    private static bool IsPatchPossible<TTable, TInfo, TDex>(ReadOnlySpan<TDex> evos, TTable table)
        where TTable : IPersonalTable<TInfo>
        where TInfo : class, IPersonalInfo, IPersonalAbility12H
        where TDex : ISpeciesForm
    {
        for (int i = evos.Length - 1; i >= 0; i--)
        {
            var evo = evos[i];
            var pi = table[evo.Species, evo.Form];
            if (pi.GetIsAbilityHiddenUnique())
                return true;
        }

        // Some species have a distinct hidden ability only on another form, and can change between that form and its current form.
        var first = evos[0];
        return first.Form != 0 && first.Species switch
        {
            (int)Species.Giratina => true, // Form-0 is a/a/h
            (int)Species.Tornadus => true, // Form-0 is a/a/h
            (int)Species.Thundurus => true, // Form-0 is a/a/h
            (int)Species.Landorus => true, // Form-0 is a/a/h
            (int)Species.Enamorus => true, // Form-0 is a/a/h
            _ => false,
        };
    }

    private static bool IsRevertPossible<TTable, TInfo, TDex>(ReadOnlySpan<TDex> evos, TTable table, int abilityIndex)
        where TTable : IPersonalTable<TInfo>
        where TInfo : class, IPersonalInfo, IPersonalAbility12H
        where TDex : ISpeciesForm
    {
        bool revert = false;
        for (var i = evos.Length - 1; i >= 0; i--)
        {
            var evo = evos[i];
            var pi = table[evo.Species, evo.Form];
            if (revert && !pi.GetIsAbility12Same())
                return true;
            if (!pi.GetIsAbilityHiddenUnique())
                continue;
            if (abilityIndex == 1 || !pi.GetIsAbility12Same())
                return true;
            revert = true;
        }
        return false;
    }
}
