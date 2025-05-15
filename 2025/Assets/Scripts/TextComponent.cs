using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextComponent : MonoBehaviour
{
    private SortedDictionary<int, CensorTarget> censorTargets = new SortedDictionary<int, CensorTarget>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCensorTarget(CensorTarget censorTarget, int firstCharacterIndex)
    {
        if (censorTarget == null)
        {
            Debug.LogError("Censor target is null.");
            return;
        }

        //Debug.Log($"Censor target addition attempt for the specified word location: {(lineIndex, wordIndex)}.");
        
        try 
        {
            censorTargets.Add(firstCharacterIndex, censorTarget);

            //Debug.Log($"Censor target added for the specified word location: {(lineIndex, wordIndex)}.");
        }
        catch (System.ArgumentException e)
        {
            Debug.LogError($"Censor target already exists for the specified word location: {firstCharacterIndex}. Exception: {e.Message}");
        }
        
    }
    
    public CensorTarget GetCensorTarget(int firstCharacterIndex)
    {
        if (censorTargets == null)
        {
            Debug.LogError("Censor targets dictionary is not initialized.");
            return null;
        }

        try 
        {
            //Debug.Log($"Censor target retrieval attempt for the specified word location: {(lineIndex, wordIndex)}.");

            return censorTargets[firstCharacterIndex];
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogError($"Censor target not found for the specified word location: {firstCharacterIndex}. Exception: {e.Message}");
            return null;
        }
    }

    public List<CensorTarget> ClearCensorTargetIndices(int firstCharacterIndex)
    {
        if (censorTargets == null)
        {
            Debug.LogError("Censor targets dictionary is not initialized.");
            return null;
        }

        List<CensorTarget> targets = new List<CensorTarget>();
    

        // Iterate over the keys starting from the first key >= firstCharacterIndex
        List<int> keysToRemove = new List<int>();
        foreach (var pair in censorTargets)
        {
            if (pair.Key >= firstCharacterIndex)
            {
                targets.Add(pair.Value);
                keysToRemove.Add(pair.Key);
            }
        }

        // Remove the keys after iteration
        foreach (int key in keysToRemove)
        {
            censorTargets.Remove(key);
        }

        return targets;
    }
}
