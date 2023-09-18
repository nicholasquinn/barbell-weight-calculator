
using BarbellWeightCalculator;
using Metsys.Bson;
using System.Diagnostics;

// define the tests...

List<Test> tests = new List<Test> {

    new Test("WeightConfiguration_TargetWeight_NotNegative", () => 
    {
        WeightConfiguration weightConfiguration = new (-1,0,true,new () { { 0,0 } });
        return weightConfiguration.TargetWeight == 0;
    }),

    new Test("WeightConfiguration_TargetWeight_NotLessThanBarbellWeight", () => 
    {
        WeightConfiguration weightConfiguration = new (20,20,true,new () { { 0,0 } });
        Debug.Assert(weightConfiguration.TargetWeight == 20);
        Debug.Assert(weightConfiguration.BarbellWeight == 20);
        weightConfiguration.TargetWeight = 10;
        return weightConfiguration.TargetWeight == 20;
    }),

    new Test("WeightConfiguration_TargetWeight_UpperBound", () =>
    {
        WeightConfiguration weightConfiguration = new (WeightConfiguration.MaxTargetWeight,0,true,new () { { 0,0 } });
        Debug.Assert(weightConfiguration.TargetWeight == WeightConfiguration.MaxTargetWeight);
        weightConfiguration.TargetWeight =  WeightConfiguration.MaxTargetWeight + 1;
        return weightConfiguration.TargetWeight ==  WeightConfiguration.MaxTargetWeight;
    }),

    new Test("WeightConfiguration_BarbellWeight_NotNegative", () => 
    {
        WeightConfiguration weightConfiguration = new (0,-10,true,new () { { 0,0 } });
        return weightConfiguration.BarbellWeight == 0;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_OnlyBarbell", () => 
    {
        WeightConfiguration weightConfiguration
            = new (20,20,true, Test.Helpers.FullWeightSet);
        return weightConfiguration.CalculatePlateSet().Count == 0;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_SinglePlate", () => 
    {
        WeightConfiguration weightConfiguration
            = new (70,20,true, Test.Helpers.FullWeightSet);
        // Expect to use a single 25kg plate since we have a target weight of 70kg
        // and the bar is 20kg, hence we need 70-20=50kg of plates, therefore 25kg
        // per side, and we have 25kg plates, so should use that one plate per side.
        var plateSet = weightConfiguration.CalculatePlateSet();
        plateSet.TryGetValue(25, out uint twentyFiveCount);
        return plateSet.Count == 1 && twentyFiveCount == 1;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_TwoDistinctPlatesNoSkip", () => 
    {
        WeightConfiguration weightConfiguration
            = new (110,20,true, Test.Helpers.FullWeightSet);
        // Expect to use 1x 25kg + 1x 20kg since we have a target weight of 110kg
        // and the bar is 20kg, hence we need 110-20=90kg of plates, therefore 45kg
        // per side, and we have 25kg and 20kg plates, so should use one per side.
        var plateSet = weightConfiguration.CalculatePlateSet();
        plateSet.TryGetValue(25, out uint twentyFiveCount);
        plateSet.TryGetValue(20, out uint twentyCount);
        return plateSet.Count == 2 && twentyFiveCount == 1 && twentyCount == 1;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_TwoDistinctPlatesWithSkip", () => 
    {
        WeightConfiguration weightConfiguration
            = new (100,20,true, Test.Helpers.FullWeightSet);
        // Expect to use 1x 25kg + 1x 15kg since we have a target weight of 100kg
        // and the bar is 20kg, hence we need 100-20=80kg of plates, therefore 40kg
        // per side, and we have 25kg and 15kg plates, so should use one per side.
        // Note that the 20kg plate will be skipped.
        var plateSet = weightConfiguration.CalculatePlateSet();
        plateSet.TryGetValue(25, out uint twentyFiveCount);
        plateSet.TryGetValue(15, out uint fifteenCount);
        return plateSet.Count == 2 && twentyFiveCount == 1 && fifteenCount == 1;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_CheckNotGreedy", () =>
    {
        WeightConfiguration weightConfiguration
            = new (100,20,true, Test.Helpers.FullWeightSet);
        // Expect to use 1x 25kg + 1x 15kg since we have a target weight of 100kg
        // and the bar is 20kg, hence we need 100-20=80kg of plates, therefore 40kg
        // per side, and we have 25kg and 15kg plates, so should use one per side.
        // Note that the 20kg plate will be skipped.
        var plateSet = weightConfiguration.CalculatePlateSet();
        plateSet.TryGetValue(25, out uint twentyFiveCount);
        plateSet.TryGetValue(15, out uint fifteenCount);
        return plateSet.Count == 2 && twentyFiveCount == 1 && fifteenCount == 1;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_CheckImpossibleInRange", () =>
    {
        WeightConfiguration weightConfiguration
            = new (100.125,20,true, Test.Helpers.FullWeightSet);
        // We don't have any .125 plates, so impossible to make this weight even
        // though we can make weights higher than this amount
        var plateSet = weightConfiguration.CalculatePlateSet();
        return plateSet.Count == 0;
    }),

    new Test("WeightConfiguration_CalculateWeightSet_CheckImpossibleOutOfRange", () =>
    {
        WeightConfiguration weightConfiguration
            = new ( WeightConfiguration.MaxTargetWeight,20,true, Test.Helpers.FullWeightSet);
        // We cannot reach this value
        var plateSet = weightConfiguration.CalculatePlateSet();
        return plateSet.Count == 0;
    }),

    new Test("PersistenceService_ReadWeightConfiguration_NoExistingFile", () =>
    {
        Test.Helpers.DeleteFile(Test.Helpers.FullFilePath);
        var persistenceService 
            = new PersistenceService(Test.Helpers.TestFilePath, Test.Helpers.TestFileName);
        WeightConfiguration weightConfiguration = persistenceService.ReadWeightConfiguration();
        return
            weightConfiguration.TargetWeight == 0 &&
            weightConfiguration.BarbellWeight == 0 &&
            weightConfiguration.IsMetric &&
            weightConfiguration.PlatesPerSide.Count == 0;
    }),

    new Test("PersistenceService_WriteWeightConfiguration_NoExistingFile", () =>
    {
        Test.Helpers.DeleteFile(Test.Helpers.FullFilePath);

        var persistenceService
            = new PersistenceService(Test.Helpers.TestFilePath, Test.Helpers.TestFileName);

        WeightConfiguration weightConfiguration
            = new ( WeightConfiguration.MaxTargetWeight,20,true, Test.Helpers.FullWeightSet);
        return persistenceService.WriteWeightConfiguration(weightConfiguration);
    }),

    new Test("PersistenceService_ReadWeightConfiguration_ExistingFile", () =>
    {
        var persistenceService
            = new PersistenceService(Test.Helpers.TestFilePath, Test.Helpers.TestFileName);
        WeightConfiguration weightConfiguration = persistenceService.ReadWeightConfiguration();
        return // values from above write test case
            weightConfiguration.TargetWeight == WeightConfiguration.MaxTargetWeight &&
            weightConfiguration.BarbellWeight == 20 &&
            weightConfiguration.IsMetric &&
            weightConfiguration.PlatesPerSide.SequenceEqual(Test.Helpers.FullWeightSet);
    }),

    new Test("PersistenceService_WriteWeightConfiguration_ExistingFile", () =>
    {
        var persistenceService
            = new PersistenceService(Test.Helpers.TestFilePath, Test.Helpers.TestFileName);

        WeightConfiguration weightConfiguration
            = new ( WeightConfiguration.MaxTargetWeight,20,true, Test.Helpers.FullWeightSet);
        return persistenceService.WriteWeightConfiguration(weightConfiguration);
    }),
};

// then run the tests!

Console.WriteLine("Running tests...\n");
foreach (var test in tests) test.Run();
Test.PrintSummary();

class Test
{
    public static int NumPassed { get; set; }
    static int NumRan { get; set; }

    string _name = string.Empty;
    Func<bool> _testFunction;

    public Test(string name, Func<bool> testFunction)
    {
        _name = name;
        _testFunction = testFunction;
    }

    public void Run()
    {
        Console.Write($"Test \"{_name}\" ");
        bool result = _testFunction();
        ++NumRan;

        var originalColor = Console.ForegroundColor;
        if (result) 
        {
            ++NumPassed;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Passed");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed");
        }

        Console.ForegroundColor = originalColor;
    }

    public static void PrintSummary()
    {
        Console.Write("\nResults: ");
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor =
            NumPassed == NumRan ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{NumPassed}/{NumRan}");
        Console.ForegroundColor = originalColor;
    }

    public static class Helpers
    {
        public static SortedDictionary<double, uint> FullWeightSet
                => new SortedDictionary<double, uint>() {
                    { 25, 2 }, { 20, 10 }, { 15, 10 }, { 10, 2 }, { 5, 2 },
                    { 2.5, 2 }, { 2, 2 }, { 1.5, 2 }, { 1, 2 }, { .5, 2 }
                };

        public static string TestFilePath => Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

        public static string TestFileName => "barbell-weight-calculator-test-data.json";

        public static string FullFilePath => Path.Combine(TestFilePath, TestFileName);

        public static void DeleteFile(string filePath)
        {
            // don't care if this has an exception, will manually fix
            if (File.Exists(filePath)) File.Delete(filePath);
        }

    }
}
