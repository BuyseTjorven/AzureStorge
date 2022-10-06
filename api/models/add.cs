namespace MCT.Functions
{
    public class Persoon
    {
        public System.Guid RegistrationId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string EMail { get; set; }
        public string Zipcode { get; set; }
        public int Age { get; set; }

        public bool IsFirstTimer {get; set;}
    }
}