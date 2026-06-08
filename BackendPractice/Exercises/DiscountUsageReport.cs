using System;
using System.Collections.Generic;
using System.Linq;

public static class Exercise01_DiscountUsageReport
{
    public static void Run()
    {
        var redemptions = new List<DiscountRedemption>
        {
            new DiscountRedemption
            {
                RedemptionId = "R1",
                DiscountCode = "SAVE10",
                DiscountAmount = 10
            },
            new DiscountRedemption
            {
                RedemptionId = "R2",
                DiscountCode = "SAVE10",
                DiscountAmount = 10
            },
            new DiscountRedemption
            {
                RedemptionId = "R3",
                DiscountCode = "WELCOME20",
                DiscountAmount = 20
            },
            new DiscountRedemption
            {
                RedemptionId = "R4",
                DiscountCode = "SAVE10",
                DiscountAmount = 10
            },
            new DiscountRedemption
            {
                RedemptionId = "R5",
                DiscountCode = "WELCOME20",
                DiscountAmount = 20
            }
        };

        var service = new BillingReportService();

        var result = service.GetDiscountUsageSummary(redemptions);

        foreach (var item in result)
        {
            Console.WriteLine(
                $"DiscountCode: {item.DiscountCode}, " +
                $"TotalDiscountAmount: {item.TotalDiscountAmount}");
        }
    }
}

public class DiscountRedemption
{
    public required string RedemptionId { get; set; }
    public required string DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class DiscountUsageSummary
{
    public required string DiscountCode { get; set; }
    public decimal TotalDiscountAmount { get; set; }
}

public class BillingReportService
{
    public List<DiscountUsageSummary> GetDiscountUsageSummary(
        List<DiscountRedemption> redemptions)
    {
        // Your code here
        var discountCodeToDiscountAmount = new Dictionary<string, decimal>();

        foreach (var redemption in redemptions)
        {
            var key = redemption.DiscountCode;
            if (!discountCodeToDiscountAmount.ContainsKey(key))
            {
                discountCodeToDiscountAmount[key] = redemption.DiscountAmount;
            }
            else
            {
                discountCodeToDiscountAmount[key] += redemption.DiscountAmount;
            }
        }

        return [.. discountCodeToDiscountAmount
        .Select(x => new DiscountUsageSummary
        {
            DiscountCode = x.Key,
            TotalDiscountAmount = x.Value
        })];

        throw new NotImplementedException();
    }
}