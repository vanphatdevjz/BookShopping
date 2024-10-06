using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    public class PdfController : Controller
    {
        public IActionResult ShowPdf(int bookId)
        {
            // Lấy đường dẫn file PDF từ bookId
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", $"{bookId}.pdf");

            if (System.IO.File.Exists(filePath))
            {
                byte[] pdfBytes = System.IO.File.ReadAllBytes(filePath);
                return File(pdfBytes, "application/pdf");
            }
            else
            {
                return NotFound(); // Nếu file không tồn tại
            }
        }
    }
}
