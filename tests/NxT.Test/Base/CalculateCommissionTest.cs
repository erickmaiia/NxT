using NxT.Core.Contracts;
using NxT.Core.Models;
using NxT.Core.Models.Enums;

namespace NxT.Test.Base;

[TestFixture]
public class CalculateCommissionTest
{
    [Test]
    public void Test_NoCommission()
    {
        // Arrange
        var department = new Department
        {
            ID = 1,
            Name = "Electronics",
        };
        var seller = new Seller
        {
            ID = 1,
            Name = "David Mills",
            Email = "mills.seller@nxt.sales.com",
            BirthDate = new DateTime(1995, 12, 15),
            BaseSalary = 3500.0m,
            Department = department,
        };

        // Act
        var total = seller.CalculateSalary();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Empty);
            Assert.That(total, Is.EqualTo(3500.0m));
        });
    }

    [Test]
    public void Test_CommissionPerSale_WithOneSale()
    {
        var department = new Department
        {
            ID = 2,
            Name = "Clothing",
        };
        var seller = new Seller
        {
            ID = 2,
            Name = "John Doe",
            Email = "john-doe.seller@nxt.sales.com",
            BirthDate = new DateTime(1995, 12, 15),
            BaseSalary = 1050.0m,
            Commission = new CommissionPerSaleStrategy(0.2m),
            Department = department,
        };
        var record = new SalesRecord
        {
            ID = 1,
            Amount = 350.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now,
            Seller = seller,
        };

        seller.AddSales(record);
        var total = seller.CalculateSalary();

        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Not.Empty);
            Assert.That(total, Is.EqualTo(1050.0m + 0.2m * 350.0m));
        });
    }

    [Test]
    public void Test_CommissionPerSale_WithTwoSales()
    {
        var department = new Department
        {
            ID = 2,
            Name = "Clothing",
        };
        var seller = new Seller
        {
            ID = 2,
            Name = "John Doe",
            Email = "john-doe.seller@nxt.sales.com",
            BirthDate = new DateTime(1995, 12, 15),
            BaseSalary = 1050.0m,
            Commission = new CommissionPerSaleStrategy(0.2m),
            Department = department,
        };
        var record1 = new SalesRecord
        {
            ID = 1,
            Amount = 140.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now,
            Seller = seller,
        };
        var record2 = new SalesRecord
        {
            ID = 2,
            Amount = 220.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now.AddDays(-1),
            Seller = seller,
        };

        seller.AddSales(record1);
        seller.AddSales(record2);
        var total = seller.CalculateSalary();

        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Not.Empty);
            Assert.That(total, Is.EqualTo(1050.0m + 0.2m * (140.0m + 220.0m)));
        });
    }

    [Test]
    public void Test_CommissionPerGoal_WhenNotAchieved()
    {
        // Arrange
        var department = new Department
        {
            ID = 1,
            Name = "Electronics",
        };
        var seller = new Seller
        {
            ID = 3,
            Name = "Mark Harrison",
            Email = "mark-harr.seller@nxt.sales.com",
            BirthDate = new DateTime(2003, 10, 17),
            BaseSalary = 550.0m,
            Commission = new CommissionPerGoalStrategy(2500.0m, 0.15m, 0.13m),
            Department = department,
        };
        var record = new SalesRecord
        {
            ID = 1,
            Amount = 140.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now,
            Seller = seller,
        };

        seller.AddSales(record);
        var total = seller.CalculateSalary();

        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Not.Empty);
            Assert.That(total, Is.EqualTo(550.0m));
        });
    }
    
    [Test]
    public void Test_CommissionPerGoal_WhenAchieved()
    {
        // Arrange
        var department = new Department
        {
            ID = 1,
            Name = "Electronics",
        };
        var seller = new Seller
        {
            ID = 3,
            Name = "Mark Harrison",
            Email = "mark-harr.seller@nxt.sales.com",
            BirthDate = new DateTime(2003, 10, 17),
            BaseSalary = 550.0m,
            Commission = new CommissionPerGoalStrategy(2500.0m, 0.15m, 0.13m),
            Department = department,
        };
        var record1 = new SalesRecord
        {
            ID = 1,
            Amount = 140.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now,
            Seller = seller,
        };
        var record2 = new SalesRecord
        {
            ID = 2,
            Amount = 2450.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now.AddDays(-1),
            Seller = seller,
        };

        seller.AddSales(record1);
        seller.AddSales(record2);
        var total = seller.CalculateSalary();

        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Not.Empty);
            Assert.That(total, Is.EqualTo(2500.0m * 0.15m + 90.0m * 0.13m));
        });
    }

    [Test]
    public void Test_TieredCommission_OnTierOne()
    {
        var department = new Department
        {
            ID = 3,
            Name = "Books",
        };
        var seller = new Seller
        {
            ID = 4,
            Name = "Washington Elliot",
            Email = "eli.seller@nxt.sales.com",
            BirthDate = new DateTime(2010, 1, 15),
            BaseSalary = 1000.0m,
            Commission = new TieredCommissionStrategy
                (ranges: [10_000.0m, 13_000.0m], commissionRates: [0.1m, 0.15m]),
            Department = department,
        };
        var record1 = new SalesRecord
        {
            ID = 1,
            Amount = 80.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now,
            Seller = seller,
        };
        var record2 = new SalesRecord
        {
            ID = 2,
            Amount = 590.5m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now.AddDays(-1),
            Seller = seller,
        };

        seller.AddSales(record1);
        seller.AddSales(record2);
        var total = seller.CalculateSalary();

        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Not.Empty);
            Assert.That(total, Is.EqualTo(1000.0m + 0.1m * (80.0m + 590.5m)));
        });
    }
    
    [Test]
    public void Test_TieredCommission_OnTierTwo()
    {
        var department = new Department
        {
            ID = 4,
            Name = "TI",
        };
        var seller = new Seller
        {
            ID = 5,
            Name = "William Somerset",
            Email = "somerset.seller@nxt.sales.com",
            BirthDate = new DateTime(1995, 12, 15),
            BaseSalary = 1000.0m,
            Commission = new TieredCommissionStrategy
                (ranges: [10_000.0m, 13_000.0m], commissionRates: [0.1m, 0.15m]),
            Department = department,
        };
        var record1 = new SalesRecord
        {
            ID = 1,
            Amount = 9000.0m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now,
            Seller = seller,
        };
        var record2 = new SalesRecord
        {
            ID = 2,
            Amount = 5575.80m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now.AddDays(-1),
            Seller = seller,
        };
        var record3 = new SalesRecord
        {
            ID = 3,
            Amount = 8424.20m,
            Status = ESaleStatus.Billed,
            Date = DateTime.Now.AddDays(-2),
            Seller = seller,
        };


        seller.AddSales(record1);
        seller.AddSales(record2);
        seller.AddSales(record3);
        var total = seller.CalculateSalary();

        Assert.Multiple(() =>
        {
            Assert.That(seller.Sales, Is.Not.Empty);
            Assert.That(total, Is.EqualTo(1000.0m + 0.1m * (8480.20m + 1519.8m) + 0.15m * (9000.0m + 4000.0m)));
        });
    }
}