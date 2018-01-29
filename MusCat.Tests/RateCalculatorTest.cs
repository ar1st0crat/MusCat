using System;
using System.Collections.Generic;
using MusCat.Core.Entities;
using MusCat.Core.Services;
using NUnit.Framework;

namespace MusCat.Tests
{
    [TestFixture]
    public class RateCalculatorTest
    {
        readonly RateCalculator _rateCalculator = new RateCalculator();

        [Test]
        public void TestCalculateRateForNoAlbums()
        {
            // ARRANGE
            var rates = new List<byte?>();
            // ACT
            var rate = _rateCalculator.Calculate(rates);
            // ASSERT
            Assert.That(rate, Is.Null);
        }

        [Test]
        public void TestCalculateSingleRate()
        {
            // ARRANGE
            var rates = new byte?[] { 5 };
            // ACT
            var rate = _rateCalculator.Calculate(rates);
            // ASSERT
            Assert.That(rate, Is.EqualTo(5));
        }

        [Test]
        public void TestCalculateDoubleRate()
        {
            // ARRANGE
            var rates = new byte? [] { 5, 10 };
            // ACT
            var rate = _rateCalculator.Calculate(rates);
            // ASSERT
            Assert.That(rate, Is.EqualTo(8));
        }

        [Test]
        public void TestCalculateRate()
        {
            // ARRANGE
            var rates = new byte? [] { 1, 10, 2, 4 };
            // ACT
            var rate = _rateCalculator.Calculate(rates);
            // ASSERT
            Assert.That(rate, Is.EqualTo(3));
        }

        [Test]
        public void TestCalculateRateNullCollection()
        {
            // ARRANGE, ACT, ASSERT
            Assert.Throws<ArgumentNullException>(() => _rateCalculator.Calculate(null));
        }

        [Test]
        public void TestCalculateSomeNullRates()
        {
            // ARRANGE
            var rates = new byte? [] { 1, null, 2, 4 };
            // ACT
            var rate = _rateCalculator.Calculate(rates);
            // ASSERT
            Assert.That(rate, Is.EqualTo(2));
        }
    }
}
