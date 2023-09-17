
// define the tests...

List<Test> tests = new List<Test> {
    new Test("dummy failure", () => { return false; } )
    ,new Test("dummy success", () => { return true; } )
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
}
