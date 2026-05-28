using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    public static async Task Main()
    {
        var ct = CancellationToken.None;

        IOrderRepository orders = new InMemoryOrderRepository();
        ICouponRepository coupons = new InMemoryCouponRepository();
        ICouponRedemptionRepository redemptions = new InMemoryCouponRedemptionRepository();
        IClock clock = new SystemClock();
        ILogger logger = new ConsoleLogger();

        var service = new CouponRedemptionService(
            orders,
            coupons,
            redemptions,
            clock,
            logger);

        await Run(service, "ORD-1", "SAVE10", "RED-1", ct);
        await Run(service, "ORD-1", "SAVE10", "RED-1", ct);
        await Run(service, "ORD-2", "SAVE10", "RED-2", ct);
        await Run(service, "ORD-1", "EXPIRED20", "RED-3", ct);
        await Run(service, "ORD-1", "USEDUP15", "RED-4", ct);
        await Run(service, "ORD-3", "SAVE10", "RED-5", ct);
        await Run(service, "ORD-1", "BIG500", "RED-6", ct);
        await Run(service, "ORD-404", "SAVE10", "RED-7", ct);
        await Run(service, "ORD-1", "NOPE", "RED-8", ct);
        await Run(service, "ORD-1", "SAVE10", "", ct);
    }

    private static async Task Run(
        CouponRedemptionService service,
        string orderId,
        string couponCode,
        string redemptionReference,
        CancellationToken ct)
    {
        var result = await service.RedeemCouponAsync(
            new RedeemCouponRequest(orderId, couponCode, redemptionReference),
            ct);

        Console.WriteLine(
            $"RESULT: Status={result.Status}, RedemptionId={result.RedemptionId}, Discount={result.DiscountApplied}, NewOrderTotal={result.NewOrderTotal}, Reason={result.Reason}");

        Console.WriteLine(new string('-', 100));
    }
}

// =======================
// Domain
// =======================

public readonly record struct RedeemCouponRequest(
    string OrderId,
    string CouponCode,
    string RedemptionReference
);

public enum OrderStatus
{
    Unknown = 0,
    Pending = 1,
    DiscountApplied = 2,
    Paid = 3,
    Cancelled = 4
}

public enum CouponStatus
{
    Unknown = 0,
    Active = 1,
    Expired = 2,
    Disabled = 3
}

public enum CouponRedemptionStatus
{
    Unknown = 0,
    DiscountApplied = 1,
    Declined = 2,
    DuplicateRedemption = 3
}

public sealed record CouponRedemptionResult(
    CouponRedemptionStatus Status,
    string RedemptionId,
    decimal DiscountApplied,
    decimal NewOrderTotal,
    string Reason,
    DateTimeOffset CreatedAtUtc
);

public sealed record Order(
    string OrderId,
    decimal OrderTotal,
    OrderStatus Status
);

public sealed record Coupon(
    string CouponCode,
    decimal DiscountAmount,
    CouponStatus Status,
    DateTimeOffset ExpiresAtUtc,
    int UsageLimit,
    int UsageCount
);

public sealed record CouponRedemption(
    string RedemptionId,
    string OrderId,
    string CouponCode,
    string RedemptionReference,
    decimal DiscountApplied,
    DateTimeOffset CreatedAtUtc
);

// =======================
// Interfaces
// =======================

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string orderId, CancellationToken ct);
    Task UpdateAsync(Order order, CancellationToken ct);
}

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string couponCode, CancellationToken ct);
    Task UpdateAsync(Coupon coupon, CancellationToken ct);
}

public interface ICouponRedemptionRepository
{
    Task<CouponRedemption?> GetByReferenceAsync(
        string redemptionReference,
        CancellationToken ct);

    Task SaveAsync(CouponRedemption redemption, CancellationToken ct);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface ILogger
{
    void Info(string message);
}

// =======================
// Fake Implementations
// =======================

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private static Order[] _orders =
    {
        new Order("ORD-1", 100m, OrderStatus.Pending),
        new Order("ORD-2", 150m, OrderStatus.Paid),
        new Order("ORD-3", 5m, OrderStatus.Pending)
    };

    public Task<Order?> GetByIdAsync(string orderId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var order = _orders.FirstOrDefault(o =>
            o.OrderId.Equals(orderId, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(order);
    }

    public Task UpdateAsync(Order order, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int index = Array.FindIndex(_orders, o =>
            o.OrderId.Equals(order.OrderId, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            _orders[index] = order;

            Console.WriteLine(
                $"Order updated: OrderId={order.OrderId}, Total={order.OrderTotal}, Status={order.Status}");
        }

        return Task.CompletedTask;
    }
}

public sealed class InMemoryCouponRepository : ICouponRepository
{
    private static Coupon[] _coupons =
    {
        new Coupon("SAVE10", 10m, CouponStatus.Active, DateTimeOffset.UtcNow.AddDays(10), 5, 0),
        new Coupon("EXPIRED20", 20m, CouponStatus.Active, DateTimeOffset.UtcNow.AddDays(-1), 5, 0),
        new Coupon("USEDUP15", 15m, CouponStatus.Active, DateTimeOffset.UtcNow.AddDays(10), 1, 1),
        new Coupon("BIG500", 500m, CouponStatus.Active, DateTimeOffset.UtcNow.AddDays(10), 5, 0),
        new Coupon("DISABLED5", 5m, CouponStatus.Disabled, DateTimeOffset.UtcNow.AddDays(10), 5, 0)
    };

    public Task<Coupon?> GetByCodeAsync(string couponCode, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var coupon = _coupons.FirstOrDefault(c =>
            c.CouponCode.Equals(couponCode, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(coupon);
    }

    public Task UpdateAsync(Coupon coupon, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int index = Array.FindIndex(_coupons, c =>
            c.CouponCode.Equals(coupon.CouponCode, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            _coupons[index] = coupon;

            Console.WriteLine(
                $"Coupon updated: CouponCode={coupon.CouponCode}, UsageCount={coupon.UsageCount}/{coupon.UsageLimit}");
        }

        return Task.CompletedTask;
    }
}

public sealed class InMemoryCouponRedemptionRepository : ICouponRedemptionRepository
{
    private static CouponRedemption[] _redemptions =
        Array.Empty<CouponRedemption>();

    public Task<CouponRedemption?> GetByReferenceAsync(
        string redemptionReference,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var redemption = _redemptions.FirstOrDefault(r =>
            r.RedemptionReference.Equals(redemptionReference, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(redemption);
    }

    public Task SaveAsync(CouponRedemption redemption, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _redemptions = _redemptions.Append(redemption).ToArray();

        Console.WriteLine(
            $"Redemption saved: RedemptionId={redemption.RedemptionId}, Ref={redemption.RedemptionReference}");

        return Task.CompletedTask;
    }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class ConsoleLogger : ILogger
{
    public void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
}

// =======================
// Main Exercise
// =======================

public sealed class CouponRedemptionService
{
    private readonly IOrderRepository _orders;
    private readonly ICouponRepository _coupons;
    private readonly ICouponRedemptionRepository _redemptions;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public CouponRedemptionService(
        IOrderRepository orders,
        ICouponRepository coupons,
        ICouponRedemptionRepository redemptions,
        IClock clock,
        ILogger logger)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _coupons = coupons ?? throw new ArgumentNullException(nameof(coupons));
        _redemptions = redemptions ?? throw new ArgumentNullException(nameof(redemptions));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CouponRedemptionResult> RedeemCouponAsync(
        RedeemCouponRequest request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _logger.Info("Coupon redemption started");

        try
        {
            var (requestOrderId,
            requestCouponCode,
            requestRedemptionReference) = request;

            //validation

            if (string.IsNullOrWhiteSpace(requestOrderId))
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Unknown,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The order ID cannot be empty",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (string.IsNullOrWhiteSpace(requestCouponCode))
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Unknown,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The coupon code cannot be empty",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (string.IsNullOrWhiteSpace(requestRedemptionReference))
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Unknown,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Redemption reference cannot be empty",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            //existence

            var currentOrder = await _orders.GetByIdAsync(requestOrderId, ct);
            if (currentOrder is null)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "No Order exists with the provided Order ID",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            var (_,
            currentOrderTotal,
            currentOrderStatus) = currentOrder;

            var currentCoupon = await _coupons.GetByCodeAsync(requestCouponCode, ct);
            if (currentCoupon is null)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "No Coupon exists with the provided Coupon code",
                    CreatedAtUtc: _clock.UtcNow
                );
            }
            
            var (_,
            currentCouponDiscountAmount,
            currentCouponStatus,
            currentCouponExpiresAtUtc,
            currentCouponUsageLimit,
            currentCouponUsageCount) = currentCoupon;

            var currentRedemption = await _redemptions.GetByReferenceAsync(requestRedemptionReference, ct);
            if (currentRedemption is not null)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.DuplicateRedemption,
                    RedemptionId: currentRedemption.RedemptionId,
                    DiscountApplied: currentRedemption.DiscountApplied,
                    NewOrderTotal: currentOrderTotal,
                    Reason: "This redemption reference has already been processed",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            //Allow only Order status pending
            if (currentOrderStatus == OrderStatus.Unknown)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Order status is currently unknown",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (currentOrderStatus == OrderStatus.Cancelled)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Order has been Cancelled",
                    CreatedAtUtc: _clock.UtcNow
                );
            } 

            if (currentOrderStatus == OrderStatus.DiscountApplied)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: currentCouponDiscountAmount,
                    NewOrderTotal: currentOrderTotal,
                    Reason: "Discount has already been applied to the Order",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (currentOrderStatus == OrderStatus.Paid)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: currentOrderTotal,
                    Reason: "Order has been paid, cannot apply coupon code now",
                    CreatedAtUtc: _clock.UtcNow
                );
            }
            
            //Order is now Pending
            //Allow only Active coupon
            if (currentCouponStatus == CouponStatus.Unknown)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Coupon cannot be applied as its status is Unknown",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (currentCouponStatus == CouponStatus.Disabled)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Coupon cannot be applied as it has been Disabled",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (currentCouponStatus == CouponStatus.Expired)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Coupon cannot be applied as it has been Expired",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            //Coupon is now Active and Order is Pending
            //check for Coupon usage count and usage limit
            if (currentCouponUsageCount >= currentCouponUsageLimit)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Coupon cannot be used beyond it's usage limit",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            if (currentCouponDiscountAmount > currentOrderTotal)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Coupon discount should be less than Order total amount",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            //check for Coupon expires at
            if (_clock.UtcNow > currentCouponExpiresAtUtc)
            {
                return new CouponRedemptionResult(
                    Status: CouponRedemptionStatus.Declined,
                    RedemptionId: "",
                    DiscountApplied: 0,
                    NewOrderTotal: 0,
                    Reason: "The Coupon cannot be applied as it has been Expired",
                    CreatedAtUtc: _clock.UtcNow
                );
            }

            //Coupon is now Active and Order is Pending
            //Coupon usage count is valid and discount amt is less than order amt
            var newRedemption = new CouponRedemption(
                RedemptionId: Guid.NewGuid().ToString(),
                OrderId: requestOrderId,
                CouponCode: requestCouponCode,
                RedemptionReference: requestRedemptionReference,
                DiscountApplied: currentCouponDiscountAmount,
                CreatedAtUtc: _clock.UtcNow
            );
            await _redemptions.SaveAsync(newRedemption, ct);

            var CouponWithNewCount = currentCoupon with
            {
                UsageCount = currentCouponUsageCount + 1
            }; 
            await _coupons.UpdateAsync(CouponWithNewCount, ct);

            var OrderWithNewTotal = currentOrder with
            {
                OrderTotal = currentOrderTotal - currentCouponDiscountAmount,
                Status = OrderStatus.DiscountApplied
            };
            await _orders.UpdateAsync(OrderWithNewTotal, ct);

            return new CouponRedemptionResult(
                Status: CouponRedemptionStatus.DiscountApplied,
                RedemptionId: newRedemption.RedemptionId,
                DiscountApplied: currentCouponDiscountAmount,
                NewOrderTotal: currentOrderTotal - currentCouponDiscountAmount,
                Reason: "The Coupon has been successfully applied",
                CreatedAtUtc: _clock.UtcNow
            );

        }
        finally
        {
            _logger.Info("Coupon redemption ended");
        }
    }
}