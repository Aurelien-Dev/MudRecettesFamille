﻿using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Components.Pages.RecetteBook.BlockDefinitions;

public static class IngredientComponentService
{
    public const int Step = 10; // Default spacing
    private const int MinSpacing = 2; // Critical spacing threshold
    private const int MaxReorderOperations = 10; // Max number of moves before restabilization

    private static int reorderCount = 0; // Move counter

    /// <summary>
    /// Assigns spaced values to Orders to avoid conflicts.
    /// </summary>
    public static void RestabilizeOrders(List<IngredientDto> ingredients)
    {
        var sortedList = ingredients.OrderBy(i => i.Order).ToList();
        for (int i = 0; i < sortedList.Count; i++)
        {
            sortedList[i].Order = (i + 1) * Step; // 10, 20, 30, ...
        }
        reorderCount = 0; // Reset the counter
    }

    /// <summary>
    /// Checks if all Orders are consecutive (e.g., 1, 2, 3, 4...).
    /// </summary>
    private static bool AreOrdersConsecutive(List<IngredientDto> ingredients)
    {
        var sortedList = ingredients.OrderBy(i => i.Order).ToList();
        for (int i = 1; i < sortedList.Count; i++)
        {
            if (sortedList[i].Order != sortedList[i - 1].Order + 1)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if restabilization is necessary.
    /// </summary>
    public static bool ShouldRestabilize(List<IngredientDto> ingredients)
    {
        // If orders are simply incremented by 1, immediate restabilization
        if (AreOrdersConsecutive(ingredients))
        {
            return true;
        }

        var sortedList = ingredients.OrderBy(i => i.Order).ToList();

        // Check the minimum spacing between elements
        for (int i = 1; i < sortedList.Count; i++)
        {
            if (sortedList[i].Order - sortedList[i - 1].Order < MinSpacing)
            {
                return true;
            }
        }

        // Check if too many moves have taken place
        if (reorderCount >= MaxReorderOperations)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves an ingredient up.
    /// </summary>
    public static void MoveUp(List<IngredientDto> ingredients, int ingredientId)
    {
        var sortedList = ingredients.OrderBy(i => i.Order).ToList();
        var index = sortedList.FindIndex(i => i.Id == ingredientId);

        if (index <= 0) return; // Already at the top or not found

        var current = sortedList[index];
        var previous = sortedList[index - 1];

        if (index == 1)
        {
            current.Order = previous.Order - Step;
        }
        else
        {
            current.Order = previous.Order - ((previous.Order - sortedList[index - 2].Order) / 2);
        }

        reorderCount++;

        // Check if restabilization is necessary
        if (ShouldRestabilize(ingredients))
        {
            RestabilizeOrders(ingredients);
        }
    }

    /// <summary>
    /// Moves an ingredient down.
    /// </summary>
    public static void MoveDown(List<IngredientDto> ingredients, int ingredientId)
    {
        var sortedList = ingredients.OrderBy(i => i.Order).ToList();
        var index = sortedList.FindIndex(i => i.Id == ingredientId);

        if (index < 0 || index >= sortedList.Count - 1) return; // Already at the bottom or not found

        var current = sortedList[index];
        var next = sortedList[index + 1];

        if (index == sortedList.Count - 2)
        {
            current.Order = next.Order + Step;
        }
        else
        {
            current.Order = next.Order + ((sortedList[index + 2].Order - next.Order) / 2);
        }

        reorderCount++;

        // Check if restabilization is necessary
        if (ShouldRestabilize(ingredients))
        {
            RestabilizeOrders(ingredients);
        }
    }
}
