using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class ResponseJson
    {
        public bool EmailAddressFound;

        public int PersonCounter;

        public int EmailCounter;

        public List<KeyValuePair<string, int>> ActivityParticipants;

        public List<KeyValuePair<string, string>> NotificationAddresses;

    }
}