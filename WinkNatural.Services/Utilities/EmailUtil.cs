using System.Text;
using WinkNatural.Services.DTO.Customer;

namespace WinkNatural.Services.Utilities
{
    public static class EmailUtil
    {
        //Create contact email body
        public static string ContactEmailBody(ContactRequest request)
        {
            var stringBody = new StringBuilder();
            stringBody.Append("<!DOCTYPE html>");
            stringBody.Append("<html lang='en'><head>");
            stringBody.Append("</head><body>");
            stringBody.Append("<table cellspacing='0' style ='width:680px;border:1px solid #e6e6e6;'>");
            stringBody.Append("<thead style='background-color: #f7f7f7;'>");
            stringBody.Append("<tr><td style='text-align: center;border-top: 5px solid #1aa1b3;'><img src='https://winknaturals.com/Content/images/logo.png' style=' width: 120px;'> </td>");
            stringBody.Append("</tr></thead><tbody style='font-family: arial;font-size: 14px;'>");
            stringBody.Append("<tr style='height: 310px;vertical-align: baseline;'>");
            stringBody.Append("<td style='padding: 15px;'><h3 style=''>Contact Us Email:</h3>");
            stringBody.Append($"<ul><li style='line-height:32px'><strong>Name:</strong> {request.Name}</li>");
            stringBody.Append($"<li style='line-height:32px'><strong>Phone:</strong> {request.Phone}</li>");
            stringBody.Append($"<li style='line-height:32px'><strong>Email:</strong> {request.Email}</li>");
            stringBody.Append($"<li style='line-height:32px'><strong>Notes:</strong> {request.Notes}</li></ul>");
            stringBody.Append("</td></tr><tr style='background-color: #272727;'>");
            stringBody.Append("<td style='text-align: center;'><p style='color: #fff;'>© 2021 Wink Naturals All rights reserved</p></td>");
            stringBody.Append("</tr></tbody></table></body></html>");
            return stringBody.ToString();
        }
    }
}
