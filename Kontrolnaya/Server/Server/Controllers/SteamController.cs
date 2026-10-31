using System.Net;
using System.Net.Mail;
using Server.Framework;
using Server.Framework.attribute;
using Server.Framework.Handlers;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Server.Controllers;
//http://127.0.0.1:8888/steam/send?=to=&hero=
[HttpController("steam")]
internal class SteamController
{
    [Get("send")]
    public Task Send(HandlerContext context, string to, string hero)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument document = web.Load($"https://steamcommunity.com/market/search?category_Hero=npc_dota_hero_{hero}&appid=570");
        HtmlNodeCollection typeallNodes = document.DocumentNode.SelectNodes("//span[@class='NWgTAn1-iiQ- UEx0MpggIp8- IokSIloSPlA-']");
        HtmlNodeCollection nameallNodes = document.DocumentNode.SelectNodes("//span[@class='h9k0m4mdzeY- -jRzZNDvN4c- E-F5JVsCXEo- IokSIloSPlA-']");
        string result = "";
        for (int i = 1; i < 15; i++)
        {
            result += $"{i}.";
            result += typeallNodes[i].InnerText;
            result += nameallNodes[i].InnerText;
            result += "\n";
        }
        return SendEmailAsync(to, "Dota2",result);
    }


    public async Task SendEmailAsync(string email, string subject, string message)
    {
        using var emailMessage = new MimeMessage();
 
        emailMessage.From.Add(new MailboxAddress("Любитель Доты", "marself4izov@yandex.ru"));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };
        
        Console.WriteLine("123");
        
        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            await client.ConnectAsync("smtp.yandex.ru", 465, true);
            await client.AuthenticateAsync("marself4izov@yandex.ru", "fbggmvwdmetafcmc");
            await client.SendAsync(emailMessage);
  
            await client.DisconnectAsync(true);
            Console.WriteLine("123");
        }
    }
}