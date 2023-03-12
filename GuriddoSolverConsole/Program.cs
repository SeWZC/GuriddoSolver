using GuriddoSolver;

var guriddoGrid = new GuriddoGrid(new[]
    {
        new[] { 0, 0, 2, 0, 0, 0, 9, 0, 0 },
        new[] { 0, 0, 1, 0, 0, 7, 0, 0, 0 },
        new[] { 0, 0, 9, 0, 0, 0, 0, 0, 0 },
        new[] { 0, 2, 3, 1, 0, 0, 0, 0, 0 },
        new[] { 0, 0, 0, 7, 8, 9, 0, 3, 4 },
        new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        new[] { 0, 7, 0, 6, 0, 0, 0, 2, 1 },
        new[] { 0, 8, 0, 0, 6, 0, 0, 0, 0 },
        new[] { 0, 9, 0, 8, 0, 0, 0, 0, 0 }
    },
    new[]
    {
        new[] { false, false, false, false, true, false, false, false, false },
        new[] { false, false, false, false, false, false, false, false, false },
        new[] { false, false, true, false, false, false, false, true, false },
        new[] { true, false, false, true, false, false, false, false, false },
        new[] { false, false, true, false, false, false, true, false, false },
        new[] { false, false, false, false, false, true, false, false, true },
        new[] { false, true, false, false, false, false, true, false, false },
        new[] { false, false, false, false, false, false, false, false, false },
        new[] { false, false, false, false, true, false, false, false, false }
    });
Console.WriteLine(guriddoGrid);