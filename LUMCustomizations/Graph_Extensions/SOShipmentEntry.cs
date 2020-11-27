﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LUMCustomizations.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.SO
{
    public class SOShipmentEntry_Extension : PXGraphExtension<SOShipmentEntry>
    {
        #region String & Decimal Contants
        public const string CartonSize = "CARTONSIZE";
        public const string QtyCarton  = "QTYCARTON";
        public const string GrsWeight  = "GRSWEIGHT";
        public const string CartonPalt = "CARTONPALT";
        public const string PalletWgt  = "PALLETWGT";

        public class CartonSizeAttr : PX.Data.BQL.BqlString.Constant<CartonSizeAttr>
        {
            public CartonSizeAttr() : base(CartonSize) { }
        }

        public class QtyCartonAttr : PX.Data.BQL.BqlString.Constant<QtyCartonAttr>
        {
            public QtyCartonAttr() : base(QtyCarton) { }
        }

        public class GrsWeightAttr : PX.Data.BQL.BqlString.Constant<GrsWeightAttr>
        {
            public GrsWeightAttr() : base(GrsWeight) { }
        }

        public class decimal5000 : PX.Data.BQL.BqlDecimal.Constant<decimal5000>
        {
            public decimal5000() : base(5000M) { }
        }
        #endregion

        #region Delegate Function        
        public delegate void DelegatePersist();
        [PXOverride]
        public void Persist(DelegatePersist baseMethod)
        {
            baseMethod();

            this.SetShipContactAndAddress();

            baseMethod();
        }

        public override void Initialize()
        {
            base.Initialize();

            // Get Visible
            var _graph = PXGraph.CreateInstance<SOOrderEntry>();
            var _PIPreference = from t in _graph.Select<LifeSyncPreference>()
                                select t;
            var _visible = _PIPreference.FirstOrDefault() == null ? false : _PIPreference.FirstOrDefault().ProformaInvoicePrinting.Value
                                                                  ? true : false;
            // Set Button Visible
            ProformaInvoice.SetVisible(_visible);
            // Add Button
            if (_visible)
                Base.report.AddMenuAction(ProformaInvoice);
        }
        #endregion

        #region Event Handler
        protected void _(Events.FieldUpdated<SOShipLineExt.usrCartonQty> e)
        {
            SOShipLine row = e.Row as SOShipLine;
            SOShipLineExt rowExt = row.GetExtension<SOShipLineExt>();

            rowExt.UsrDimWeight = rowExt.UsrCartonQty * rowExt.UsrBaseItemVolume * 1000000M / row.ShippedQty / 5000M;
        }
        #endregion

        #region Static Method
        public static string GetMultLotSerNbr(string shipmentNbr)
        {
            string str = null;

            foreach (PXResult<SOShipLineSplit> result in SelectFrom<SOShipLineSplit>.Where<SOShipLineSplit.shipmentNbr.IsEqual<@P.AsString>>.View.ReadOnly
                                                         .Select(PXGraph.CreateInstance<SOShipmentEntry>(), shipmentNbr))
            {
                SOShipLineSplit shipLineSplit = result;

                str += string.Format("{0}/", shipLineSplit.LotSerialNbr);
            }

            return str == null ? string.Empty : str.Substring(0, str.Length - 1);
        }
        #endregion

        #region Method
        public virtual void SetShipContactAndAddress()
        {
            SOShipment current = Base.Document.Current;

            if (current == null || !current.GetExtension<SOShipmentExt>().UsrShipToID.HasValue) { return; }

            AddressAttribute addressAttribute = new SOShipmentAddressAttribute(typeof(Select2<Address, InnerJoin<Location, On<Location.bAccountID, Equal<Address.bAccountID>,
                                                                                                                              And<Address.addressID, Equal<Location.defAddressID>>>,
                                                                                                       LeftJoin<SOShipmentAddress, On<SOShipmentAddress.customerID, Equal<Address.bAccountID>,
                                                                                                                                      And<SOShipmentAddress.customerAddressID, Equal<Address.addressID>,
                                                                                                                                          And<SOShipmentAddress.revisionID, Equal<Address.revisionID>,
                                                                                                                                              And<SOShipmentAddress.isDefaultAddress, Equal<True>>>>>>>,
                                                                                                       Where<Location.bAccountID, Equal<Current<SOShipment.customerID>>,
                                                                                                             And<Location.locationID, Equal<Current<SOShipmentExt.usrShipToID>>>>>))
            {
                FieldName = "shipAddressID"
            };
            addressAttribute.DefaultAddress<SOShipmentAddress, SOShipmentAddress.addressID>(Base.Document.Cache, current, null);

            ContactAttribute contactAttribute = new SOShipmentContactAttribute(typeof(Select2<Contact, InnerJoin<Location, On<Location.bAccountID, Equal<Contact.bAccountID>,
                                                                                                                              And<Contact.contactID, Equal<Location.defContactID>>>,
                                                                                                       LeftJoin<SOShipmentContact, On<SOShipmentContact.customerID, Equal<Contact.bAccountID>,
                                                                                                                                      And<SOShipmentContact.customerContactID, Equal<Contact.contactID>,
                                                                                                                                          And<SOShipmentContact.revisionID, Equal<Contact.revisionID>,
                                                                                                                                              And<SOShipmentContact.isDefaultContact, Equal<True>>>>>>>,
                                                                                                       Where<Location.bAccountID, Equal<Current<SOShipment.customerID>>,
                                                                                                             And<Location.locationID, Equal<Current<SOShipmentExt.usrShipToID>>>>>))
            {
                FieldName = "shipContactID"
            };

            contactAttribute.DefaultContact<SOShipmentContact, SOShipmentContact.contactID>(Base.Document.Cache, (object)current, (object)null);

            Base.Document.Cache.Update((object)current);
        }
        #endregion

        #region Action
        public PXAction<SOShipment> ProformaInvoice;
        [PXButton]
        [PXUIField(DisplayName = "Print Proforma Invoice Report", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable proformaInvoice(PXAdapter adapter)
        {
            var _reportID = "lm611000";
            var parameters = new Dictionary<string, string>()
            {
                ["ShipmentNbr"] = (Base.Caches<SOShipment>().Current as SOShipment)?.ShipmentNbr
            };
            if (parameters["ShipmentNbr"] != null)
                throw new PXReportRequiredException(parameters, _reportID, string.Format("Report {0}", _reportID));
            return adapter.Get<SOShipment>().ToList();
        }
        #endregion
    }
}