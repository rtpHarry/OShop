﻿using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;

namespace OShop.Migrations {
    [OrchardFeature("OShop.Customers")]
    public class CustomerMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("CustomerPartRecord", table => table
                 .ContentPartRecord()
                 .Column<string>("FirstName")
                 .Column<string>("LastName"));

            SchemaBuilder.CreateTable("CustomerAddressPartRecord", table => table
                 .ContentPartRecord()
                 .Column<int>("CustomerPartRecord_Id")
                 .Column<string>("Label")
                 .Column<int>("LocationsCountryRecord_Id")
                 .Column<int>("LocationsStateRecord_Id"));

            ContentDefinitionManager.AlterPartDefinition("CustomerPart", part => part
                .Attachable(false)
                );

            ContentDefinitionManager.AlterTypeDefinition("Customer", type => type
                .WithPart("CommonPart", builder => builder
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("CustomerPart")
                .Creatable(false)
                );

            ContentDefinitionManager.AlterPartDefinition("CustomerAddressPart", part => part
                .Attachable(false)
                .WithField("Name", f => f.OfType("TextField"))
                .WithField("AddressLine1", f => f.OfType("TextField"))
                .WithField("AddressLine2", f => f.OfType("TextField"))
                .WithField("Zipcode", f => f.OfType("TextField"))
                .WithField("City", f => f.OfType("TextField"))
                );

            ContentDefinitionManager.AlterTypeDefinition("CustomerAddress", type => type
                .WithPart("CommonPart")
                .WithPart("AddressPart")
                .Creatable(false)
                );


            return 1;
        }
    }
}