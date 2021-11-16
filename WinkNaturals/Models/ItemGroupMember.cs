using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public class ItemGroupMember : IItemGroupMember
    {
        public string ItemCode { get; set; }
        public string MasterItemCode { get; set; }

        public int MasterItemID { get; set; }
        public string MemberDescription { get; set; }
        public int SortOrder { get; set; }

        public Item Item { get; set; }
    }
}
