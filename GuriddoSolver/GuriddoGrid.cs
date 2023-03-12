using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace GuriddoSolver;

public record GuriddoGrid
{
    private readonly ImmutableArray<ImmutableArray<GuriddoCell>> _cells;
    private readonly ImmutableArray<GuriddoValues> _colArr;
    private readonly ImmutableArray<ImmutableArray<GuriddoRange>> _colRanges;
    private readonly ImmutableArray<GuriddoValues> _rowArr;
    private readonly ImmutableArray<ImmutableArray<GuriddoRange>> _rowRanges;

    public GuriddoGrid(IReadOnlyList<IReadOnlyList<int>> cellGrid, IReadOnlyList<IReadOnlyList<bool>> immutableCellGrid)
    {
        var internalGuriddoGrid = new InternalGuriddoGrid(cellGrid, immutableCellGrid);
        var guriddoGrid = internalGuriddoGrid.Try(0);
    }

    public override string ToString()
    {
        return string.Join("\n", _cells.Select(rowCells => string.Join(", ", rowCells.Select(cell => cell.ToString()))));
    }
}

file class InternalGuriddoGrid
{
    private readonly GuriddoCell[][] _cells;
    private readonly GuriddoValues[] _colArr;
    private readonly GuriddoRange[][] _colRanges;
    private readonly GuriddoValues[] _rowArr;
    private readonly GuriddoRange[][] _rowRanges;
    private int _valueZero = 0;

    private InternalGuriddoGrid(InternalGuriddoGrid oldGrid)
    {
        _cells = new GuriddoCell[9][];
        for (var row = 0; row < 9; row++)
        {
            _cells[row] = new GuriddoCell[9];
            for (var col = 0; col < 9; col++)
                _cells[row][col] = new GuriddoCell(oldGrid._cells[row][col]);
        }

        _colArr = new GuriddoValues[9];
        _rowArr = new GuriddoValues[9];
        oldGrid._colArr.CopyTo(_colArr, 0);
        oldGrid._rowArr.CopyTo(_rowArr, 0);

        _colRanges = new GuriddoRange[9][];
        _rowRanges = new GuriddoRange[9][];
        for (var col = 0; col < 9; col++)
        {
            var ranges = oldGrid._colRanges[col];
            _colRanges[col] = new GuriddoRange[ranges.Length];
            for (var i = 0; i < ranges.Length; i++)
            {
                var oldRange = ranges[i];
                _colRanges[col][i] = new GuriddoRange(oldRange, _cells[oldRange.Start..oldRange.End].Select(x => x[col]).ToList());
            }
        }
        for (var row = 0; row < 9; row++)
        {
            var ranges = oldGrid._rowRanges[row];
            _rowRanges[row] = new GuriddoRange[ranges.Length];
            for (var i = 0; i < ranges.Length; i++)
            {
                var oldRange = ranges[i];
                _rowRanges[row][i] = new GuriddoRange(oldRange, _cells[row][oldRange.Start..oldRange.End]);
            }
        }
    }

    public InternalGuriddoGrid(IReadOnlyList<IReadOnlyList<int>> cellGrid, IReadOnlyList<IReadOnlyList<bool>> immutableCellGrid)
    {
        if (cellGrid.Count != 9 || cellGrid.Any(x => x.Count != 9) || immutableCellGrid.Count != 9 || immutableCellGrid.Any(x => x.Count != 9))
            throw new ArgumentException("棋盘数组的大小必须是9*9");

        var cells = new GuriddoCell[9][];
        for (var row = 0; row < 9; row++)
        {
            cells[row] = new GuriddoCell[9];
            for (var col = 0; col < 9; col++) cells[row][col] = new GuriddoCell(cellGrid[row][col], row, col, immutableCellGrid[row][col]);
        }

        _cells = cells.Select(row => row.ToArray()).ToArray();
        _valueZero = cells.Sum(x => x.Count(y => y is { Immutable: false, Value: 0 }));

        var rowArr = new GuriddoValues[9];
        var colArr = new GuriddoValues[9];

        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
            if (_cells[row][col].Value != 0)
            {
                var value = (GuriddoValues)(1 << _cells[row][col].Value);
                Debug.Assert((rowArr[row] & value) == 0, "(_rowArr[row] ^= value) == 0");
                Debug.Assert((colArr[col] & value) == 0, "(_colArr[col] ^= value) == 0");
                rowArr[row] ^= value;
                colArr[col] ^= value;
            }

        _rowArr = rowArr.ToArray();
        _colArr = colArr.ToArray();

        var rowRanges = new GuriddoRange[9][];
        for (var row = 0; row < 9; row++)
        {
            List<GuriddoRange> ranges = new();
            var start = 0;
            for (var col = 0; col <= 9; col++)
                if (col == 9 || _cells[row][col].Immutable)
                {
                    if (col != start)
                    {
                        var rangeCells = _cells[row][start..col];
                        var range = new GuriddoRange(rangeCells, start, col);

                        ranges.Add(range);
                    }

                    start = col + 1;
                }

            rowRanges[row] = ranges.ToArray();
        }

        _rowRanges = rowRanges.ToArray();

        var colRanges = new GuriddoRange[9][];
        for (var col = 0; col < 9; col++)
        {
            List<GuriddoRange> ranges = new();
            var start = 0;
            for (var row = 0; row <= 9; row++)
                if (row == 9 || _cells[row][col].Immutable)
                {
                    if (row != start)
                    {
                        var rangeCells = _cells[start..row].Select(arr => arr[col]).ToArray();
                        var range = new GuriddoRange(rangeCells, start, row);

                        ranges.Add(range);
                    }

                    start = row + 1;
                }

            colRanges[col] = ranges.ToArray();
        }

        _colRanges = colRanges.ToArray();

        Loop();
    }

    public InternalGuriddoGrid? Try(int deep)
    {
        var minRow = -1;
        var minCol = -1;
        var minCount = 10;

        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            var cell = _cells[row][col];
            if (cell.Immutable || cell.Value != 0)
                continue;
            var count = cell.PossibleValues.Count();
            if (count < minCount)
            {
                minRow = row;
                minCol = col;
                minCount = count;
            }
        }

        if (minCount == 10)
            return this;

        var targetCell = _cells[minRow][minCol];
        
        if (deep < 10)
        {
            Console.WriteLine("1");
            var tasks = Enumerable.Range(1, 9)
                .Where(value => targetCell.PossibleValues.Contains((GuriddoValues)(1 << value)))
                .Select(value => Task.Run(() => TryValue(deep, minRow, minCol, value)))
                .ToArray();
            Task.WaitAll(tasks);

            return tasks.Select(t => t.Result).FirstOrDefault(t => t is not null);
        }
        else
        {
            for (var value = 1; value <= 9; value++)
            {
                if (targetCell.PossibleValues.Contains((GuriddoValues)(1 << value)))
                {
                    var endGrid = TryValue(deep, minRow, minCol, value);
                    if (endGrid is not null)
                        return endGrid;
                }
            }

            return null;
        }
    }

    private InternalGuriddoGrid? TryValue(int deep, int row, int col, int value)
    {
        if (_cells[row][col].PossibleValues.Contains((GuriddoValues)(1 << value)))
        {
            var newGrid = new InternalGuriddoGrid(this);
            newGrid._cells[row][col].PossibleValues = (GuriddoValues)(1 << value);
            newGrid.Loop();
            var internalGuriddoGrid = newGrid.Try(deep + 1);
            if (internalGuriddoGrid is not null)
            {
                return internalGuriddoGrid;
            }
        }

        return null;
    }

    private bool Loop()
    {
        var rowHasUpdate = new bool[9];
        var colHasUpdate = new bool[9];

        while (!(rowHasUpdate.All(x => x) && colHasUpdate.All(x => x)))
        {
            for (var row = 0; row < 9; row++)
            {
                if (rowHasUpdate[row])
                    continue;
                rowHasUpdate[row] = true;
                for (var col = 0; col < 9; col++)
                    UpdateCell(row, col);
                UpdateRange(_rowRanges[row]);
            }

            for (var col = 0; col < 9; col++)
            {
                if (colHasUpdate[col])
                    continue;
                colHasUpdate[col] = true;
                for (var row = 0; row < 9; row++)
                    UpdateCell(row, col);
                UpdateRange(_colRanges[col]);
            }
        }

        return false;

        void UpdateCell(int row, int col)
        {
            if (_cells[row][col].Immutable)
                return;
            var cell = _cells[row][col];
            if (cell.Value != 0)
                return;
            var newPossibleValues = GuriddoValues.All ^ (_rowArr[row] | _colArr[col]);
            if (newPossibleValues.Contains(cell.PossibleValues))
                return;
            cell.PossibleValues &= newPossibleValues;
            if (cell.Value == 0)
                return;
            var value = cell.PossibleValues;
            Debug.Assert((_rowArr[row] & value) == 0, "(_rowArr[row] & value) == 0");
            Debug.Assert((_colArr[col] & value) == 0, "(_colArr[col] & value) == 0");
            _rowArr[row] |= value;
            _colArr[col] |= value;
            rowHasUpdate[row] = false;
            colHasUpdate[col] = false;
        }

        void UpdateRange(GuriddoRange[] ranges)
        {
            var needUpdate = true;
            while (needUpdate)
            {
                needUpdate = false;
                foreach (var range in ranges)
                {
                    range.Update();
                    foreach (var otherRange in ranges)
                    {
                        if (otherRange == range)
                            continue;
                        var removeCount = otherRange.PossibleValuesList.RemoveAll(x => !range.MustValues.IsDisjoint(x));
                        if (removeCount > 0)
                            needUpdate = true;
                    }
                }
            }

            foreach (var range in ranges)
            foreach (var cell in range.Cells)
            {
                if (range.PossibleValues.Contains(cell.PossibleValues))
                    continue;
                cell.PossibleValues &= range.PossibleValues;
                if (cell.Value == 0)
                    continue;
                rowHasUpdate[cell.Row] = true;
                colHasUpdate[cell.Col] = true;
            }
        }
    }
}

[Flags]
public enum GuriddoValues
{
    None = 0,
    V1 = 1 << 1,
    V2 = 1 << 2,
    V3 = 1 << 3,
    V4 = 1 << 4,
    V5 = 1 << 5,
    V6 = 1 << 6,
    V7 = 1 << 7,
    V8 = 1 << 8,
    V9 = 1 << 9,
    All = 0b1111111110
}

file static class GuriddoValuesMethod
{
    public static bool IsOnlyValue(this GuriddoValues values)
    {
        return ((int)values & ((int)values - 1)) == 0;
    }

    public static int ToValue(this GuriddoValues values)
    {
        return values switch
        {
            GuriddoValues.V1 => 1,
            GuriddoValues.V2 => 2,
            GuriddoValues.V3 => 3,
            GuriddoValues.V4 => 4,
            GuriddoValues.V5 => 5,
            GuriddoValues.V6 => 6,
            GuriddoValues.V7 => 7,
            GuriddoValues.V8 => 8,
            GuriddoValues.V9 => 9,
            _ => 0
        };
    }

    public static bool Contains(this GuriddoValues values, GuriddoValues other)
    {
        return (values & other) == other;
    }

    public static bool IsDisjoint(this GuriddoValues values, GuriddoValues other)
    {
        return (values & other) == 0;
    }

    public static int Count(this GuriddoValues values)
    {
        var count = 0;
        while (values > 0)
        {
            values &= values - 1;
            count += 1;
        }

        return count;
    }
}

public class GuriddoCell
{
    private GuriddoValues _possibleValues = GuriddoValues.All;
    private int _value;

    internal GuriddoCell(GuriddoCell oldCell)
    {
        _value = oldCell._value;
        _possibleValues = oldCell._possibleValues;
        Row = oldCell.Row;
        Col = oldCell.Col;
        Immutable = oldCell.Immutable;
    }

    public GuriddoCell(int value, int row, int col, bool immutable)
    {
        Value = value;
        Row = row;
        Col = col;
        Immutable = immutable;
    }

    public int Value
    {
        get => _value;
        init
        {
            _value = value;
            if (value != 0)
                _possibleValues = (GuriddoValues)(1 << value);
        }
    }

    public int Row { get; }

    public int Col { get; }

    public bool Immutable { get; }

    public GuriddoValues PossibleValues
    {
        get => _possibleValues;
        set
        {
            if (_possibleValues == value)
                return;
            _possibleValues = value;
            if (_possibleValues.IsOnlyValue())
                _value = _possibleValues.ToValue();
        }
    }

    public override string ToString()
    {
        if (Value != 0)
            return Immutable ? $"({Value})" : $"[{Value}]";
        if (Immutable)
            return "()";
        return PossibleValues.ToString();
    }
}

public class GuriddoRange
{
    private static readonly ImmutableArray<ImmutableList<GuriddoValues>> AllPossibleValues;
    private readonly List<GuriddoCell> _cells;

    static GuriddoRange()
    {
        var allPossibleValues = new GuriddoValues[10][];
        allPossibleValues[0] = Array.Empty<GuriddoValues>();
        for (var count = 1; count <= 9; count++)
        {
            allPossibleValues[count] = new GuriddoValues[9 - count + 1];
            for (var end = count; end <= 9; end++)
            {
                var sum = 0;
                for (var value = end - count + 1; value <= end; value++)
                    sum |= 1 << value;
                allPossibleValues[count][end - count] = (GuriddoValues)sum;
            }
        }

        AllPossibleValues = allPossibleValues.Select(x => x.ToImmutableList()).ToImmutableArray();
    }

    public GuriddoRange(GuriddoRange oldRange, IReadOnlyList<GuriddoCell> cells)
    {
        _cells = cells.ToList();
        Start = oldRange.Start;
        End = oldRange.End;
        PossibleValuesList = new List<GuriddoValues>(oldRange.PossibleValuesList);
        MustValues = oldRange.MustValues;
        PossibleValues = oldRange.PossibleValues;
    }

    public GuriddoRange(IReadOnlyList<GuriddoCell> cells, int start, int end)
    {
        Start = start;
        End = end;
        PossibleValuesList = AllPossibleValues[cells.Count].ToList();
        Cells = cells.ToList();
    }

    public int Start { get; }
    public int End { get; }

    public List<GuriddoValues> PossibleValuesList { get; }

    public GuriddoValues MustValues { get; private set; } = GuriddoValues.None;

    public GuriddoValues PossibleValues { get; private set; } = GuriddoValues.All;

    public List<GuriddoCell> Cells
    {
        get => _cells;
        init
        {
            _cells = value;
            Update();
        }
    }

    public void Update()
    {
        var hasValues = GuriddoValues.None;
        var cellPossibleValues = GuriddoValues.None;
        foreach (var cell in _cells)
        {
            if (cell.Value != 0)
                hasValues |= cell.PossibleValues;
            cellPossibleValues |= cell.PossibleValues;
        }

        PossibleValuesList.RemoveAll(rvs =>
            !cellPossibleValues.Contains(rvs) ||
            !rvs.Contains(hasValues) ||
            _cells.Any(c => rvs.IsDisjoint(c.PossibleValues)));

        var rangePossibleValues = GuriddoValues.None;
        var mustValues = GuriddoValues.All;
        foreach (var values in PossibleValuesList)
        {
            rangePossibleValues |= values;
            mustValues &= values;
        }

        PossibleValues &= rangePossibleValues;
        MustValues |= mustValues;
    }
}