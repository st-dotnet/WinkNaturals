using System;

namespace WinkNaturals.Models
{
    public class HomePageReviewsModel
    {
        public int id { get; set; }
        public int score { get; set; }
        public int votes_up { get; set; }
        public int votes_down { get; set; }
        public string content { get; set; }
        public string title { get; set; }
        public DateTime created_at { get; set; }
        public bool deleted { get; set; }
        public bool verified_buyer { get; set; }
        public object source_review_id { get; set; }
        public double sentiment { get; set; }
        public object custom_fields { get; set; }
        public int product_id { get; set; }
        public User user { get; set; }
    }
    public class User
    {
        public int user_id { get; set; }
        public object social_image { get; set; }
        public string user_type { get; set; }
        public int is_social_connected { get; set; }
        public string display_name { get; set; }
    }
}
