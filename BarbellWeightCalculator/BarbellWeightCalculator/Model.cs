using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BarbellWeightCalculator
{

    public class WeightConfiguration
    {
        const byte BARBELL_SIDES = 2;
        // Only support 3 decimal places of precision for weight values e.g. 0.125kg
        readonly uint WEIGHT_SCALER = 1000;

        private double _targetWeight;

        public double TargetWeight { 
            get => _targetWeight;
            set
            {
                if (value < 0 || value < BarbellWeight || value == _targetWeight
                    || value > MaxTargetWeight) return;
                _targetWeight = value;
            }
        }

        public static double MaxTargetWeight => 2000;

        private double _barbellWeight;

        public double BarbellWeight { 
            get => _barbellWeight; 
            set 
            {
                if (value < 0 || value == _barbellWeight) return;
                _barbellWeight = value;
            }
        }

        public bool IsMetric { get; set; }

        // plate -> quantity e.g. 25 -> 2 means we have 2x 25 kg|lb plates
        // storing double for plate key since it makes sorting easy and double for
        // value since plates can have fractional weights e.g. a 2.5 kg plate.
        // Storing plates per side since we load the same on each side. This does
        // assume that the person has pairs of each plate, but that is a reasonable
        // assumption.
        public SortedDictionary<double, uint> PlatesPerSide { get; set; }

        public WeightConfiguration(double targetWeight, double barbellWeight, bool isMetric, 
            SortedDictionary<double, uint> platesPerSide)
        {
            TargetWeight = targetWeight;
            BarbellWeight = barbellWeight;
            IsMetric = isMetric;
            PlatesPerSide = platesPerSide;
        }

        private double CalculateSingleSidePlateWeight() 
            => (TargetWeight - BarbellWeight) / BARBELL_SIDES;


        /// <summary>
        /// Uses dynamic programming to calculate the plates required to hit a target
        /// weight. Returns an empty list if not possible.
        /// </summary>
        /// <param name="plates"> The list of plate weights available per side.
        /// e.g. 25, 25, 20, 20, 20, 20, 15, 10, 2.5, 1 </param>
        /// <param name="targetWeight"> The target weight to make from the plates. </param>
        /// <returns></returns>
        private List<double> FindMinimumWeights(double[] plates, double targetWeight)
        {
            int n = plates.Length;
            int[] memo = new int[(int)(targetWeight * WEIGHT_SCALER) + 1];
            int[] lastWeightIndex = new int[(int)(targetWeight * WEIGHT_SCALER) + 1];

            int intTargetWeight = (int)(targetWeight * WEIGHT_SCALER);

            // Initialize memo with a large value (infinite) to represent no solution
            for (int i = 1; i <= intTargetWeight; i++)
            {
                memo[i] = int.MaxValue;
            }

            // Set the base case
            memo[0] = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = intTargetWeight; j >= 0; j--)
                {
                    if (memo[j] != int.MaxValue && j + (int)(plates[i] * WEIGHT_SCALER) <= intTargetWeight)
                    {
                        int nextWeight = j + (int)(plates[i] * WEIGHT_SCALER);
                        // <= since we want to take largest plates possible
                        if (memo[j] + 1 <= memo[nextWeight])
                        {
                            memo[nextWeight] = memo[j] + 1;
                            lastWeightIndex[nextWeight] = i;
                        }
                    }
                }
            }

            if (memo[intTargetWeight] == int.MaxValue)
            {
                return new List<double>(); // No solution
            }

            // Reconstruct the minimum weight subset
            List<double> selectedWeights = new List<double>();
            int remainingWeight = intTargetWeight;
            while (remainingWeight > 0)
            {
                int lastWeightIdx = lastWeightIndex[remainingWeight];
                selectedWeights.Add(plates[lastWeightIdx]);
                remainingWeight -= (int)(plates[lastWeightIdx] * WEIGHT_SCALER);
            }

            return selectedWeights;
        }

        /// <summary>
        /// Calculates the set of weight plates you need to put on each side of the barbell
        /// in order to achieve the target weight (or empty set if not possible).
        /// The set of weights is guaranteed to be the minimal set i.e. there may be other
        /// ways to achieve the target weight, but they use the same amount or more plates.
        /// </summary>
        /// <returns></returns>
        public Dictionary<double, uint> CalculatePlateSet()
        {
            // get flat array of weight plates i.e. dictionary of 25kg -> 2, 10kg -> 1
            // would become 25, 25, 10
            double[] plates = PlatesPerSide.Reverse().SelectMany(
                pair => Enumerable.Repeat(pair.Key, (int)pair.Value)).ToArray();

            double remainingWeight = CalculateSingleSidePlateWeight();

            var answer = FindMinimumWeights(plates, remainingWeight);

            var plateSet = new Dictionary<double, uint>();
            foreach (double plate in answer)
            {
                plateSet.TryGetValue(plate, out uint plateCount);
                plateSet[plate] = plateCount + 1;
            }
            return plateSet;
        }

    }
}
