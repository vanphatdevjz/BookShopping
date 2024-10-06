using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;

namespace BookShoppingCartMvcUI.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly PayOS _payOS;


        public CheckoutController(PayOS payOS)
        {
            _payOS = payOS;

        }

        [HttpGet("/main")]
        public IActionResult Index()
        {
            // Trả về trang HTML có tên "MyView.cshtml"
            return View("Index");
        }
        [HttpGet("/cancel")]
        public IActionResult Cancel()
        {
            // Trả về trang HTML có tên "MyView.cshtml"
            return View("cancel");
        }
        [HttpGet("/success")]
        public IActionResult Success()
        {
            // Trả về trang HTML có tên "MyView.cshtml"
            return View("success");
        }
        [HttpPost("/create-payment-link")]
        public async Task<IActionResult> Checkout()
        {
            try
            {

                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData("One Piece", 1, 2999);
                List<ItemData> items = new List<ItemData>();
                items.Add(item);
                PaymentData paymentData = new PaymentData(orderCode, 2999, "Thanh toan don hang", items, "http://localhost:5232/cancel", "http://localhost:5232/success");

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                return Redirect(createPayment.checkoutUrl);
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                return Redirect("http://localhost:5232/");
            }
        }
    }
}
