using System.Collections.Immutable;
using System.Diagnostics;

namespace GuriddoSolver;

/// <summary>
///     Guriddo游戏棋盘
/// </summary>
public record GuriddoGrid
{
    private GuriddoCell[][] _cells;

    public GuriddoGrid(IReadOnlyList<IReadOnlyList<int>> cellGrid, IReadOnlyList<IReadOnlyList<bool>> immutableCellGrid)
    {
        _cells = new GuriddoCell[9][];
        for (var i = 0; i < 9; i++)
        {
            _cells[i] = new GuriddoCell[9];
            for (var j = 0; j < 9; j++) _cells[i][j] = new GuriddoCell(cellGrid[i][j], GuriddoValues.All, immutableCellGrid[i][j]);
        }
    }

    public void SimpleAnalysis()
    {
        var internalGuriddoGrid = new InternalGuriddoGrid(_cells);
        internalGuriddoGrid.SimpleAnalysis();
        _cells = internalGuriddoGrid.GetCells();
    }

    public bool TryToSolve()
    {
        var internalGuriddoGrid = new InternalGuriddoGrid(_cells);
        internalGuriddoGrid.SimpleAnalysis();
        if (internalGuriddoGrid.TryToSolve() is false)
            return false;
        _cells = internalGuriddoGrid.GetCells();
        return true;
    }

    public override string ToString()
    {
        return string.Join("\n", _cells.Select(rows => string.Join(";", rows.Select(c => c.ToString()))));
    }
}

/// <summary>
///     棋盘单元格
/// </summary>
public class GuriddoCell
{
    public GuriddoCell(int value, GuriddoValues possibleValues, bool immutable)
    {
        Value = value;
        PossibleValues = possibleValues;
        Immutable = immutable;
    }

    public int Value { get; }
    public GuriddoValues PossibleValues { get; }
    public bool Immutable { get; }

    public override string ToString()
    {
        if (Value != 0)
            return Immutable ? $"({Value})" : $"[{Value}]";
        if (Immutable)
            return "( )";
        return PossibleValues.ToString();
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

public static class GuriddoValuesExtensions
{
    /// <summary>
    ///     值枚举是否只有一个值
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static bool IsOnlyValue(this GuriddoValues values)
    {
        return ((int)values & ((int)values - 1)) == 0;
    }

    /// <summary>
    ///     将值枚举转换为一个值
    /// </summary>
    /// <param name="values"></param>
    /// <returns>如果<paramref name="values" />并非只有一个值，则返回0；否则返回仅有的值</returns>
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

    /// <summary>
    ///     值枚举是否包含另一个值枚举
    /// </summary>
    /// <param name="values"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool Contains(this GuriddoValues values, GuriddoValues other)
    {
        return (values & other) == other;
    }

    /// <summary>
    ///     值枚举是否与另一个值枚举不相交
    /// </summary>
    /// <param name="values"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool IsDisjoint(this GuriddoValues values, GuriddoValues other)
    {
        return (values & other) == 0;
    }

    /// <summary>
    ///     值枚举所包含的值的数量
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
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

/// <summary>
///     内部棋盘网格，用于分析求解
/// </summary>
file class InternalGuriddoGrid
{
    private readonly InternalGuriddoCell[][] _cells;
    private readonly GuriddoValues[] _colArr;
    private readonly InternalGuriddoRange[][] _colRanges;
    private readonly GuriddoValues[] _rowArr;
    private readonly InternalGuriddoRange[][] _rowRanges;

    public InternalGuriddoGrid(IReadOnlyList<IReadOnlyList<int>> cellGrid, IReadOnlyList<IReadOnlyList<bool>> immutableCellGrid)
    {
        if (cellGrid.Count != 9 || cellGrid.Any(x => x.Count != 9) || immutableCellGrid.Count != 9 || immutableCellGrid.Any(x => x.Count != 9))
            throw new ArgumentException("棋盘数组的大小必须是9*9");

        var cells = new InternalGuriddoCell[9][];
        for (var row = 0; row < 9; row++)
        {
            cells[row] = new InternalGuriddoCell[9];
            for (var col = 0; col < 9; col++) cells[row][col] = new InternalGuriddoCell(cellGrid[row][col], row, col, immutableCellGrid[row][col]);
        }

        _cells = cells;
        Init(_cells, out _rowArr, out _colArr, out _rowRanges, out _colRanges);
    }

    public InternalGuriddoGrid(IReadOnlyList<IReadOnlyList<GuriddoCell>> cells)
    {
        if (cells.Count != 9 || cells.Any(x => x.Count != 9))
            throw new ArgumentException("棋盘数组的大小必须是9*9");

        var cells1 = new InternalGuriddoCell[9][];
        for (var row = 0; row < 9; row++)
        {
            cells1[row] = new InternalGuriddoCell[9];
            for (var col = 0; col < 9; col++) cells1[row][col] = new InternalGuriddoCell(cells[row][col].Value, row, col, cells[row][col].Immutable);
        }

        _cells = cells1;
        Init(_cells, out _rowArr, out _colArr, out _rowRanges, out _colRanges);
    }

    private static GuriddoValues[] Init(InternalGuriddoCell[][] cells, out GuriddoValues[] rowArr, out GuriddoValues[] colArr,
        out InternalGuriddoRange[][] rowRanges,
        out InternalGuriddoRange[][] colRanges)
    {
        rowArr = new GuriddoValues[9];
        colArr = new GuriddoValues[9];

        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
            if (cells[row][col].Value != 0)
            {
                var values = (GuriddoValues)(1 << cells[row][col].Value);
                Debug.Assert((rowArr[row] & values) == 0, "(_rowArr[row] ^= value) == 0");
                Debug.Assert((colArr[col] & values) == 0, "(_colArr[col] ^= value) == 0");
                rowArr[row] ^= values;
                colArr[col] ^= values;
            }

        rowRanges = new InternalGuriddoRange[9][];
        for (var row = 0; row < 9; row++)
        {
            List<InternalGuriddoRange> ranges = new();
            var start = 0;
            for (var col = 0; col <= 9; col++)
                if (col == 9 || cells[row][col].Immutable)
                {
                    if (col != start)
                    {
                        var rangeCells = cells[row][start..col];
                        var range = new InternalGuriddoRange(rangeCells);
                        foreach (var cell in range.Cells)
                            cell.RowRange = range;

                        ranges.Add(range);
                    }

                    start = col + 1;
                }

            rowRanges[row] = ranges.ToArray();
        }

        colRanges = new InternalGuriddoRange[9][];
        for (var col = 0; col < 9; col++)
        {
            List<InternalGuriddoRange> ranges = new();
            var start = 0;
            for (var row = 0; row <= 9; row++)
                if (row == 9 || cells[row][col].Immutable)
                {
                    if (row != start)
                    {
                        var rangeCells = cells[start..row].Select(arr => arr[col]).ToArray();
                        var range = new InternalGuriddoRange(rangeCells);
                        foreach (var cell in range.Cells)
                            cell.ColRange = range;

                        ranges.Add(range);
                    }

                    start = row + 1;
                }

            colRanges[col] = ranges.ToArray();
        }

        return rowArr;
    }

    /// <summary>
    ///     简单分析，先将简单的分析完成，可以简化后续的测试
    /// </summary>
    public void SimpleAnalysis()
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

        void UpdateRange(InternalGuriddoRange[] ranges)
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

    /// <summary>
    ///     测试当前局面是否有解，如果有解，棋盘将会设置为结果并返回 true
    /// </summary>
    /// <returns></returns>
    public bool TryToSolve()
    {
        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            var targetCell = _cells[row][col];
            if (targetCell.Immutable || targetCell.Value != 0)
                continue;

            var rowMax = targetCell.RowRange.MaxValue;
            var rowMin = targetCell.RowRange.MinValue;
            var colMax = targetCell.ColRange.MaxValue;
            var colMin = targetCell.ColRange.MinValue;
            var values = ~(_rowArr[row] | _colArr[col]) & targetCell.RowRange.PossibleValues & targetCell.ColRange.PossibleValues;

            var rowLengthSubOne = targetCell.RowRange.Cells.Length - 1;
            var colLengthSubOne = targetCell.ColRange.Cells.Length - 1;
            var min = Math.Max(Math.Max(rowMax - rowLengthSubOne, colMax - colLengthSubOne), 1);
            var max = Math.Min(Math.Min(rowMin + rowLengthSubOne, colMin + colLengthSubOne), 9);
            for (var value = min; value <= max; value++)
            {
                var valuesValue = (GuriddoValues)(1 << value);
                if (!values.Contains(valuesValue))
                    continue;
                targetCell.Value = value;
                _rowArr[row] |= valuesValue;
                _colArr[col] |= valuesValue;
                // 调整行、列区段的最大最小值，方便后续限制
                if (value > rowMax) targetCell.RowRange.MaxValue = value;
                if (value > colMax) targetCell.ColRange.MaxValue = value;
                if (value < rowMin) targetCell.RowRange.MinValue = value;
                if (value < colMin) targetCell.ColRange.MinValue = value;

                // 如果填这个值是正确的，则直接返回
                if (TryToSolve())
                    return true;

                // 如果填这个值不正确，需要恢复到原来的状态
                targetCell.Value = 0;
                _rowArr[row] ^= valuesValue;
                _colArr[col] ^= valuesValue;
                if (value > rowMax) targetCell.RowRange.MaxValue = rowMax;
                if (value > colMax) targetCell.ColRange.MaxValue = colMax;
                if (value < rowMin) targetCell.RowRange.MinValue = rowMin;
                if (value < colMin) targetCell.ColRange.MinValue = colMin;
            }

            // 如果所有值都不能填，说明这个解法是错误的
            return false;
        }

        // 如果没有空位了，说明已经填完了，这个解法是正确的
        return true;
    }

    /// <summary>
    ///     获取所有棋盘单元格
    /// </summary>
    /// <returns></returns>
    public GuriddoCell[][] GetCells()
    {
        var result = new GuriddoCell[9][];
        for (var row = 0; row < 9; row++)
        {
            result[row] = new GuriddoCell[9];
            for (var col = 0; col < 9; col++)
            {
                var internalCell = _cells[row][col];
                result[row][col] = new GuriddoCell(internalCell.Value, internalCell.PossibleValues, internalCell.Immutable);
            }
        }

        return result;
    }

    public override string ToString()
    {
        return string.Join("\n", _cells.Select(rowCells => string.Join(";", rowCells.Select(cell => cell.ToString()))));
    }
}

/// <summary>
///     内部棋盘单元格，用于分析求解
/// </summary>
file class InternalGuriddoCell
{
    public readonly bool Immutable;
    private GuriddoValues _possibleValues = GuriddoValues.All;

    public InternalGuriddoCell(int value, int row, int col, bool immutable)
    {
        Value = value;
        if (value != 0)
            _possibleValues = (GuriddoValues)(1 << value);
        Row = row;
        Col = col;
        Immutable = immutable;
    }

    /// <summary>
    ///     单元格的值。如果没有值，为0。
    /// </summary>
    public int Value { get; set; }

    public int Row { get; }

    public int Col { get; }

    /// <summary>
    ///     单元格可能可以设置的值
    /// </summary>
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

    public InternalGuriddoRange RowRange { get; set; } = null!;
    public InternalGuriddoRange ColRange { get; set; } = null!;

    public override string ToString()
    {
        if (Value != 0)
            return Immutable ? $"({Value})" : $"[{Value}]";
        if (Immutable)
            return "( )";
        return PossibleValues.ToString();
    }
}

/// <summary>
///     内部棋盘区段，用于分析求解
/// </summary>
file class InternalGuriddoRange
{
    private static readonly ImmutableArray<ImmutableArray<GuriddoValues>> AllPossibleValues;

    static InternalGuriddoRange()
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

    public InternalGuriddoRange(IReadOnlyList<InternalGuriddoCell> cells)
    {
        PossibleValuesList = AllPossibleValues[cells.Count];
        Cells = cells.ToImmutableArray();
    }

    /// <summary>
    ///     可能允许的值的数组
    /// </summary>
    public ImmutableArray<GuriddoValues> PossibleValuesList { get; set; }

    /// <summary>
    ///     包含的单元格的数组
    /// </summary>
    public ImmutableArray<InternalGuriddoCell> Cells { get; }

    /// <summary>
    ///     区段内必须存在的值的枚举
    /// </summary>
    public GuriddoValues MustValues { get; private set; } = GuriddoValues.None;

    /// <summary>
    ///     区段内可能存在的值的枚举
    /// </summary>
    public GuriddoValues PossibleValues { get; private set; } = GuriddoValues.All;

    /// <summary>
    ///     当前已有的值的最大值
    /// </summary>
    public int MaxValue { get; set; } = 1;

    /// <summary>
    ///     当前已有的值的最小值
    /// </summary>
    public int MinValue { get; set; } = 9;

    /// <summary>
    ///     根据单元格内容更新属性
    /// </summary>
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