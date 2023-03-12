using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GuriddoSolver.Tests;

[TestClass]
public class GuriddoGuidTests
{
    [TestMethod]
    public void GuriddoGuidTest()
    {
        //new GuriddoGrid(new[]
        //    {
        //        new[] { 0, 1, 5, 0, 3, 0, 7, 0, 8 },
        //        new[] { 0, 0, 0, 0, 0, 6, 0, 0, 7 },
        //        new[] { 0, 0, 3, 4, 0, 0, 8, 0, 0 },
        //        new[] { 3, 0, 2, 0, 0, 8, 0, 1, 0 },
        //        new[] { 2, 0, 0, 0, 0, 7, 0, 0, 0 },
        //        new[] { 8, 9, 0, 0, 7, 0, 0, 0, 4 },
        //        new[] { 0, 6, 0, 0, 0, 2, 0, 0, 3 },
        //        new[] { 6, 7, 0, 8, 4, 0, 0, 3, 0 },
        //        new[] { 7, 0, 0, 5, 1, 3, 4, 2, 9 }
        //    },
        //    new[]
        //    {
        //        new[] { true, false, false, false, false, false, true, false, false },
        //        new[] { true, false, false, false, true, false, false, false, false },
        //        new[] { false, false, false, false, true, false, false, false, false },
        //        new[] { false, false, false, true, false, false, true, false, true },
        //        new[] { false, false, true, false, false, false, true, false, false },
        //        new[] { true, false, true, false, false, true, false, false, false },
        //        new[] { false, false, false, false, true, false, false, false, false },
        //        new[] { false, false, false, false, true, false, false, false, true },
        //        new[] { false, false, true, false, false, false, false, false, true }
        //    });
        new GuriddoGrid(new[]
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
    }

    [TestMethod]
    public void InitTest()
    {
        Assert.Fail();
    }
}