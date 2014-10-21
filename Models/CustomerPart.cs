﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;

namespace OShop.Models {
    public class CustomerPart : ContentPart<CustomerPartRecord>, ITitleAspect {
        internal LazyField<IEnumerable<CustomerAddressPartRecord>> _addresses = new LazyField<IEnumerable<CustomerAddressPartRecord>>();

        public String FirstName {
            get { return this.Retrieve(x => x.FirstName); }
            set { this.Store(x => x.FirstName, value); }
        }

        public String LastName {
            get { return this.Retrieve(x => x.LastName); }
            set { this.Store(x => x.LastName, value); }
        }

        public IEnumerable<CustomerAddressPartRecord> Addresses {
            get { return _addresses.Value; }
        }

        public string Title {
            get { return this.FirstName + " " + this.LastName; }
        }

    }
}