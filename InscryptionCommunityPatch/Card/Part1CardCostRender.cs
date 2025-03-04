using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class Part1CardCostRender
{
    // This patches the way card costs are rendered in Act 1 (Leshy's cabin)
    // It allows mixed card costs to display correctly (i.e., 2 blood, 1 bone)
    // And allows gem cost and energy cost to render on the card at all.

    public static event Action<CardInfo, List<Texture2D>> UpdateCardCost;

    private static readonly Dictionary<string, Texture2D> AssembledTextures = new();

    public const int COST_OFFSET = 28;

    public const int MOX_OFFSET = 21;

    public static Texture2D CombineCostTextures(List<Texture2D> costs)
    {
        while (costs.Count < 4)
            costs.Add(null);
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("empty_cost.png", typeof(Part1CardCostRender).Assembly);
        return TextureHelper.CombineTextures(costs, baseTexture, yStep: COST_OFFSET);
    }

    public static Texture2D CombineMoxTextures(List<Texture2D> costs)
    {
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("mox_cost_empty.png", typeof(Part1CardCostRender).Assembly);
        return TextureHelper.CombineTextures(costs, baseTexture, xStep: MOX_OFFSET);
    }

    private static Texture2D GetTextureByName(string key)
    {
        if (AssembledTextures.ContainsKey(key))
        {
            if (AssembledTextures[key] != null)
                return AssembledTextures[key];

            AssembledTextures.Remove(key);
        }

        Texture2D texture = TextureHelper.GetImageAsTexture($"{key}.png", typeof(Part1CardCostRender).Assembly);
        AssembledTextures.Add(key, texture);
        return texture;
    }

    public static Sprite Part1SpriteFinal(CardInfo cardInfo)
    {
        PlayableCard playableCard = cardInfo.GetPlayableCard();

        // A list to hold the textures (important later, to combine them all)
        List<Texture2D> list = new();

        // Setting mox first
        List<GemType> gemsCost = playableCard?.GemsCost() ?? cardInfo.GemsCost;
        if (gemsCost.Count > 0)
        {
            // Make a new list for the mox textures
            List<Texture2D> gemCost = new();

            // Add all moxes to the gemcost list
            if (gemsCost.Contains(GemType.Green))
                gemCost.Add(GetTextureByName("mox_cost_g"));

            if (gemsCost.Contains(GemType.Blue))
                gemCost.Add(GetTextureByName("mox_cost_b"));

            if (gemsCost.Contains(GemType.Orange))
                gemCost.Add(GetTextureByName("mox_cost_o"));

            while (gemCost.Count < 3)
                gemCost.Insert(0, null);

            // Combine the textures into one
            list.Add(CombineMoxTextures(gemCost));
        }

        int energyCost = playableCard?.EnergyCost ?? cardInfo.EnergyCost;
        if (energyCost > 0) // there's 6+ texture but since Energy can't go above 6 normally I have excluded it from consideration
            list.Add(GetTextureByName($"energy_cost_{Mathf.Min(6, energyCost)}"));

        int bonesCost = playableCard?.BonesCost() ?? cardInfo.BonesCost;
        if (bonesCost > 0)
            list.Add(GetTextureByName($"bone_cost_{Mathf.Min(14, bonesCost)}"));

        int bloodCost = playableCard?.BloodCost() ?? cardInfo.BloodCost;
        if (bloodCost > 0)
            list.Add(GetTextureByName($"blood_cost_{Mathf.Min(14, bloodCost)}"));

        // Call the event and allow others to modify the list of textures
        UpdateCardCost?.Invoke(cardInfo, list);

        // Combine all the textures from the list into one texture
        Texture2D finalTexture = CombineCostTextures(list);

        // Convert the final texture to a sprite
        return TextureHelper.ConvertTexture(finalTexture, TextureHelper.SpriteType.OversizedCostDecal);
    }

    [HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.GetCostSpriteForCard))]
    [HarmonyPrefix]
    private static bool Part1CardCostDisplayerPatch(ref Sprite __result, CardDisplayer __instance, CardInfo card)
    {
        //Make sure we are in Leshy's Cabin
        if (__instance is CardDisplayer3D && SceneLoader.ActiveSceneName.StartsWith("Part1"))
        {
            // Set the results as the new sprite
            __result = Part1SpriteFinal(card);
            return false;
        }
        
        return true;
    }
}