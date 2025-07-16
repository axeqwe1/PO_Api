using Microsoft.AspNetCore.Mvc;
using PO_Api.Data.DTO;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly EmailService _emailService;

    public MailController()
    {
        // ตั้งค่าของ SMTP (ตัวอย่างใช้ Gmail)
        _emailService = new EmailService(
            host: "mail.yehpattana.com",
            port: 587,
            username: "jarvis-ypt@Ta-yeh.com",
            password: "J!@#1028", // ต้องเป็น App Password
            fromEmail: "jarvis-ypt@ta-yeh.com",
            displayFromEmail: "YPT PO ระบบทดสอบอีเมล"
        );
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendTestEmail([FromBody] string email)
    {

        EmailTemplateDTO template = new EmailTemplateDTO();

        template.subject = "TEST SEND EMAIL";
        template.body = "Hi";

        await _emailService.SendEmailAsync(
            to: email,
            subject: "ทดสอบส่งอีเมล",
            body: template
        );

        return Ok("ส่งแล้ว!");
    }

    [HttpPost("sendBtn")]
    public async Task<IActionResult> SendTestEmailBtn([FromBody] string email)
    {

        EmailTemplateDTO template = new EmailTemplateDTO();

        template.subject = "TEST SEND EMAIL";
        template.body = "Hi";
        template.link = "https://www.ymt-group.com/PO_Website/";
        template.btnName = "TEST";
        await _emailService.SendEmailAsync(
            to: email,
            subject: "ทดสอบส่งอีเมล",
            body: template
        );

        return Ok("ส่งแล้ว!");
    }
}
