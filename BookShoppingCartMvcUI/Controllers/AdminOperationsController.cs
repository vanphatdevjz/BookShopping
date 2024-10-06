using BookShoppingCartMvcUI.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SelectPdf;
using System.Text;

namespace BookShoppingCartMvcUI.Controllers;

[Authorize(Roles = nameof(Roles.Admin))]
public class AdminOperationsController : Controller
{
    private readonly IUserOrderRepository _userOrderRepository;
    private readonly Dictionary<int, string> _orderStatusTranslations = new Dictionary<int, string>
        {
            { 1, "Chờ giao hàng" },
            { 2, "Đang giao hàng" },
            { 3, "Đã giao thành công" },
            { 4, "Chưa giao thành công" },
            { 5, "Đã trả lại" },
            { 6, "Shiper đưa tiền cửa hàng " }
        };
    public AdminOperationsController(IUserOrderRepository userOrderRepository)
    {
        _userOrderRepository = userOrderRepository;
    }

    public async Task<IActionResult> AllOrders()
    {
        var orders = await _userOrderRepository.UserOrders(true);
        var sortedOrders = orders.OrderByDescending(order => order.CreateDate);
        return View(sortedOrders);
    }

    public async Task<IActionResult> TogglePaymentStatus(int orderId)
    {
        try
        {
            await _userOrderRepository.TogglePaymentStatus(orderId);
        }
        catch (Exception ex)
        {
            // log exception here
        }
        return RedirectToAction(nameof(AllOrders));
    }

    public async Task<IActionResult> UpdateOrderStatus(int orderId)
    {
        var order = await _userOrderRepository.GetOrderById(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with id:{orderId} does not found.");
        }
        var orderStatusList = (await _userOrderRepository.GetOrderStatuses()).Select(orderStatus =>
        {
            return new SelectListItem { 
                Value = orderStatus.Id.ToString(),
                Text = _orderStatusTranslations.ContainsKey(orderStatus.Id) ? _orderStatusTranslations[orderStatus.Id] : orderStatus.StatusName, 
                Selected = order.OrderStatusId == orderStatus.Id };
        });
        var data = new UpdateOrderStatusModel
        {
            OrderId = orderId,
            OrderStatusId = order.OrderStatusId,
            OrderStatusList = orderStatusList
        };
        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                data.OrderStatusList = (await _userOrderRepository.GetOrderStatuses()).Select(orderStatus =>
                {
                    return new SelectListItem { Value = orderStatus.Id.ToString(), Text = _orderStatusTranslations.ContainsKey(orderStatus.Id) ? _orderStatusTranslations[orderStatus.Id] : orderStatus.StatusName, Selected = orderStatus.Id == data.OrderStatusId };
                });

                return View(data);
            }
            await _userOrderRepository.ChangeOrderStatus(data);
            TempData["msg"] = "Updated successfully";
        }
        catch (Exception ex)
        {
            // catch exception here
            TempData["msg"] = "Something went wrong";
        }
        return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });
    }
    public async Task<IActionResult> PrintInvoice(int orderId)
    {
        var order = await _userOrderRepository.GetOrderById(orderId);
        if (order == null)
        {
            return NotFound();
        }

        var orderDetails = await _userOrderRepository.GetOrderDetailsByOrderId(orderId); // Assuming this fetches the order details
        var orderDetailModal = new OrderDetailModalDTO
        {
            DivId = orderId.ToString(),
            OrderDetail = orderDetails ?? Enumerable.Empty<OrderDetail>()
        };

        var htmlContent = await GenerateInvoiceHtml(order, orderDetailModal);

        HtmlToPdf converter = new HtmlToPdf();
        PdfDocument pdfDocument = converter.ConvertHtmlString(htmlContent);
        byte[] pdf = pdfDocument.Save();
        pdfDocument.Close();

        return File(pdf, "application/pdf", $"Invoice_Order_{orderId}.pdf");
    }

    private async Task<string> GenerateInvoiceHtml(Order order, OrderDetailModalDTO orderDetailModal)
    {
        var sb = new StringBuilder();

        sb.Append("<html>");
        sb.Append("<head>");
        sb.Append("<style>");
        sb.Append("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.Append("h1 { text-align: center; }");
        sb.Append("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        sb.Append("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
        sb.Append("th { background-color: #f2f2f2; }");
        sb.Append("td { text-align: right; }");
        sb.Append(".header { margin-bottom: 20px; }");
        sb.Append(".header img { display: block; margin: 0 auto; }");
        sb.Append(".total-row { font-weight: bold; }");
        sb.Append("</style>");
        sb.Append("</head>");
        sb.Append("<body>");
        sb.Append("<div class='header'>");
        sb.Append("<img src='https://localhost:7158/images/logo-trang-an-web.png' alt='Logo' width='300' height='100' />");
        sb.Append("<h1 style='text-align:center'>Hóa đơn mua hàng</h1>");
        sb.Append("<h2>Thông tin người nhận</h2>");
        sb.Append($"<p>Ngày đặt: {order.CreateDate:dd-MM-yyyy}</p>");
        sb.Append($"<p>Tên: {order.Name}</p>");

        sb.Append($"<p>Số điện thoại: {order.MobileNumber}</p>");
        sb.Append($"<p>Địa chỉ: {order.Address}</p>");
        sb.Append($"<p>Tình trạng thanh toán: {(order.IsPaid ? "Đã trả tiền" : "Chưa trả tiền")}</p>");

        sb.Append("<h2>Chi tiết đơn hàng</h2>");
        sb.Append("<table style='width:100%'>");
        sb.Append("<tr><th>Sách</th><th>Thể loại</th><th>Giá</th><th>Số lượng</th><th>Tổng giá</th></tr>");

        decimal totalAmount = 0;
        foreach (var item in orderDetailModal.OrderDetail)
        {
            decimal itemTotal = (decimal)(item.Quantity * item.UnitPrice);
            totalAmount += itemTotal;
            sb.Append("<tr>");
            sb.Append($"<td>{item.Book.BookName}</td>");
            sb.Append($"<td>{item.Book.Genre.GenreName}</td>");
            sb.Append($"<td>{item.UnitPrice} VND</td>");
            sb.Append($"<td>{item.Quantity}</td>");
            sb.Append($"<td>{(item.Quantity * item.UnitPrice)} VND</td>");
            sb.Append("</tr>");
        }
        // Thêm hàng tổng tiền thu
        sb.Append("<tr>");
        sb.Append("<td colspan='4' style='text-align:right'><strong>Tổng tiền thu:</strong></td>");
        sb.Append($"<td><strong>{totalAmount} VND</strong></td>");
        sb.Append("</tr>");

        sb.Append("</table>");
        sb.Append("</body>");
        sb.Append("</html>");

        return sb.ToString();
    }


    public IActionResult Dashboard()
    {
        return View();
    }
    
}
