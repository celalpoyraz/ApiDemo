using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDemo.Context.Entity
{
    public class AddXmlToDb
    {
        public int ID { get; set; }

        public string WebSiteUrl { get; set; }
        public string PartyIdentifacition { get; set; }
        public string PartyName { get; set; }
        public string PartyTaxSchema { get; set; }

        public string PostalAddress { get; set; }

        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Mail { get; set; }
    }
}
