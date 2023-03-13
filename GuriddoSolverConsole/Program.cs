using System.Diagnostics;
using GuriddoSolver;

var numbers = new int[9][];
var immutableCellGrid = new bool[9][];
Console.WriteLine("请输入数字矩阵，其中0为没有数字：");
// 输入示例（天才一 2）：
// 034020876
// 000001080
// 080002000
// 070000000
// 000080000
// 000006040
// 000950000
// 000000000
// 002000000
for (var i = 0; i < 9; i++)
{
    var readLine = Console.ReadLine();
    if (readLine is null)
    {
        Console.WriteLine("行数不够");
        return;
    }

    if (readLine.Length != 9)
    {
        Console.WriteLine("输入行的字符长度错误");
        return;
    }

    if (readLine.Any(x => x is < '0' or > '9'))
    {
        Console.WriteLine("输入包含非数字字符");
        return;
    }

    numbers[i] = readLine.Select(x => x - '0').ToArray();
    Debug.Assert(numbers[i].Length == 9);
}

Console.WriteLine("请输入分割块矩阵，其中0为普通块，1为分割块：");
// 输入示例（天才一 2）：
// 100001000
// 001000101
// 000000010
// 000010000
// 000001000
// 001100000
// 100100000
// 000000011
// 000000000
for (var i = 0; i < 9; i++)
{
    var readLine = Console.ReadLine();
    if (readLine is null)
    {
        Console.WriteLine("行数不够");
        return;
    }

    if (readLine.Length != 9)
    {
        Console.WriteLine("输入行的字符长度错误");
        return;
    }

    if (readLine.Any(x => x is not '0' and not '1'))
    {
        Console.WriteLine("输入包含非数字字符");
        return;
    }

    immutableCellGrid[i] = readLine.Select(x => x is '1').ToArray();
    Debug.Assert(immutableCellGrid[i].Length == 9);
}

// 结果示例（天才一 2）：
// ( );[3];[4];[5];[2];( );[8];[7];[6]
// [7];[6];( );[2];[3];[1];( );[8];( )
// [5];[8];[6];[3];[4];[2];[7];( );[1]
// [9];[7];[8];[6];( );[3];[2];[5];[4]
// [6];[5];[7];[4];[8];( );[1];[3];[2]
// [8];[9];( );( );[7];[6];[3];[4];[5]
// ( );[2];[1];(9);[5];[7];[4];[6];[3]
// [2];[1];[3];[7];[6];[4];[5];( );( )
// [3];[4];[2];[8];[9];[5];[6];[1];[7]
var guriddoGrid = new GuriddoGrid(numbers,
    immutableCellGrid);
if (guriddoGrid.TryToSolve())
    Console.WriteLine(guriddoGrid);
else
    Console.WriteLine("没有找到解");