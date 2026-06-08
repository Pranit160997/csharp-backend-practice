using System;
using System.Collections.Generic;
using System.Linq;

public static class Exercise03_TotalInvoiceAmountByCustomer
{
    public static void Run()
    {
        var invoices = new List<InvoiceAmountRecord>
        {
            new InvoiceAmountRecord { InvoiceId = "INV-1", CustomerId = "C1", Amount = 100 },
            new InvoiceAmountRecord { InvoiceId = "INV-2", CustomerId = "C1", Amount = 200 },
            new InvoiceAmountRecord { InvoiceId = "INV-3", CustomerId = "C2", Amount = 300 },
            new InvoiceAmountRecord { InvoiceId = "INV-4", CustomerId = "C3", Amount = 400 },
            new InvoiceAmountRecord { InvoiceId = "INV-5", CustomerId = "C2", Amount = 50 }
        };

        var service = new InvoiceAmountReportService();

        var result = service.GetTotalInvoiceAmountByCustomer(invoices);

        foreach (var item in result)
        {
            Console.WriteLine(
                $"CustomerId: {item.CustomerId}, " +
                $"TotalInvoiced: {item.TotalInvoiced}");
        }
    }
}

public class InvoiceAmountRecord
{
    public required string InvoiceId { get; set; }
    public required string CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class CustomerInvoiceAmountSummary
{
    public required string CustomerId { get; set; }
    public decimal TotalInvoiced { get; set; }
}

public class InvoiceAmountReportService
{
    public List<CustomerInvoiceAmountSummary> GetTotalInvoiceAmountByCustomer(
        List<InvoiceAmountRecord> invoices)
    {
        // Your code here
        var customerIdToTotalAmount = new Dictionary<string, decimal>();

        foreach (var invoice in invoices)
        {
            var key = invoice.CustomerId;

            if (!customerIdToTotalAmount.ContainsKey(key))
            {
                customerIdToTotalAmount[key] = invoice.Amount;
            }
            else
            {
                customerIdToTotalAmount[key] += invoice.Amount;
            }
        }

        return customerIdToTotalAmount
        .Select(x => new CustomerInvoiceAmountSummary
        {
            CustomerId = x.Key,
            TotalInvoiced = x.Value
        })
        .ToList();

        throw new NotImplementedException();
    }
}