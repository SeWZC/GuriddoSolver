# GuriddoSolver

一个用来求解Guriddo游戏的程序。  
A program for solving the Guriddo game.

## 用法/Usage

### 命令行/Command Line

首先需要安装[.NET](https://dotnet.microsoft.com/zh-cn/download)运行时或SDK（至少为.NET 6）。  
First you need to install [.NET](https://dotnet.microsoft.com/zh-cn/download) Runtime or SDK (.NET 6 at least)

然后下载代码，并进入文件夹```GuriddoSolver\GuriddoSolverConsole```，使用```dotnet build```进行编译，然后使用```dotnet run```运行。  
Then download the code, and enter the folder ```GuriddoSolver\GuriddoSolverConsole```, use ```dotnet build``` to compile, and then use ```dotnet run``` to run.

最后输入数字矩阵和分隔符矩阵即可得到结果，如果要再次运行程序，可以再次使用```dotnet run```运行。  
Finally, enter the number matrix and separator matrix to get the result. If you want to run the program again, you can use ```dotnet run``` to run it again.

示例（天才一 2）：  
Example(Genius 一 2):

> 请输入数字矩阵，其中0为没有数字：  
> 034020876  
> 000001080  
> 080002000  
> 070000000  
> 000080000  
> 000006040  
> 000950000  
> 000000000  
> 002000000  
> 请输入分割块矩阵，其中0为普通块，1为分割块：  
> 100001000  
> 001000101  
> 000000010  
> 000010000  
> 000001000  
> 001100000  
> 100100000  
> 000000011  
> 000000000  
> ( );[3];[4];[5];[2];( );[8];[7];[6]  
> [7];[6];( );[2];[3];[1];( );[8];( )  
> [5];[8];[6];[3];[4];[2];[7];( );[1]  
> [9];[7];[8];[6];( );[3];[2];[5];[4]  
> [6];[5];[7];[4];[8];( );[1];[3];[2]  
> [8];[9];( );( );[7];[6];[3];[4];[5]  
> ( );[2];[1];(9);[5];[7];[4];[6];[3]  
> [2];[1];[3];[7];[6];[4];[5];( );( )  
> [3];[4];[2];[8];[9];[5];[6];[1];[7]  
