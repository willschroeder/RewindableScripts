using UnityEngine;

public static class Util {
     public static bool ValueInRange(float value, float min, float max) {
          return value > min && value < max;
     }

     public static float BringToZero(float currentValue, float rate) {
          if (ValueInRange(currentValue, -1f, 1f)) {
               return 0;
          }
          
          // If current value in positives
          if (currentValue > 0) {
               if (currentValue > rate) {
                    return currentValue - rate; 
               }

               return 0;
          }

          if (currentValue < 0) {
               if (currentValue < -rate) {
                    return currentValue + rate;
               }

               return 0;
          }
          

          return 0;
     }
}