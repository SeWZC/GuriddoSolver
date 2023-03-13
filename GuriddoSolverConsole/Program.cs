using GuriddoSolver;

var guriddoGrid = new GuriddoGrid(new[]
    {
        new[] { 2, 3, 0, 0, 0, 0, 0, 0, 0 },
        new[] { 4, 0, 0, 0, 0, 0, 0, 0, 0 },
        new[] { 0, 0, 0, 9, 0, 0, 0, 0, 0 },
        new[] { 0, 0, 0, 0, 0, 0, 6, 8, 9 },
        new[] { 0, 0, 2, 0, 0, 0, 0, 0, 0 },
        new[] { 0, 0, 0, 3, 0, 5, 0, 0, 0 },
        new[] { 0, 0, 9, 0, 0, 0, 0, 0, 0 },
        new[] { 0, 0, 0, 0, 0, 1, 0, 0, 0 },
        new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    },
    new[]
    {
        new[] { false, true, false, false, false, true, false, false, false },
        new[] { false, false, false, false, false, false, false, false, false },
        new[] { false, false, false, true, false, false, true, false, false },
        new[] { false, true, false, false, false, false, false, false, true },
        new[] { false, false, false, false, false, false, false, false, false },
        new[] { true, false, false, false, false, false, false, true, false },
        new[] { false, false, true, false, false, true, false, false, false },
        new[] { false, false, false, false, false, false, false, false, false },
        new[] { false, false, false, true, false, false, false, true, false }
    });
guriddoGrid.TryToSolve();
Console.WriteLine(guriddoGrid);