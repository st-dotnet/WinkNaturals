namespace WinkNaturals.Models
{
    public class CustomerUpdateModel
    {
        public int CustomerId { get; set; }
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string NewPassword { get; set; }
        public string Url { get; set; }
    }
}
