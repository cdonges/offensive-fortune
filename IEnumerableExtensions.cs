// Source - https://stackoverflow.com/a/11930875
// Posted by necrogt4, modified by community. See post 'Timeline' for change history
// Retrieved 2026-02-15, License - CC BY-SA 4.0

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class IEnumerableExtensions {
    
    public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector) {
        float totalWeight = sequence.Sum(weightSelector);
        // The weight we are after...
        float itemWeightIndex =  (float)new Random().NextDouble() * totalWeight;
        float currentWeightIndex = 0;

        foreach(var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) }) {
            currentWeightIndex += item.Weight;
            
            // If we've hit or passed the weight we are after for this item then it's the one we want....
            if(currentWeightIndex >= itemWeightIndex)
                return item.Value;
            
        }
        
        return default;
        
    }
    
}
