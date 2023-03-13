using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace GuriddoSolver;

public record GuriddoGrid
{
    private InternalGuriddoGrid? _guriddoGrid;

    public GuriddoGrid(IReadOnlyList<IReadOnlyList<int>> cellGrid, IReadOnlyList<IReadOnlyList<bool>> immutableCellGrid)
    {
        var internalGuriddoGrid = new InternalGuriddoGrid(cellGrid, immutableCellGrid);
        _guriddoGrid = internalGuriddoGrid.TryLocal(0);
    }

    public override string ToString()
    {
        return _guriddoGrid.ToString();
    }
}

internal class InternalGuriddoGrid
{
    private readonly GuriddoCell[][] _cells;
    private readonly GuriddoValues[] _colArr;
    private readonly GuriddoRange[][] _colRanges;
    private readonly GuriddoValues[] _rowArr;
    private readonly GuriddoRange[][] _rowRanges;

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
                        var range = new GuriddoRange(rangeCells);
                        foreach (var cell in range.Cells) 
                            cell.RowRange = range;

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
                        var range = new GuriddoRange(rangeCells);
                        foreach (var cell in range.Cells) 
                            cell.ColRange = range;

                        ranges.Add(range);
                    }

                    start = row + 1;
                }

            colRanges[col] = ranges.ToArray();
        }

        _colRanges = colRanges.ToArray();

        Init();
    }
    
    private bool Init()
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
            rowHasUpdate[row] = false;
            colHasUpdate[col] = false;
            if (cell.Value == 0)
                return;
            var value = cell.PossibleValues;
            Debug.Assert((_rowArr[row] & value) == 0, "(_rowArr[row] & value) == 0");
            Debug.Assert((_colArr[col] & value) == 0, "(_colArr[col] & value) == 0");
            _rowArr[row] |= value;
            _colArr[col] |= value;
        }

        void UpdateRange(GuriddoRange[] ranges)
        {
            var needUpdate = true;
            while (needUpdate)
            {
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
                            var afterRemove = otherRange.PossibleValuesList.RemoveAll(x => !range.MustValues.IsDisjoint(x));
                            if (afterRemove != otherRange.PossibleValuesList)
                            {
                                otherRange.PossibleValuesList = afterRemove;
                                needUpdate = true;
                            }
                        }
                    }
                }

                foreach (var range in ranges)
                foreach (var cell in range.Cells)
                {
                    if (range.PossibleValues.Contains(cell.PossibleValues))
                        continue;
                    cell.PossibleValues &= range.PossibleValues;
                    var row = cell.Row;
                    var col = cell.Col;
                    rowHasUpdate[row] = false;
                    colHasUpdate[col] = false;
                    needUpdate = true;
                    if (cell.Value == 0)
                        continue;
                    var value = (GuriddoValues)(1 << cell.Value);
                    Debug.Assert((_rowArr[row] & value) == 0, "(_rowArr[row] & value) == 0");
                    Debug.Assert((_colArr[col] & value) == 0, "(_colArr[col] & value) == 0");
                    _rowArr[row] |= value;
                    _colArr[col] |= value;
                }
            }
        }
    }

    private int maxIndex = 0;

    public InternalGuriddoGrid? TryLocal(int deep)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var targetCell = _cells[row][col];
                if (targetCell.Immutable || targetCell.Value != 0)
                    continue;

                if (row * 9 + col > maxIndex)
                {
                    maxIndex = (row * 9 + col);
                    Console.WriteLine(maxIndex);
                }
                
                var rowMax = targetCell.RowRange.MaxValue;
                var rowMin = targetCell.RowRange.MinValue;
                var colMax = targetCell.ColRange.MaxValue;
                var colMin = targetCell.ColRange.MinValue;
                var values = ~(_rowArr[row] | _colArr[col]) & targetCell.RowRange.PossibleValues & targetCell.ColRange.PossibleValues;

                var rowLengthSubOne = targetCell.RowRange.Cells.Length - 1;
                var colLengthSubOne = targetCell.ColRange.Cells.Length - 1;
                var min = Math.Max(Math.Max(rowMax - rowLengthSubOne, colMax - colLengthSubOne), 1);
                var max = Math.Min(Math.Min(rowMin + rowLengthSubOne, colMin + colLengthSubOne), 9);
                for (int value = min; value <= max; value++)
                {
                    var valuesValue = (GuriddoValues)(1 << value);
                    if (values.Contains(valuesValue))
                    {
                        targetCell.Value = value;
                        _rowArr[row] |= valuesValue;
                        _colArr[col] |= valuesValue;
                        if (value > rowMax) targetCell.RowRange.MaxValue = value;
                        if (value > colMax) targetCell.ColRange.MaxValue = value;
                        if (value < rowMin) targetCell.RowRange.MinValue = value;
                        if (value < colMin) targetCell.ColRange.MinValue = value;

                        var result = TryLocal(deep + 1);
                        if (result is not null)
                        {
                            if (row == 3 && col == 8)
                            {}
                            return result;
                        }

                        targetCell.Value = 0;
                        _rowArr[row] ^= valuesValue;
                        _colArr[col] ^= valuesValue;
                        if (value > rowMax) targetCell.RowRange.MaxValue = rowMax;
                        if (value > colMax) targetCell.ColRange.MaxValue = colMax;
                        if (value < rowMin) targetCell.RowRange.MinValue = rowMin;
                        if (value < colMin) targetCell.ColRange.MinValue = colMin;
                    }
                }

                return null;
            }
        }

        return this;
    }

    public override string ToString()
    {
        return string.Join("\n", _cells.Select(rowCells => string.Join(";", rowCells.Select(cell => cell.ToString()))));
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

    public GuriddoCell(int value, int row, int col, bool immutable)
    {
        Value = value;
        if (value != 0)
            _possibleValues = (GuriddoValues)(1 << value);
        Row = row;
        Col = col;
        Immutable = immutable;
    }

    public int Value { get; set; }

    public int Row { get; }

    public int Col { get; }

    public readonly bool Immutable;

    public GuriddoValues PossibleValues
    {
        get => _possibleValues;
        set
        {
            if (_possibleValues == value)
                return;
            _possibleValues = value;
            if (_possibleValues.IsOnlyValue())
                Value = _possibleValues.ToValue();
        }
    }

    public GuriddoRange RowRange { get; set; } = null!;
    public GuriddoRange ColRange { get; set; } = null!;

    public override string ToString()
    {
        if (Value != 0)
            return Immutable ? $"({Value})" : $"[{Value}]";
        if (Immutable)
            return "( )";
        return PossibleValues.ToString();
    }
}

public class GuriddoRange
{
    private static readonly ImmutableArray<ImmutableArray<GuriddoValues>> AllPossibleValues;

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

        AllPossibleValues = allPossibleValues.Select(x => x.ToImmutableArray()).ToImmutableArray();
    }

    public GuriddoRange(IReadOnlyList<GuriddoCell> cells)
    {
        PossibleValuesList = AllPossibleValues[cells.Count];
        Cells = cells.ToImmutableArray();
    }
    
    public ImmutableArray<GuriddoValues> PossibleValuesList { get; set; }

    public ImmutableArray<GuriddoCell> Cells { get; }

    public GuriddoValues MustValues { get; private set; } = GuriddoValues.None;

    public GuriddoValues PossibleValues { get; private set; } = GuriddoValues.All;

    public int MaxValue { get; set; } = 1;
    public int MinValue { get; set; } = 9;

    public void Update()
    {
        var hasValues = GuriddoValues.None;
        var cellPossibleValues = GuriddoValues.None;
        foreach (var cell in Cells)
        {
            if (cell.Value != 0)
                hasValues |= cell.PossibleValues;
            cellPossibleValues |= cell.PossibleValues;
        }

        var valuesList = PossibleValuesList.RemoveAll(rvs =>
            !cellPossibleValues.Contains(rvs) ||
            !rvs.Contains(hasValues) ||
            Cells.Any(c => rvs.IsDisjoint(c.PossibleValues)));

        var rangePossibleValues = GuriddoValues.None;
        var mustValues = GuriddoValues.All;
        foreach (var values in valuesList)
        {
            rangePossibleValues |= values;
            mustValues &= values;
        }

        PossibleValues &= rangePossibleValues;
        MustValues |= mustValues;

        var guriddoCells = Cells.Where(x => x.Value != 0).ToList();
        if (guriddoCells.Count == 0)
        {
            MaxValue = 0;
            MinValue = 9;
        }
        else
        {
            MaxValue = guriddoCells.Max(x => x.Value);
            MinValue = guriddoCells.Min(x => x.Value);
        }
    }
}