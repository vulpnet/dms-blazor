using DmsBlazor.Shared.Models;

namespace DmsBlazor.Api.Data;

/// <summary>
/// So sánh danh sách dòng cũ/mới của 1 đơn hàng, sinh ra mô tả ngắn gọn cho
/// OrderEditLog — vd "Sửa SL Cola 330ml: 60 → 80 thùng", "Thêm SP: Snack x20",
/// "Xoá SP: Bánh quy 200g". Tách riêng để dễ test độc lập với controller.
/// </summary>
public static class OrderEditDiffBuilder
{
    public static List<string> BuildChanges(List<OrderLine> oldLines, List<PricedOrderLine> newLines)
    {
        var changes = new List<string>();
        var oldByCode = oldLines.ToDictionary(l => l.ProductCode);
        var newByCode = newLines.ToDictionary(l => l.Product.Code);

        foreach (var (code, oldLine) in oldByCode)
        {
            if (!newByCode.TryGetValue(code, out var newLine))
            {
                changes.Add($"Xoá SP: {oldLine.ProductName}");
            }
            else if (oldLine.Qty != newLine.Qty)
            {
                changes.Add($"Sửa SL {oldLine.ProductName}: {oldLine.Qty} → {newLine.Qty}");
            }
        }

        foreach (var (code, newLine) in newByCode)
        {
            if (!oldByCode.ContainsKey(code))
                changes.Add($"Thêm SP: {newLine.Product.Name} x{newLine.Qty}");
        }

        return changes;
    }
}
