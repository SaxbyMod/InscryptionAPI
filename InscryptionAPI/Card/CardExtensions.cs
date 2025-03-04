using DiskCardGame;
using GBC;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Microsoft.Win32.SafeHandles;

namespace InscryptionAPI.Card;

public static class CardExtensions
{
    /// <summary>
    /// Gets the first card matching the given name, or null if it does not exist
    /// </summary>
    /// <param name="cards">An enumeration of Inscryption cards</param>
    /// <param name="name">The name to search for (case sensitive).</param>
    /// <returns>The first matching card, or null if no match.</returns>
    public static CardInfo CardByName(this IEnumerable<CardInfo> cards, string name) => cards.FirstOrDefault(c => c.name.Equals(name));

    private static Sprite GetPortrait(Texture2D portrait, TextureHelper.SpriteType spriteType, FilterMode? filterMode = null)
    {
        return portrait.ConvertTexture(spriteType, filterMode ?? FilterMode.Point);
    }

    #region Adders

    /// <summary>
    /// Adds any number of abilities to the card. Abilities can be added multiple times.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="abilities">The abilities to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddAbilities(this CardInfo info, params Ability[] abilities)
    {
        info.abilities ??= new();
        info.abilities.AddRange(abilities);
        return info;
    }

    /// <summary>
    /// Adds any number of appearance behaviors to the card. Duplicate appearance behaviors are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="appearances">The appearances to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddAppearances(this CardInfo info, params CardAppearanceBehaviour.Appearance[] appearances)
    {
        info.appearanceBehaviour ??= new();
        foreach (var app in appearances)
            if (!info.appearanceBehaviour.Contains(app))
                info.appearanceBehaviour.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of decals to the card. Duplicate decals are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="decals">The decals to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddDecal(this CardInfo info, params Texture[] decals)
    {
        info.decals ??= new();
        foreach (var dec in decals)
            if (!info.decals.Contains(dec))
                info.decals.Add(dec);

        return info;
    }

    /// <summary>
    /// Adds any number of decals to the card. Duplicate decals are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="decals">The paths to the .png files containing the decals (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddDecal(this CardInfo info, params string[] decals)
    {
        return decals == null
            ? info
            : info.AddDecal(decals.Select(d => TextureHelper.GetImageAsTexture(d)).ToArray());

    }

    /// <summary>
    /// Adds any number of metacategories to the card. Duplicate metacategories are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="categories">The categories to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddMetaCategories(this CardInfo info, params CardMetaCategory[] categories)
    {
        info.metaCategories ??= new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of special abilities to the card. Duplicate special abilities are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="abilities">The abilities to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        info.specialAbilities ??= new();
        foreach (var app in abilities)
            if (!info.specialAbilities.Contains(app))
                info.specialAbilities.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of traits to the card. Duplicate traits are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="traits">The traits to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddTraits(this CardInfo info, params Trait[] traits)
    {
        info.traits ??= new();
        foreach (var app in traits)
            if (!info.traits.Contains(app))
                info.traits.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of tribes to the card. Duplicate tribes are ignored.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tribes">The tribes to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo AddTribes(this CardInfo info, params Tribe[] tribes)
    {
        info.tribes ??= new();
        foreach (var app in tribes)
            if (!info.tribes.Contains(app))
                info.tribes.Add(app);
        return info;
    }

    #endregion

    #region Removers

    /// <summary>
    /// Removes all stacks of any number of abilities from the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="abilities">The abilities to remove</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo RemoveAbilities(this CardInfo info, params Ability[] abilities)
    {
        if (info.abilities?.Count > 0)
        {
            foreach (Ability ab in abilities)
            {
                info.abilities.RemoveAll(a => a == ab);
            }
        }
        return info;
    }
    /// <summary>
    /// Removes any number of abilities from the card. Will remove one instance of each passed ability; multiple instances can be passed.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="abilities">The abilities to remove</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo RemoveAbilitiesSingle(this CardInfo info, params Ability[] abilities)
    {
        if (info.abilities?.Count > 0)
        {
            foreach (Ability ab in abilities)
            {
                info.abilities.Remove(ab);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of special abilities from the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="abilities">The special abilities to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        if (info.specialAbilities?.Count > 0)
        {
            foreach (var ab in abilities)
            {
                info.specialAbilities.RemoveAll(a => a == ab);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of special abilities from the card. Will remove one instance of each passed ability; multiple instances can be passed.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="abilities">The special abilities to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveSpecialAbilitiesSingle(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        if (info.specialAbilities?.Count > 0)
        {
            foreach (var ab in abilities)
            {
                info.specialAbilities.Remove(ab);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of appearance behaviors from the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="appearances">The appearances to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveAppearances(this CardInfo info, params CardAppearanceBehaviour.Appearance[] appearances)
    {
        if (info.appearanceBehaviour?.Count > 0)
        {
            foreach (var app in appearances)
            {
                info.appearanceBehaviour.RemoveAll(a => a == app);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of appearance behaviors from the card. Will remove one instance of each passed appearance; multiple instances can be passed.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="appearances">The appearances to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveAppearancesSingle(this CardInfo info, params CardAppearanceBehaviour.Appearance[] appearances)
    {
        if (info.appearanceBehaviour?.Count > 0)
        {
            foreach (var app in appearances)
            {
                info.appearanceBehaviour.Remove(app);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of decals from the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="decals">The decals to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveDecals(this CardInfo info, params Texture[] decals)
    {
        if (info.decals?.Count > 0)
        {
            foreach (var dec in decals)
            {
                info.decals.RemoveAll(d => d == dec);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of decals behaviors from the card. Will remove one instance of each passed decals; multiple instances can be passed.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="decals">The decals to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveDecalsSingle(this CardInfo info, params Texture[] decals)
    {
        if (info.decals?.Count > 0)
        {
            foreach (var dec in decals)
            {
                info.decals.Remove(dec);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of metacategories from the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="categories">The categories to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveMetaCategories(this CardInfo info, params CardMetaCategory[] categories)
    {
        if (info.metaCategories?.Count > 0)
        {
            foreach (var cat in categories)
            {
                info.metaCategories.RemoveAll(c => c == cat);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of metacategories behaviors from the card. Will remove one instance of each passed appearance; multiple instances can be passed.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="categories">The categories to remove</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo RemoveMetaCategoriesSingle(this CardInfo info, params CardMetaCategory[] categories)
    {
        if (info.metaCategories?.Count > 0)
        {
            foreach (var cat in categories)
            {
                info.metaCategories.Remove(cat);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of traits from the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="traits">The traits to remove</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo RemoveTraits(this CardInfo info, params Trait[] traits)
    {
        if (info.traits?.Count > 0)
        {
            foreach (Trait tr in traits)
            {
                if (info.HasTrait(tr))
                    info.traits.Remove(tr);
            }
        }
        return info;
    }
    
    /// <summary>
    /// Removes any number of traits from the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tribes">The tribes to remove</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo RemoveTribes(this CardInfo info, params Tribe[] tribes)
    {
        if (info.traits?.Count > 0)
        {
            foreach (Tribe tr in tribes)
            {
                if (info.IsOfTribe(tr))
                    info.tribes.Remove(tr);
            }
        }
        return info;
    }

    /// <summary>
    /// Removes any number of CardMetaCategories from the card.
    /// </summary>
    /// <param name="info">Card to access.</param>
    /// <param name="cardMetaCategories">The CardMetaCategories to remove.</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo RemoveCardMetaCategories(this CardInfo info, params CardMetaCategory[] cardMetaCategories)
    {
        if (info.metaCategories?.Count > 0)
        {
            foreach (CardMetaCategory cm in cardMetaCategories)
            {
                if (info.HasCardMetaCategory(cm))
                    info.metaCategories.Remove(cm);
            }
        }
        return info;
    }

    #endregion

    #region Setters

    /// <summary>
    /// Sets a number of basic properties of the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="displayedName">Displayed name of the card</param>
    /// <param name="attack">Attack of the card</param>
    /// <param name="health">Health of the card</param>
    /// <param name="description">The description that plays when the card is seen for the first time.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBasic(this CardInfo info, string displayedName, int attack, int health, string description = default)
    {
        info.displayedName = displayedName;
        info.baseAttack = attack;
        info.baseHealth = health;
        info.description = description;
        return info;
    }

    /// <summary>
    /// Sets an indicator of whether this is a base game card or not.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="isBaseGameCard">Whether this is a base game card or not</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    internal static CardInfo SetBaseGameCard(this CardInfo info, bool isBaseGameCard = true)
    {
        info.SetExtendedProperty("BaseGameCard", isBaseGameCard);
        return info;
    }

    /// <summary>
    /// Indicates if this is a base game card or not
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>True of this card came from the base game; false otherwise.</returns>
    public static bool IsBaseGameCard(this CardInfo info)
    {
        bool? isBGC = info.GetExtendedPropertyAsBool("BaseGameCard");
        return isBGC.HasValue && isBGC.Value;
    }

    #region Fields

    /// <summary>
    /// Sets the base attack and health of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="baseAttack">The base attack for the card.</param>
    /// <param name="baseHealth">The base health for the card.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBaseAttackAndHealth(this CardInfo info, int? baseAttack = 0, int? baseHealth = 0)
    {
        if (baseAttack.HasValue)
            info.baseAttack = baseAttack.Value;

        if (baseHealth.HasValue)
            info.baseHealth = baseHealth.Value;

        return info;
    }

    /// <summary>
    /// Sets the Card Temple for the card. Used in Act 2 and in Act 1 for certain card choice nodes.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="temple">The Card Temple to use.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCardTemple(this CardInfo info, CardTemple temple)
    {
        info.temple = temple;
        return info;
    }

    /// <summary>
    /// Sets the displayed name of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="displayedName">The displayed name for the card</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetDisplayedName(this CardInfo info, string displayedName)
    {
        info.displayedName = displayedName.IsNullOrWhitespace() ? "" : displayedName;
        return info;
    }

    /// <summary>
    /// Sets the name of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="name">The name for the card</param>
    /// <param name="modPrefix">The string that will be prefixed to the card name if it doesn't already exist.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetName(this CardInfo info, string name, string modPrefix = default)
    {
        info.name = !string.IsNullOrEmpty(modPrefix) && !name.StartsWith(modPrefix) ? $"{modPrefix}_{name}" : name;
        return info;
    }

    /// <summary>
    /// Sets the card name and displayed name of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="name">The name for the card</param>
    /// <param name="displayedName">The displayed name for the card</param>
    /// <param name="modPrefix">The string that will be prefixed to the card name if it doesn't already exist.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetNames(this CardInfo info, string name, string displayedName, string modPrefix = default(string))
    {
        info.SetDisplayedName(displayedName);
        return info.SetName(name, modPrefix);
    }

    /// <summary>
    /// Sets any number of special abilities to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="abilities">The abilities to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        info.specialAbilities = abilities?.ToList();
        return info;
    }

    /// <summary>
    /// Sets the stat icon to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="statIcon">The stat icon to set</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetStatIcon(this CardInfo info, SpecialStatIcon statIcon)
    {
        info.specialStatIcon = statIcon;
        info.AddSpecialAbilities(StatIconManager.AllStatIcons.FirstOrDefault(sii => sii.Id == statIcon).AbilityId);
        return info;
    }

    /// <summary>
    /// Sets any number of traits to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="traits">The traits to add</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTraits(this CardInfo info, params Trait[] traits)
    {
        info.traits = traits?.ToList();
        return info;
    }


    /// <summary>
    /// Set any number of tribes to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tribes">The tribes to add.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTribes(this CardInfo info, params Tribe[] tribes)
    {
        info.tribes = tribes?.ToList();
        return info;
    }

    /// <summary>
    /// Sets whether the card should be Gemified or not. Can and will un-Gemify cards.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="gemify">Whether the card should be gemified.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGemify(this CardInfo info, bool gemify = true)
    {
        if (gemify && !info.Mods.Exists(x => x.gemify))
            info.Mods.Add(new() { gemify = true });
        else if (!gemify)
            info.Mods.FindAll(x => x.gemify).ForEach(y => y.gemify = false);

        return info;
    }

    #region MetaCategories

    /// <summary>
    /// Sets the card to behave as a "normal" card in Part 1. The CardTemple is Nature and it will appear in choice nodes and trader nodes.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetDefaultPart1Card(this CardInfo info)
    {
        if (!info.metaCategories.Contains(CardMetaCategory.Rare))
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer);
            info.cardComplexity = CardComplexity.Simple;
        }
        info.temple = CardTemple.Nature;
        return info;
    }

    /// <summary>
    /// Sets the card to behave as a "normal" card in Part 3. The CardTemple is Tech and it will appear in choice nodes and as a potential random card from GiftBot.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetDefaultPart3Card(this CardInfo info)
    {
        if (!info.metaCategories.Contains(CardMetaCategory.Rare))
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.Part3Random);
            info.cardComplexity = CardComplexity.Simple;
        }
        info.temple = CardTemple.Tech;
        return info;
    }

    /// <summary>
    /// Makes the card fully playable in GBC mode and able to appear in card packs.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="temple">The temple that the card will exist under.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGBCPlayable(this CardInfo info, CardTemple temple)
    {
        info.AddMetaCategories(CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable);
        info.temple = temple;
        return info;
    }


    /// <summary>
    /// Sets the card so it shows up for rare card choices and applies the rare background.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetRare(this CardInfo info)
    {
        info.AddMetaCategories(CardMetaCategory.Rare);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.RareCardBackground);

        info.metaCategories.Remove(CardMetaCategory.ChoiceNode);
        info.metaCategories.Remove(CardMetaCategory.TraderOffer);
        info.metaCategories.Remove(CardMetaCategory.Part3Random);

        info.cardComplexity = CardComplexity.Intermediate;

        return info;
    }


    /// <summary>
    /// Adds the terrain trait and background to this card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The same card info so a chain can continue</returns>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTerrain(this CardInfo info)
    {
        info.AddTraits(Trait.Terrain);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);
        return info;
    }
    /// <summary>
    /// Adds the Terrain trait and background to this card, with the option to not use TerrainLayout.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTerrain(this CardInfo info, bool useTerrainLayout)
    {
        info.AddTraits(Trait.Terrain);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground);
        if (useTerrainLayout)
            info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout);
        return info;
    }

    /// <summary>
    /// Adds the Pelt trait and background to this card, and optionally adds the SpawnLice special ability.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPelt(this CardInfo info, bool spawnLice = true)
    {
        info.AddTraits(Trait.Pelt);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);
        if (spawnLice)
            info.AddSpecialAbilities(SpecialTriggeredAbility.SpawnLice);
        return info;
    }

    #endregion

    #region EvolveIceCubeTail

    /// <summary>
    /// Sets the evolve parameters of the card. These parameters are used to make the Evolve ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="evolveCard">The card that will be generated after the set number of turns.</param>
    /// <param name="numberOfTurns">The number of turns before the card evolves</param>
    /// <param name="mods">A set of card mods to be applied to the evolved card</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEvolve(this CardInfo info, CardInfo evolveCard, int numberOfTurns, IEnumerable<CardModificationInfo> mods = null)
    {
        info.evolveParams = new()
        {
            evolution = evolveCard
        };

        if (mods != null && mods.Any())
        {
            info.evolveParams.evolution = CardLoader.Clone(info.evolveParams.evolution);
            (info.evolveParams.evolution.mods ??= new()).AddRange(mods);
        }

        info.evolveParams.turnsToEvolve = numberOfTurns;

        return info;
    }

    /// <summary>
    /// Sets the evolve parameters of the card. These parameters are used to make the Evolve ability function correctly.
    /// This function uses delayed loading to attach the evolution to the card, so if the evolve card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="evolveInto">The name of card that will be generated after the set number of turns.</param>
    /// <param name="numberOfTurns">The number of turns before the card evolves</param>
    /// <param name="mods">A set of card mods to be applied to the evolved card</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEvolve(this CardInfo info, string evolveInto, int numberOfTurns, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo evolution = CardManager.AllCardsCopy.CardByName(evolveInto);

        if (evolution == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                CardInfo target = cards.CardByName(info.name);
                CardInfo evolveIntoCard = cards.CardByName(evolveInto);

                if (target != null && evolveIntoCard == null && target.IsOldApiCard()) // Maybe this is due to poor naming conventions allowed by the old API.
                    evolveIntoCard = cards.CardByName($"{target.GetModPrefix()}_{evolveInto}");

                if (target != null && evolveIntoCard != null)
                    target.SetEvolve(evolveIntoCard, numberOfTurns, mods);

                return cards;
            };
        }
        else
        {
            info.SetEvolve(evolution, numberOfTurns, mods);
        }
        return info;
    }

    /// <summary>
    /// Sets the default evolution name for the card. This is the name used when the card doesn't evolve into another card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="defaultName">The default evolution name to use. Pass in 'null' to use the vanilla default.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetDefaultEvolutionName(this CardInfo info, string defaultName)
    {
        info.defaultEvolutionName = defaultName;
        return info;
    }

    /// <summary>
    /// Sets the ice cube parameters of the card. These parameters are used to make the IceCube ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="iceCube">The card that will be generated when this card dies.</param>
    /// <param name="mods">A set of card mods to be applied to the ice cube contents</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetIceCube(this CardInfo info, CardInfo iceCube, IEnumerable<CardModificationInfo> mods = null)
    {
        info.iceCubeParams = new()
        {
            creatureWithin = iceCube
        };

        if (mods != null && mods.Any())
        {
            info.iceCubeParams.creatureWithin = CardLoader.Clone(info.iceCubeParams.creatureWithin);
            (info.iceCubeParams.creatureWithin.mods ??= new()).AddRange(mods);
        }

        return info;
    }

    /// <summary>
    /// Sets the ice cube parameters of the card. These parameters are used to make the IceCube ability function correctly.
    /// This function uses delayed loading to attach the ice cube to the card, so if the ice cube card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="iceCubeName">The name of the card that will be generated when this card dies.</param>
    /// <param name="mods">A set of card mods to be applied to the ice cube contents</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetIceCube(this CardInfo info, string iceCubeName, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo creatureWithin = CardManager.AllCardsCopy.CardByName(iceCubeName);

        if (creatureWithin == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                CardInfo target = cards.CardByName(info.name);
                CardInfo creatureWithinCard = cards.CardByName(iceCubeName);

                if (target != null && creatureWithinCard == null && target.IsOldApiCard()) // Maybe this is due to poor naming conventions allowed by the old API.
                    creatureWithinCard = cards.CardByName($"{target.GetModPrefix()}_{iceCubeName}");

                if (target != null && creatureWithinCard != null)
                    target.SetIceCube(creatureWithinCard, mods);

                return cards;
            };
        }
        else
        {
            info.SetIceCube(creatureWithin, mods);
        }

        return info;
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="pathToLostTailArt">The path to the .png file containing the lost tail artwork (relative to the Plugins directory)</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, string pathToLostTailArt = null, IEnumerable<CardModificationInfo> mods = null)
    {
        Texture2D lostTailPortrait = pathToLostTailArt == null ? null : TextureHelper.GetImageAsTexture(pathToLostTailArt);
        return info.SetTail(tailName, lostTailPortrait, mods: mods);
    }


    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, IEnumerable<CardModificationInfo> mods = null)
    {
        return info.SetTail(tailName, tailLostPortrait: null, mods: mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, IEnumerable<CardModificationInfo> mods = null)
    {
        return info.SetTail(tail, null, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, Texture2D tailLostPortrait, FilterMode? filterMode = null, IEnumerable<CardModificationInfo> mods = null)
    {
        var tailLostSprite = !filterMode.HasValue
                                 ? tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
                                 : tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);
        return info.SetTail(tail, tailLostSprite, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The sprite containing the card portrait</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, Sprite tailLostPortrait, IEnumerable<CardModificationInfo> mods = null)
    {
        info.tailParams = new()
        {
            tail = tail
        };

        if (mods != null && mods.Any())
        {
            info.tailParams.tail = CardLoader.Clone(info.tailParams.tail);
            (info.tailParams.tail.mods ??= new()).AddRange(mods);
        }

        if (tailLostPortrait != null)
            info.tailParams.SetLostTailPortrait(tailLostPortrait, info);

        return info;
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, Texture2D tailLostPortrait, FilterMode? filterMode = null, IEnumerable<CardModificationInfo> mods = null)
    {
        var tailLostSprite = !filterMode.HasValue
            ? tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
            : tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);
        return info.SetTail(tailName, tailLostSprite, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The sprite containing the card portrait.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue..</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, Sprite tailLostPortrait, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo tail = CardManager.AllCardsCopy.CardByName(tailName);

        if (tail == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                CardInfo target = cards.CardByName(info.name);
                CardInfo tailCard = cards.CardByName(tailName);

                if (target != null && tailCard == null && target.IsOldApiCard()) // Maybe this is due to poor naming conventions allowed by the old API.
                    tailCard = cards.CardByName($"{target.GetModPrefix()}_{tailName}");

                if (target != null && tailCard != null)
                    target.SetTail(tailCard, tailLostPortrait, mods);

                return cards;
            };
        }
        else
        {
            info.SetTail(tail, tailLostPortrait, mods);
        }

        return info;
    }

    #endregion

    #region CardCosts

    /// <summary>
    /// Sets the cost of the card. Any and all costs can be set this way.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="bloodCost">The cost in blood (sacrifices)</param>
    /// <param name="bonesCost">The cost in bones</param>
    /// <param name="energyCost">The cost in energy</param>
    /// <param name="gemsCost">The cost in gems</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCost(this CardInfo info, int? bloodCost = 0, int? bonesCost = 0, int? energyCost = 0, List<GemType> gemsCost = null)
    {
        info.SetBloodCost(bloodCost);
        info.SetBonesCost(bonesCost);
        info.SetEnergyCost(energyCost);
        return info.SetGemsCost(gemsCost);
    }

    /// <summary>
    /// Sets the blood cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="bloodCost">The cost in blood (sacrifices)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBloodCost(this CardInfo info, int? bloodCost = 0)
    {
        if (bloodCost.HasValue)
            info.cost = bloodCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the bones cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="bonesCost">The cost in bones</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBonesCost(this CardInfo info, int? bonesCost = 0)
    {
        if (bonesCost.HasValue)
            info.bonesCost = bonesCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the energy cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="energyCost">The cost in energy</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEnergyCost(this CardInfo info, int? energyCost = 0)
    {
        if (energyCost.HasValue)
            info.energyCost = energyCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the gems cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="gemsCost">The cost in Mox.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGemsCost(this CardInfo info, List<GemType> gemsCost = null)
    {
        info.gemsCost = gemsCost ?? new();
        return info;
    }
    /// <summary>
    /// Sets the gems cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="gemsCost">The cost in Mox.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGemsCost(this CardInfo info, params GemType[] gemsCost)
    {
        info.gemsCost = gemsCost.ToList();
        return info;
    }

    #endregion

    #region Bools

    /// <summary>
    /// Sets whether the card is onePerDeck or not.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="onePerDeck">Whether this is onePerDeck or not</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetOnePerDeck(this CardInfo info, bool onePerDeck = true)
    {
        info.onePerDeck = onePerDeck;
        return info;
    }
    /// <summary>
    /// Sets whether the card's Power and Health will be displayed or not.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="hideStats">Whether the stats should be hidden or not</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetHideStats(this CardInfo info, bool hideStats = true)
    {
        info.hideAttackAndHealth = hideStats;
        return info;
    }

    #endregion

    #endregion

    #region Portraits

    #region Main

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortrait(this CardInfo info, string pathToArt)
    {
        try
        {
            return info.SetPortrait(TextureHelper.GetImageAsTexture(pathToArt));
        }
        catch (FileNotFoundException fnfe)
        {
            throw new ArgumentException($"Image file not found for card \"{info.name}\"!", fnfe);
        }
    }

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="emission">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>.</returns>
    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="sprite">The sprite containing the card portrait</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortrait(this CardInfo info, Sprite sprite)
    {
        info.portraitTex = sprite;

        if (!string.IsNullOrEmpty(info.name))
            info.portraitTex.name = info.name + "_portrait";

        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <param name="pathToEmission">The path to the .png file containing the emission artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, string pathToArt, string pathToEmission)
    {
        info.SetPortrait(pathToArt);
        info.SetEmissivePortrait(pathToEmission);
        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="emission">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <param name="emission">The sprite containing the emission</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, Sprite portrait, Sprite emission)
    {
        info.SetPortrait(portrait);
        info.SetEmissivePortrait(emission);
        return info;
    }

    #endregion

    #region Alt

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetAltPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, Sprite portrait)
    {
        info.alternatePortrait = portrait;
        if (!string.IsNullOrEmpty(info.name))
            info.alternatePortrait.name = info.name + "_altportrait";

        TextureHelper.TryReuseEmission(info, info.alternatePortrait);

        return info;
    }

    #endregion

    #region Emissive

    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, string pathToArt)
    {
        try
        {
            return info.SetEmissivePortrait(TextureHelper.GetImageAsTexture(pathToArt));
        }
        catch (FileNotFoundException fnfe)
        {
            throw new ArgumentException($"Image file not found for card \"{info.name}\"!", fnfe);
        }
    }

    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissivePortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="sprite">The sprite containing the emission</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, Sprite sprite)
    {
        if (info.portraitTex == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait");

        info.portraitTex.RegisterEmissionForSprite(sprite);

        return info;
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissiveAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissiveAltPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the emission</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, Sprite portrait)
    {
        if (info.alternatePortrait == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait");

        info.alternatePortrait.RegisterEmissionForSprite(portrait);

        return info;
    }

    #endregion

    #region Pixel

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelPortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, Sprite portrait)
    {
        info.pixelPortrait = portrait;

        if (!string.IsNullOrEmpty(info.name))
            info.pixelPortrait.name = info.name + "_pixelportrait";

        return info;
    }

    #endregion

    #region LostTail

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetLostTailPortrait(this CardInfo info, string pathToArt)
    {
        if (info.tailParams == null)
            throw new InvalidOperationException("Cannot set lost tail portrait without tail params being set first");

        info.tailParams.SetLostTailPortrait(pathToArt, info);

        return info;
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetLostTailPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        if (info.tailParams == null)
            throw new InvalidOperationException("Cannot set lost tail portrait without tail params being set first");

        info.tailParams.SetLostTailPortrait(portrait, info, filterMode);

        return info;
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue.</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, string pathToArt, CardInfo owner)
    {
        owner.tailParams = info;
        return info.SetLostTailPortrait(TextureHelper.GetImageAsTexture(pathToArt), owner);
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue.</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, Texture2D portrait, CardInfo owner, FilterMode? filterMode = null)
    {
        var tailSprite = !filterMode.HasValue
                             ? portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
                             : portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);

        return info.SetLostTailPortrait(tailSprite, owner);
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue.</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, Sprite portrait, CardInfo owner)
    {
        info.tailLostPortrait = portrait;

        if (!string.IsNullOrEmpty(owner.name))
            info.tailLostPortrait.name = owner.name + "_tailportrait";

        TextureHelper.TryReuseEmission(owner, info.tailLostPortrait);

        return info;
    }

    #endregion

    #region Extra Alts
        /// <summary>
    /// Sets the card's pixel alternate portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelAlternatePortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelAlternatePortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's pixel alternate portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelAlternatePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelAlternatePortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }

    /// <summary>
    /// Sets the card's pixel alternate portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelAlternatePortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_pixelalternateportrait";

        info.GetAltPortraits().PixelAlternatePortrait = portrait;
        return info;
    }
    #endregion

    #endregion

    /// <summary>
    /// Sets the custom unlock check for the card.
    /// </summary>
    /// <param name="c">The card</param>
    /// <param name="check">The custom unlock check, a func that needs to return true for the card to be unlocked. The bool argument is true when the game is in Kaycee's Mod mode and the int argument is the current Kaycee's Mod challenge level.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCustomUnlockCheck(this CardInfo c, Func<bool, int, bool> check)
    {
        if (check == null && CardManager.CustomCardUnlocks.ContainsKey(c.name))
            CardManager.CustomCardUnlocks.Remove(c.name);
        else
        {
            if (!CardManager.CustomCardUnlocks.ContainsKey(c.name))
                CardManager.CustomCardUnlocks.Add(c.name, check);
            else
                CardManager.CustomCardUnlocks[c.name] = check;
        }
        return c;
    }

    #endregion

    #region Helpers

    #region Ability
    /// <summary>
    /// Checks if the CardInfo does not have a specific Ability.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="ability">The ability to check for.</param>
    /// <returns>true if the ability does not exist.</returns>
    public static bool LacksAbility(this CardInfo cardInfo, Ability ability)
    {
        return !cardInfo.HasAbility(ability);
    }

    /// <summary>
    /// Checks if the CardInfo has all of the given Abilities.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="abilities">The Abilities to check for.</param>
    /// <returns>true if cardInfo has all of the given Abilities.</returns>
    public static bool HasAllAbilities(this CardInfo cardInfo, params Ability[] abilities)
    {
        foreach (Ability ability in abilities)
        {
            if (cardInfo.LacksAbility(ability))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the CardInfo has none of the specified Abilities.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="abilities">The Abilities to check for.</param>
    /// <returns>true if cardInfo has none of the given Abilities.</returns>
    public static bool LacksAllAbilities(this CardInfo cardInfo, params Ability[] abilities)
    {
        foreach (Ability ability in abilities)
        {
            if (cardInfo.HasAbility(ability))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the CardInfo has any of the specified Abilities.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="abilities">The Abilities to check for.</param>
    /// <returns>true if cardInfo has at least one of the given Abilities.</returns>
    public static bool HasAnyOfAbilities(this CardInfo cardInfo, params Ability[] abilities)
    {
        foreach (Ability ability in abilities)
        {
            if (cardInfo.HasAbility(ability))
                return true;
        }
        return false;
    }
    #endregion

    #region SpecialAbility
    /// <summary>
    /// Checks if the CardInfo has a specific SpecialTriggeredAbility.
    ///
    /// A condensed version of `cardInfo.SpecialAbilities.Contains(ability)`.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="ability">The specialTriggeredAbility to check for.</param>
    /// <returns>true if the specialTriggeredAbility does exist.</returns>
    public static bool HasSpecialAbility(this CardInfo cardInfo, SpecialTriggeredAbility ability)
    {
        return cardInfo.SpecialAbilities.Contains(ability);
    }

    /// <summary>
    /// Checks if the CardInfo does not have a specific SpecialTriggeredAbility.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="ability">The specialTriggeredAbility to check for.</param>
    /// <returns>true if the specialTriggeredAbility does not exist.</returns>
    public static bool LacksSpecialAbility(this CardInfo cardInfo, SpecialTriggeredAbility ability)
    {
        return !cardInfo.HasSpecialAbility(ability);
    }

    /// <summary>
    /// Checks if the CardInfo has all of the specified SpecialTriggeredAbilities.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="specialAbilities">The SpecialTriggeredAbilities to check for.</param>
    /// <returns>true if cardInfo has all of the specified SpecialTriggeredAbilities.</returns>
    public static bool HasAllSpecialAbilities(this CardInfo cardInfo, params SpecialTriggeredAbility[] specialAbilities)
    {
        foreach (SpecialTriggeredAbility special in specialAbilities)
        {
            if (cardInfo.LacksSpecialAbility(special))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the CardInfo has none of the specified SpecialTriggeredAbilities.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="specialAbilities">The SpecialTriggeredAbilitiess to check for.</param>
    /// <returns>true if cardInfo has none of the specified SpecialTriggeredAbilities.</returns>
    public static bool LacksAllSpecialAbilities(this CardInfo cardInfo, params SpecialTriggeredAbility[] specialAbilities)
    {
        foreach (SpecialTriggeredAbility special in specialAbilities)
        {
            if (cardInfo.HasSpecialAbility(special))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the CardInfo has any of the specified SpecialTriggeredAbilities.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="specialAbilities">The SpecialTriggeredAbilities to check for.</param>
    /// <returns>true if cardInfo has at least one of the specified SpecialTriggeredAbilities.</returns>
    public static bool HasAnyOfSpecialAbilities(this CardInfo cardInfo, params SpecialTriggeredAbility[] specialAbilities)
    {
        foreach (SpecialTriggeredAbility special in specialAbilities)
        {
            if (cardInfo.HasSpecialAbility(special))
                return true;
        }
        return false;
    }
    #endregion

    #region Trait
    /// <summary>
    /// Checks if the CardInfo does not have a specific Trait.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="trait">The Trait to check for.</param>
    /// <returns>true if the card is does not have the specified Trait.</returns>
    public static bool LacksTrait(this CardInfo cardInfo, Trait trait)
    {
        return !cardInfo.HasTrait(trait);
    }

    /// <summary>
    /// Checks if the CardInfo has all of the specified Traits.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="traits">The Traits to check for.</param>
    /// <returns>true if cardInfo has all of the specified Traits.</returns>
    public static bool HasAllTraits(this CardInfo cardInfo, params Trait[] traits)
    {
        foreach (Trait trait in traits)
        {
            if (cardInfo.LacksTrait(trait))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the CardInfo has none of the specified Traits.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="traits">The Traits to check for.</param>
    /// <returns>true if cardInfo has none of the specified Traits.</returns>
    public static bool LacksAllTraits(this CardInfo cardInfo, params Trait[] traits)
    {
        foreach (Trait trait in traits)
        {
            if (cardInfo.HasTrait(trait))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the CardInfo has any of the specified Traits.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="traits">The Traits to check for.</param>
    /// <returns>true if cardInfo has at least one of the specified Traits.</returns>
    public static bool HasAnyOfTraits(this CardInfo cardInfo, params Trait[] traits)
    {
        foreach (Trait trait in traits)
        {
            if (cardInfo.HasTrait(trait))
                return true;
        }
        return false;
    }

    public static bool IsPelt(this CardInfo cardInfo) => cardInfo.HasTrait(Trait.Pelt);
    public static bool IsTerrain(this CardInfo cardInfo) => cardInfo.HasTrait(Trait.Terrain);
    #endregion

    #region CardMetaCategory
    /// <summary>
    /// Checks if the CardInfo has a specific CardMetaCategory.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="trait">The CardMetaCategory to check for.</param>
    /// <returns>true if the card is does not have the specified CardMetaCategory.</returns>
    public static bool HasCardMetaCategory(this CardInfo cardInfo, CardMetaCategory metaCategory)
    {
        return cardInfo.metaCategories.Contains(metaCategory);
    }

    /// <summary>
    /// Checks if the CardInfo does not have a specific CardMetaCategory.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="metaCategory">The CardMetaCategory to check for.</param>
    /// <returns>true if the card is does not have the specified CardMetaCategory.</returns>
    public static bool LacksCardMetaCategory(this CardInfo cardInfo, CardMetaCategory metaCategory)
    {
        return !cardInfo.HasCardMetaCategory(metaCategory);
    }

    /// <summary>
    /// Checks if the CardInfo has any of the specified CardMetaCategories.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="metaCategories">The CardMetaCategories to check for.</param>
    /// <returns>true if the card has at least one of the specified CardMetaCategories.</returns>
    public static bool HasAnyOfCardMetaCategories(this CardInfo cardInfo, params CardMetaCategory[] metaCategories)
    {
        foreach (CardMetaCategory meta in metaCategories)
        {
            if (cardInfo.HasCardMetaCategory(meta))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the CardInfo has none of the specified CardMetaCategories.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="metaCategories">The CardMetaCategories to check for.</param>
    /// <returns>true if the card has none of the specified CardMetaCategories.</returns>
    public static bool LacksAllCardMetaCategories(this CardInfo cardInfo, params CardMetaCategory[] metaCategories)
    {
        foreach (CardMetaCategory meta in metaCategories)
        {
            if (cardInfo.HasCardMetaCategory(meta))
                return false;
        }
        return true;
    }
    #endregion

    /// <summary>
    /// Checks if the CardInfo does not belong to a specific Tribe.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="tribe">The tribe to check for.</param>
    /// <returns>true if the card is not of the specified tribe.</returns>
    public static bool IsNotOfTribe(this CardInfo cardInfo, Tribe tribe) => !cardInfo.IsOfTribe(tribe);

    public static bool HasAlternatePortrait(this PlayableCard card) => card.Info.alternatePortrait != null;
    public static bool HasAlternatePortrait(this CardInfo info) => info.alternatePortrait != null;
    public static bool HasPixelAlternatePortrait(this CardInfo info) => info.GetAltPortraits().PixelAlternatePortrait != null;

    /// <summary>
    /// Checks if the CardModificationInfo does not have a specific Ability.
    /// </summary>
    /// <param name="mod">CardModificationInfo to access.</param>
    /// <param name="ability">The ability to check for.</param>
    /// <returns>true if the ability does not exist.</returns>
    public static bool LacksAbility(this CardModificationInfo mod, Ability ability) => !mod.HasAbility(ability);

    /// <summary>
    /// Creates a basic EncounterBlueprintData.CardBlueprint based off the CardInfo object.
    /// </summary>
    /// <param name="cardInfo">CardInfo to create the blueprint with.</param>
    /// <returns>The CardBlueprint object that can be used when creating EncounterData.</returns>
    public static EncounterBlueprintData.CardBlueprint CreateBlueprint(this CardInfo cardInfo)
    {
        return new EncounterBlueprintData.CardBlueprint
        {
            card = cardInfo
        };
    }

    /// <summary>
    /// Spawns the CardInfo object to the player's hand.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="temporaryMods">The mods that will be added to the PlayableCard object.</param>
    /// <param name="spawnOffset">The position of where the card will appear from. Default is a Vector3 of (0, 6, 1.5)</param>
    /// <param name="onDrawnTriggerDelay">The amount of time to wait before being added to the hand.</param>
    /// <param name="cardSpawnedCallback">
    /// The action to invoke after the card has spawned but before being added to the hand.
    /// 1. One of two uses in the vanilla game is if the player has completed the event 'ImprovedSmokeCardDiscovered'.
    ///     If this event is complete, the 'Improved Smoke' PlayableCard has the emissive portrait forced on and is then re-rendered.
    /// 2. The other use is during Grimora's fight in Act 2. During the reanimation sequence, the background sprite is replaced with a rare card background.
    /// </param>
    /// <returns>The enumeration of the card being placed in the player's hand.</returns>
    public static IEnumerator SpawnInHand(this CardInfo cardInfo, List<CardModificationInfo> temporaryMods = null, Vector3 spawnOffset = default, float onDrawnTriggerDelay = 0f, Action<PlayableCard> cardSpawnedCallback = null)
    {
        if (spawnOffset == default)
            spawnOffset = CardSpawner.Instance.spawnedPositionOffset;

        yield return CardSpawner.Instance.SpawnCardToHand(cardInfo, temporaryMods, spawnOffset, onDrawnTriggerDelay, cardSpawnedCallback);
    }

    #region Tidal Lock
    /// <summary>
    /// Checks if this card will be killed by the effect of Tidal Lock.
    /// </summary>
    /// <param name="item">PlayableCard to access.</param>
    /// <returns>True if the card is affected by Tidal Lock.</returns>
    public static bool IsAffectedByTidalLock(this PlayableCard item) => item.Info.IsAffectedByTidalLock();

    /// <summary>
    /// Checks if this card info will be killed by the effect of Tidal Lock.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>True if the card info is affected by Tidal Lock.</returns>
    public static bool IsAffectedByTidalLock(this CardInfo info) => info.GetExtendedPropertyAsBool("AffectedByTidalLock") ?? false;

    /// <summary>
    /// Sets whether the card should be killed by Tidal Lock's effect.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>True if the card info should be affected by Tidal Lock.</returns>
    public static CardInfo SetAffectedByTidalLock(this CardInfo info, bool affectedByTidalLock = true)
    {
        info.SetExtendedProperty("AffectedByTidalLock", affectedByTidalLock);
        return info;
    }
    #endregion

    #region PlayableCard

    /// <summary>
    /// Check if the other PlayableCard is on the same side of the board as this PlayableCard.
    /// </summary>
    /// <param name="playableCard">The PlayableCard to access.</param>
    /// <param name="otherCard">The other PlayableCard.</param>
    /// <returns>true if both cards are on the board and both are on the opponent cards or both are player cards.</returns>
    public static bool OtherCardIsOnSameSide(this PlayableCard playableCard, PlayableCard otherCard)
    {
        return playableCard.OnBoard && otherCard.OnBoard && playableCard.OpponentCard == otherCard.OpponentCard;
    }

    /// <summary>
    /// Retrieve a list of all abilities that exist on the PlayableCard.
    ///
    /// This will retrieve all Ability from both TemporaryMods and from the underlying CardInfo object.
    /// </summary>
    /// <param name="playableCard">The PlayableCard to access.</param>
    /// <returns>A list of Ability from the PlayableCard and underlying CardInfo object.</returns>
    public static List<Ability> AllAbilities(this PlayableCard playableCard)
    {
        return playableCard.GetAbilitiesFromAllMods().Concat(playableCard.Info.Abilities).ToList();
    }

    /// <summary>
    /// Retrieve a list of all special triggered abilities that exist on the PlayableCard.
    ///
    /// This will retrieve all SpecialTriggeredAbility from both TemporaryMods and from the underlying CardInfo object.
    /// </summary>
    /// <param name="playableCard">The PlayableCard to access.</param>
    /// <returns>A list of SpecialTriggeredAbility from the PlayableCard and underlying CardInfo object.</returns>
    public static List<SpecialTriggeredAbility> AllSpecialAbilities(this PlayableCard playableCard)
    {
        return new List<SpecialTriggeredAbility>(
            playableCard.TemporaryMods.Concat(playableCard.Info.Mods).SelectMany(mod => mod.specialAbilities)
        );
    }

    /// <summary>
    /// Retrieve a list of Ability that exist in TemporaryMods and the underlying CardInfo.Mods lists.
    /// </summary>
    /// <param name="playableCard">The PlayableCard to access.</param>
    /// <returns>A list of Ability from the PlayableCard and underlying CardInfo object.</returns>
    public static List<Ability> GetAbilitiesFromAllMods(this PlayableCard playableCard)
    {
        return AbilitiesUtil.GetAbilitiesFromMods(playableCard.TemporaryMods.Concat(playableCard.Info.Mods).ToList());
    }

    /// <summary>
    /// Checks if the card has a specific Trait.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="trait">The trait to check for.</param>
    /// <returns>true if the card has the specified trait.</returns>
    public static bool HasTrait(this PlayableCard playableCard, Trait trait)
    {
        return playableCard.Info.HasTrait(trait);
    }

    /// <summary>
    /// Checks if the card has a specific Trait.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="trait">The trait to check for.</param>
    /// <returns>true if the card does not have the specified trait.</returns>
    public static bool LacksTrait(this PlayableCard playableCard, Trait trait)
    {
        return playableCard.Info.LacksTrait(trait);
    }

    /// <summary>
    /// Checks if the PlayableCard has all of the specified Traits.
    ///
    /// A condensed version of `CardInfo.HasAllTraits`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="traits">The Traits to check for.</param>
    /// <returns>true if playableCard has all of the specified Traits.</returns>
    public static bool HasAllTraits(this PlayableCard playableCard, params Trait[] traits)
    {
        foreach (Trait trait in traits)
        {
            if (playableCard.Info.LacksTrait(trait))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the PlayableCard has none of the specified Traits.
    ///
    /// A condensed version of `CardInfo.LacksAllTraits`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="traits">The Traits to check for.</param>
    /// <returns>true if playableCard has none of the specified Traits.</returns>
    public static bool LacksAllTraits(this PlayableCard playableCard, params Trait[] traits)
    {
        foreach (Trait trait in traits)
        {
            if (playableCard.Info.HasTrait(trait))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the PlayableCard has any of the specified Traits.
    ///
    /// A condensed version of `CardInfo.HasAnyOfTraits`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="traits">The Traits to check for.</param>
    /// <returns>true if playableCard has at least one of the specified Traits.</returns>
    public static bool HasAnyOfTraits(this PlayableCard playableCard, params Trait[] traits)
    {
        foreach (Trait trait in traits)
        {
            if (playableCard.Info.HasTrait(trait))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the PlayableCard is of a specified Tribe.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="tribe">The tribe to check for.</param>
    /// <returns>true if the card is of the specified tribe.</returns>
    public static bool IsOfTribe(this PlayableCard playableCard, Tribe tribe)
    {
        return playableCard.Info.IsOfTribe(tribe);
    }

    /// <summary>
    /// Checks if the PlayableCard is not of a specified Tribe.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="tribe">The tribe to check for.</param>
    /// <returns>true if the card is not of the specified tribe.</returns>
    public static bool IsNotOfTribe(this PlayableCard playableCard, Tribe tribe)
    {
        return playableCard.Info.IsNotOfTribe(tribe);
    }

    /// <summary>
    /// Checks if the card is not null and not Dead.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <returns>true if the card is not null or not Dead.</returns>
    public static bool NotDead(this PlayableCard playableCard)
    {
        return playableCard && !playableCard.Dead;
    }

    /// <summary>
    /// Checks if the card is not the opponent's card.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <returns>true if card is not the opponent's card.</returns>
    public static bool IsPlayerCard(this PlayableCard playableCard)
    {
        return !playableCard.OpponentCard;
    }

    /// <summary>
    /// Check the PlayableCard not having a specific Ability.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="ability">The ability to check for</param>
    /// <returns>true if the ability does not exist.</returns>
    public static bool LacksAbility(this PlayableCard playableCard, Ability ability)
    {
        return !playableCard.HasAbility(ability);
    }

    /// <summary>
    /// Check if the PlayableCard has all of the given abilities.
    ///
    /// A condensed version of `CardInfo.HasAllAbilities`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="abilities">The Abilities to check for.</param>
    /// <returns>true if playableCard has all of the given Abilities.</returns>
    public static bool HasAllAbilities(this PlayableCard playableCard, params Ability[] abilities)
    {
        foreach (Ability ability in abilities)
        {
            if (playableCard.Info.LacksAbility(ability))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the PlayableCard has none of the specified Abilities.
    ///
    /// A condensed version of `CardInfo.LacksAllSpecialAbilities`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="abilities">The Abilities to check for.</param>
    /// <returns>true if playableCard has none of the given Abilities.</returns>
    public static bool LacksAllAbilities(this PlayableCard playableCard, params Ability[] abilities)
    {
        foreach (Ability ability in abilities)
        {
            if (playableCard.Info.HasAbility(ability))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Check if the PlayableCard has any of the specified Abilities.
    ///
    /// A condensed version of `CardInfo.HasAnyOfAbilities`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="abilities">The Abilities to check for.</param>
    /// <returns>true if playableCard has at least one of the given Abilities.</returns>
    public static bool HasAnyOfAbilities(this PlayableCard playableCard, params Ability[] abilities)
    {
        foreach (Ability ability in abilities)
        {
            if (playableCard.Info.HasAbility(ability))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check the PlayableCard not having a specific SpecialTriggeredAbility.
    ///
    /// A condensed version of `!playableCard.Info.SpecialAbilities.Contains(ability)`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="ability">The specialTriggeredAbility to check for</param>
    /// <returns>true if the specialTriggeredAbility does not exist.</returns>
    public static bool LacksSpecialAbility(this PlayableCard playableCard, SpecialTriggeredAbility ability)
    {
        return !playableCard.HasSpecialAbility(ability);
    }

    /// <summary>
    /// Check the PlayableCard having a specific SpecialTriggeredAbility.
    ///
    /// A condensed version of `playableCard.Info.SpecialAbilities.Contains(ability)`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="ability">The specialTriggeredAbility to check for</param>
    /// <returns>true if the specialTriggeredAbility does exist.</returns>
    public static bool HasSpecialAbility(this PlayableCard playableCard, SpecialTriggeredAbility ability)
    {
        return playableCard.TemporaryMods.Exists(mod => mod.specialAbilities.Contains(ability))
            || playableCard.Info.HasSpecialAbility(ability);
    }

    /// <summary>
    /// Checks if the PlayableCard has all of the specified SpecialTriggeredAbilities.
    ///
    /// A condensed version of `CardInfo.HasAllSpecialAbilities`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="specialAbilities">The SpecialTriggeredAbilities to check for.</param>
    /// <returns>true if playableCard has all of the specified SpecialTriggeredAbilities.</returns>
    public static bool HasAllSpecialAbilities(this PlayableCard playableCard, params SpecialTriggeredAbility[] specialAbilities)
    {
        foreach (SpecialTriggeredAbility special in specialAbilities)
        {
            if (playableCard.Info.LacksSpecialAbility(special))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the PlayableCard has none of the specified SpecialTriggeredAbilities.
    ///
    /// A condensed version of `CardInfo.LacksAllSpecialAbilities`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="specialAbilities">The SpecialTriggeredAbilitiess to check for.</param>
    /// <returns>true if playableCard has none of the specified SpecialTriggeredAbilities.</returns>
    public static bool LacksAllSpecialAbilities(this PlayableCard playableCard, params SpecialTriggeredAbility[] specialAbilities)
    {
        foreach (SpecialTriggeredAbility special in specialAbilities)
        {
            if (playableCard.Info.HasSpecialAbility(special))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the PlayableCard has any of the specified SpecialTriggeredAbilities.
    ///
    /// A condensed version of `CardInfo.HasAnyOfSpecialAbilities`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="specialAbilities">The SpecialTriggeredAbilities to check for.</param>
    /// <returns>true if playableCard has at least one of the specified SpecialTriggeredAbilities.</returns>
    public static bool HasAnyOfSpecialAbilities(this PlayableCard playableCard, params SpecialTriggeredAbility[] specialAbilities)
    {
        foreach (SpecialTriggeredAbility special in specialAbilities)
        {
            if (playableCard.Info.HasSpecialAbility(special))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if the PlayableCard has a card opposing it in the opposite slot.
    ///
    /// Also acts as a null check if this PlayableCard is in a slot.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <returns>true if a card exists in the opposing slot.</returns>
    public static bool HasOpposingCard(this PlayableCard playableCard)
    {
        return playableCard.Slot && playableCard.Slot.opposingSlot.Card;
    }

    /// <summary>
    /// Retrieve the CardSlot object that is opposing this PlayableCard.
    /// </summary>
    /// <remarks>It is on the implementer to check if the returned value is not null</remarks>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <returns>The card slot opposite of this playableCard, otherwise return null.</returns>
    public static CardSlot OpposingSlot(this PlayableCard playableCard)
    {
        return playableCard.Slot ? playableCard.Slot.opposingSlot : null;
    }

    /// <summary>
    /// Retrieve the PlayableCard that is opposing this PlayableCard in the opposite slot.
    /// </summary>
    /// <remarks>It is on the implementer to check if the returned value is not null</remarks>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <returns>The card in the opposing slot, otherwise return null.</returns>
    public static PlayableCard OpposingCard(this PlayableCard playableCard)
    {
        return playableCard.OpposingSlot()?.Card;
    }

    public static void AddTemporaryMods(this PlayableCard card, params CardModificationInfo[] mods)
    {
        foreach (CardModificationInfo mod in mods)
            card.AddTemporaryMod(mod);
    }
    public static void RemoveTemporaryMods(this PlayableCard card, params CardModificationInfo[] mods)
    {
        foreach (CardModificationInfo mod in mods)
            card.RemoveTemporaryMod(mod);
    }

    #endregion

    #endregion

    #region ExtendedProperties

    /// <summary>
    /// Adds a custom property value to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetExtendedProperty(this CardInfo info, string propertyName, object value)
    {
        info.GetCardExtensionTable()[propertyName] = value?.ToString();
        return info;
    }

    /// <summary>
    /// Gets a custom property value from the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">The name of the property to get the value of.</param>
    /// <returns>The custom property value as a string. If it doesn't exist, returns null.</returns>
    public static string GetExtendedProperty(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as an int (can be null).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int.</returns>
    public static int? GetExtendedPropertyAsInt(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a float (can be null).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float.</returns>
    public static float? GetExtendedPropertyAsFloat(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a boolean (can be null).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }

    #endregion

    #region ModPrefixesAndTags

    /// <summary>
    /// Sets the mod guid that was derived from the call stack
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="modGuid">Mod Guid</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    internal static CardInfo SetModTag(this CardInfo info, string modGuid)
    {
        info.SetExtendedProperty("CallStackModGUID", modGuid);
        return info;
    }

    /// <summary>
    /// Gets the GUID of the mod that created this card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The ID of the mod that created this card, or null if it wasn't found.</returns>
    public static string GetModTag(this CardInfo info)
    {
        return info.GetExtendedProperty("CallStackModGUID");
    }

    /// <summary>
    /// Sets the mod prefix for the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="modPrefix">Mod prefix.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    internal static CardInfo SetModPrefix(this CardInfo info, string modPrefix)
    {
        info.SetExtendedProperty("ModPrefix", modPrefix);
        return info;
    }

    /// <summary>
    /// Gets the card name prefix for this card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The mod prefix for this card, or null if it wasn't found.</returns>
    public static string GetModPrefix(this CardInfo info)
    {
        return info.GetExtendedProperty("ModPrefix");
    }
    /// <summary>
    /// Checks whether the card's mod prefix is equal to the given string.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="prefixToMatch">The prefix to check for.</param>
    /// <returns>True if the CardInfo's mod prefix equals prefixToMatch.</returns>
    public static bool ModPrefixIs(this CardInfo info, string prefixToMatch)
    {
        return info.GetExtendedProperty("ModPrefix") == prefixToMatch;
    }

    #endregion

    #region OldAPICard
    internal static CardInfo SetOldApiCard(this CardInfo info, bool isOldApiCard = true)
    {
        info.SetExtendedProperty("AddedByOldApi", isOldApiCard);
        return info;
    }

    internal static bool IsOldApiCard(this CardInfo info)
    {
        bool? isOAPI = info.GetExtendedPropertyAsBool("AddedByOldApi");
        return isOAPI.HasValue && isOAPI.Value;
    }
    #endregion

    #region Costs
    /// <summary>
    /// Gets a PlayableCards using this specific CardInfo.
    /// Sometimes inscryption clones CardInfo's and sometimes its reused so there may be more than 1 card using the same CardInfo
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <returns>Playable Card on the board, in your hand or on display somewhere</returns>
    public static PlayableCard GetPlayableCard(this CardInfo cardInfo)
    {
        if (CostProperties.CostProperties.CardInfoToCard.TryGetValue(cardInfo, out List<WeakReference<PlayableCard>> cardList))
        {
            for (int i = cardList.Count - 1; i >= 0; i--)
            {
                if (cardList[i].TryGetTarget(out PlayableCard card) && card != null)
                    return card;

                cardList.RemoveAt(i);
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the blood cost of a card.
    /// This function can be overriden if someone wants to inject new cost into a cards blood cost
    /// </summary>
    public static int BloodCost(this PlayableCard card)
    {
        //Debug.Log($"{card != null} {card?.Info != null} [{card?.GetType()}] {(card as DiskCardGame.Card)?.Info != null}");
        if (card && card.Info)
        {
            int originalBloodCost = CostProperties.CostProperties.OriginalBloodCost(card.Info);

            if (card.IsUsingBlueGem())
                originalBloodCost--;

            // add adjustments from temp mods
            foreach (CardModificationInfo mod in card.TemporaryMods)
                originalBloodCost += mod.bloodCostAdjustment;

            return originalBloodCost;
        }

        InscryptionAPIPlugin.Logger.LogError("[BloodCost] Couldn't find Card or CardInfo for blood cost??? How is this possible?");
        return 0;
    }


    /// <summary>
    /// Returns the bone cost of a card.
    /// This function can be overriden if someone wants to inject new cost into a cards bone cost
    /// </summary>
    public static int BonesCost(this PlayableCard card)
    {
        if (card && card.Info)
        {
            int originalBonesCost = CostProperties.CostProperties.OriginalBonesCost(card.Info);
            if (card.IsUsingBlueGem())
                originalBonesCost--;

            // add adjustments from temp mods
            foreach (CardModificationInfo mod in card.TemporaryMods)
                originalBonesCost += mod.bonesCostAdjustment;

            return originalBonesCost;
        }

        InscryptionAPIPlugin.Logger.LogError("Couldn't find Card or CardInfo for bone cost??? How is this possible?");
        return 0;
    }


    /// <summary>
    /// Returns the gem cost of a card.
    /// This function can be overriden if someone wants to inject new cost into a cards gem cost
    /// </summary>
    public static List<GemType> GemsCost(this PlayableCard card)
    {
        if (!card || !card.Info)
            return new();

        List<CardModificationInfo> mods = card.TemporaryMods.Concat(card.Info.Mods).ToList();
        // if gems are nullified, return a new list
        if (mods.Exists((CardModificationInfo x) => x.nullifyGemsCost))
            return new List<GemType>();

        List<GemType> gemsCost = new(card.Info.gemsCost);
        foreach (CardModificationInfo mod in mods)
        {
            if (mod.addGemCost == null)
                continue;

            foreach (GemType item in mod.addGemCost)
            {
                if (!gemsCost.Contains(item))
                    gemsCost.Add(item);
            }
        }

        if (gemsCost.Count > 0 && card.IsUsingBlueGem())
            gemsCost.RemoveAt(0);

        return gemsCost;
    }

    public static bool IsGemified(this PlayableCard card)
    {
        return card.Info.Gemified || card.TemporaryMods.Exists((CardModificationInfo x) => x.gemify);
    }
    public static bool OwnerHasBlueGem(this PlayableCard card)
    {
        return card.OpponentCard ? OpponentGemsManager.Instance.HasGem(GemType.Blue) : ResourcesManager.Instance.HasGem(GemType.Blue);
    }
    public static bool IsUsingBlueGem(this PlayableCard card)
    {
        return card.IsGemified() && card.OwnerHasBlueGem();
    }
    #endregion
}
