namespace CSMS.Models.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }

        // Roles
        public List<string> AllRoles { get; set; } = new();
        public List<string> SelectedRoles { get; set; } = new();
    }

}
