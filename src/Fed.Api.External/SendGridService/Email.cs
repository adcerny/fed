using System.Collections.Generic;

namespace Fed.Api.External.SendGridService
{
    public class Email
    {
        private string _fromAddress;
        private IList<string> _toAddresses;
        private IList<string> _ccs;
        private IList<string> _bccs;

        public string FromAddress
        {
            get { return _fromAddress; }

            set
            {
                _fromAddress = value?.ToLowerInvariant();
            }
        }

        public IList<string> ToAddresses
        {
            get { return _toAddresses; }
            set
            {
                if (value is null)
                {
                    _toAddresses = null;
                }
                else
                {
                    _toAddresses = new List<string>();

                    foreach (var emailAddress in value)
                        _toAddresses.Add(emailAddress.ToLowerInvariant());
                }
            }
        }

        public IList<string> CCs
        {
            get { return _ccs; }
            set
            {
                if (value is null)
                {
                    _ccs = null;
                }
                else
                {
                    _ccs = new List<string>();

                    foreach (var emailAddress in value)
                        _ccs.Add(emailAddress.ToLowerInvariant());
                }
            }
        }

        public IList<string> BCCs
        {
            get { return _bccs; }
            set
            {
                if (value is null)
                {
                    _bccs = null;
                }
                else
                {
                    _bccs = new List<string>();

                    foreach (var emailAddress in value)
                        _bccs.Add(emailAddress.ToLowerInvariant());
                }
            }
        }

        public string Subject { get; set; }
        public string PlainText { get; set; }
        public string HtmlText { get; set; }
    }
}
