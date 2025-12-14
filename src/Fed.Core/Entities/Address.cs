namespace Fed.Core.Entities
{
    public class Address
    {
        public Address()
        { }

        public Address(
            string companyName,
            string addressLine1,
            string addressLine2,
            string town,
            string postcode)
        {
            CompanyName = companyName;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            Town = town;
            _postcode = NormalisePostcode(postcode);
        }

        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }

        private string _postcode;
        public string Postcode
        {
            get { return _postcode; }
            set { _postcode = NormalisePostcode(value); }
        }


        public static string NormalisePostcode(string postcode)
        {
            if (string.IsNullOrEmpty(postcode))
                return postcode;

            //removes end and start spaces 
            postcode = postcode.Trim();
            //removes in middle spaces 
            postcode = postcode.Replace(" ", "");

            switch (postcode.Length)
            {
                //add space after 2 characters if length is 5 
                case 5: postcode = postcode.Insert(2, " "); break;
                //add space after 3 characters if length is 6 
                case 6: postcode = postcode.Insert(3, " "); break;
                //add space after 4 characters if length is 7 
                case 7: postcode = postcode.Insert(4, " "); break;

                default: break;
            }
            return postcode.ToUpper();
        }
    }
}