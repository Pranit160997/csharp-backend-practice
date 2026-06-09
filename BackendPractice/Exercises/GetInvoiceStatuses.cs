using System;
using System.Collections.Generic;
using System.Linq;

public static class Exercise06_InvoicePaymentStatus
{
    public static void Run()
    {
        var invoices = new List<StatusInvoice>
        {
            new StatusInvoice { InvoiceId = "INV-1", CustomerId = "C1", Amount = 100 },
            new StatusInvoice { InvoiceId = "INV-2", CustomerId = "C2", Amount = 200 },
            new StatusInvoice { InvoiceId = "INV-3", CustomerId = "C3", Amount = 300 },
            new StatusInvoice { InvoiceId = "INV-4", CustomerId = "C4", Amount = 400 }
        };

        var payments = new List<StatusPayment>
        {
            new StatusPayment { PaymentId = "P1", InvoiceId = "INV-1", Amount = 100, Status = StatusPaymentStatus.Success },
            new StatusPayment { PaymentId = "P2", InvoiceId = "INV-2", Amount = 50, Status = StatusPaymentStatus.Success },
            new StatusPayment { PaymentId = "P3", InvoiceId = "INV-2", Amount = 25, Status = StatusPaymentStatus.Failed },
            new StatusPayment { PaymentId = "P4", InvoiceId = "INV-3", Amount = 350, Status = StatusPaymentStatus.Success },
            new StatusPayment { PaymentId = "P5", InvoiceId = "INV-4", Amount = 100, Status = StatusPaymentStatus.Reversed }
        };

        var service = new InvoiceStatusService();

        var result = service.GetInvoiceStatuses(invoices, payments);

        foreach (var item in result)
        {
            Console.WriteLine(
                $"InvoiceId: {item.InvoiceId}, " +
                $"InvoiceAmount: {item.InvoiceAmount}, " +
                $"TotalPaid: {item.TotalPaid}, " +
                $"Status: {item.Status}");
        }
    }
}

public enum StatusPaymentStatus
{
    Success,
    Failed,
    Reversed
}

public enum InvoicePaymentStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overpaid
}

public class StatusInvoice
{
    public required string InvoiceId { get; set; }
    public required string CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class StatusPayment
{
    public required string PaymentId { get; set; }
    public required string InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public StatusPaymentStatus Status { get; set; }
}

public class InvoiceStatusSummary
{
    public required string InvoiceId { get; set; }
    public decimal InvoiceAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public InvoicePaymentStatus Status { get; set; }
}

public class InvoiceStatusService
{
    public List<InvoiceStatusSummary> GetInvoiceStatuses(
        List<StatusInvoice> invoices,
        List<StatusPayment> payments)
    {
        // Your code here
        var resultList = new List<InvoiceStatusSummary>();
        var totalInvoiceAmountDict = new Dictionary<string, decimal>();

        foreach (var invoice in invoices)
        {
            var currKey = invoice.InvoiceId;
            if (!totalInvoiceAmountDict.ContainsKey(currKey))
            {
                totalInvoiceAmountDict[currKey] = invoice.Amount;
            }
            else
            {
                totalInvoiceAmountDict[currKey] += invoice.Amount;
            }
        }

        var totalPaidAmountDict = payments
        .Where(p => p.Status == StatusPaymentStatus.Success)
        .GroupBy(g => g.InvoiceId)
        .ToDictionary(
            i => i.Key,
            i => i.Sum(x => x.Amount)
        );

        foreach (var dictPair in totalInvoiceAmountDict)
        {
            var invoiceId = dictPair.Key;
            var invoiceAmount = dictPair.Value;
            var didPay = totalPaidAmountDict.TryGetValue(invoiceId, out var totalPaid);

            var status = InvoicePaymentStatus.Unpaid;

            if (totalPaid == invoiceAmount)
            {
                status = InvoicePaymentStatus.Paid;
            }

            else if (totalPaid != 0 && totalPaid < invoiceAmount)
            {
                status = InvoicePaymentStatus.PartiallyPaid;
            }

            else if (totalPaid > invoiceAmount)
            {
                status = InvoicePaymentStatus.Overpaid;
            }

            else
            {
                status = InvoicePaymentStatus.Unpaid;
            }
            

            var result = new InvoiceStatusSummary
            {
              InvoiceId = invoiceId,
              InvoiceAmount = invoiceAmount,
              TotalPaid = totalPaid,
              Status = status
            };

            resultList.Add(result);
        }

        return resultList;
    }
}