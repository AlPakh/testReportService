using ConversionService.Application.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConversionService.UnitTests.Application.Services
{
    [TestClass]
    public class ConversionCalculatorTests
    {
        [TestMethod]
        public void CalculateRatio_WhenPaymentsPositive_ReturnsExpectedRatio()
        {
            ConversionCalculator calculator = new();

            decimal? result = calculator.CalculateRatio(300, 15);

            Assert.AreEqual(20m, result);
        }

        [TestMethod]
        public void CalculateRatio_WhenPaymentsZero_ReturnsNull()
        {
            ConversionCalculator calculator = new();

            decimal? result = calculator.CalculateRatio(300, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CalculateRatio_WhenResultHasFraction_RoundsToSixDigits()
        {
            ConversionCalculator calculator = new();

            decimal? result = calculator.CalculateRatio(1, 3);

            Assert.AreEqual(0.333333m, result);
        }

        [TestMethod]
        public void CalculateRatio_WhenViewsZeroAndPaymentsPositive_ReturnsZero()
        {
            ConversionCalculator calculator = new();

            decimal? result = calculator.CalculateRatio(0, 5);

            Assert.AreEqual(0m, result);
        }
    }
}