using System;
using System.Collections.Generic;
using System.Linq;

public static class Exercise02_CustomerInvoiceCountReport
{
    public static void Run()
    {
        var invoices = new List<Invoice>
        {
            new Invoice
            {
                InvoiceId = "INV-1",
                CustomerId = "C1",
                Amount = 100
            },
            new Invoice
            {
                InvoiceId = "INV-2",
                CustomerId = "C1",
                Amount = 200
            },
            new Invoice
            {
                InvoiceId = "INV-3",
                CustomerId = "C2",
                Amount = 300
            },
            new Invoice
            {
                InvoiceId = "INV-4",
                CustomerId = "C3",
                Amount = 400
            },
            new Invoice
            {
                InvoiceId = "INV-5",
                CustomerId = "C2",
                Amount = 500
            }
        };

        var service = new InvoiceReportService();

        var result = service.GetInvoiceCountsByCustomer(invoices);

        foreach (var item in result)
        {
            Console.WriteLine(
                $"CustomerId: {item.CustomerId}, " +
                $"InvoiceCount: {item.InvoiceCount}");
        }
    }
}

public class Invoice
{
    public required string InvoiceId { get; set; }
    public required string CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class CustomerInvoiceCountSummary
{
    public required string CustomerId { get; set; }
    public int InvoiceCount { get; set; }
}

public class InvoiceReportService
{
    public List<CustomerInvoiceCountSummary> GetInvoiceCountsByCustomer(
        List<Invoice> invoices)
    {
        // Your code here
        var customerIdToInvoiceCount = new Dictionary<string, int>();

        foreach (var invoice in invoices)
        {
            var key = invoice.CustomerId;

            if (!customerIdToInvoiceCount.ContainsKey(key))
            {
                customerIdToInvoiceCount[key] = 1;
            }
            else
            {
                customerIdToInvoiceCount[key] += 1;
            }
        }

        return customerIdToInvoiceCount
        .Select(x => new CustomerInvoiceCountSummary
        {
            CustomerId = x.Key,
            InvoiceCount = x.Value
        })
        .ToList();

        throw new NotImplementedException();
    }
}