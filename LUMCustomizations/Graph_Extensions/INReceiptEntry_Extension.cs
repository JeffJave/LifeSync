﻿using System.Collections;
using System.Collections.Generic;
using LUMCustomizations.Library;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.PO;

namespace PX.Objects.IN
{
    public class INReceiptEntry_Extension : PXGraphExtension<INReceiptEntry>
    {
        public bool IsActive()
        {
            //active customize button if current country ID is CN or HK
            return new LumLibrary().isCNorHK();
        }

        public override void Initialize()
        {
            base.Initialize();
            Base.report.AddMenuAction(InventoryReceiptReport);
            Base.report.AddMenuAction(InventoryReceiptReportruku);
        }

        /// <summary>
        /// 當Purchase Receipts Release的時候，自動產生Recepits單時，自動帶上Descr
        /// </summary>
        public virtual void _(Events.RowPersisting<INRegister> e)
        {
            INRegister row = e.Row;
            if (new LumLibrary().GetJournalEnhance && string.IsNullOrEmpty(row.TranDesc) && row.POReceiptType == "RT" && !string.IsNullOrEmpty(row.POReceiptNbr))
            {
                var venderID = SelectFrom<POReceipt>
                               .Where<POReceipt.receiptNbr.IsEqual<P.AsString>
                                .And<POReceipt.receiptType.IsEqual<P.AsString>>>
                               .View.Select(Base, row.POReceiptNbr, "RT")?.TopFirst?.VendorID;
                var vendorName = SelectFrom<Vendor>
                                 .Where<Vendor.bAccountID.IsEqual<@P.AsInt>>
                                 .View.Select(Base, venderID)?.TopFirst.AcctName;
                row.TranDesc = $"{row.POReceiptNbr} {vendorName}";
            }
        }

        #region Action
        public PXAction<INRegister> InventoryReceiptReport;
        [PXButton]
        [PXUIField(DisplayName = "Inventory Receipt Report", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable inventoryReceiptReport(PXAdapter adapter)
        {
            if (Base.receipt.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.receipt.Current.DocType;
                parameters["RefNbr"] = Base.receipt.Current.RefNbr;
                parameters["PeriodTo"] = null;
                parameters["PeriodFrom"] = null;
                throw new PXReportRequiredException(parameters, "LM612000", "Report LM612000");
            }
            return adapter.Get();
        }

        public PXAction<INRegister> InventoryReceiptReportruku;
        [PXButton]
        [PXUIField(DisplayName = "Inventory Receipt Report - rukudan", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable inventoryReceiptReportruku(PXAdapter adapter)
        {
            if (Base.receipt.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.receipt.Current.DocType;
                parameters["RefNbr"] = Base.receipt.Current.RefNbr;
                parameters["PeriodTo"] = null;
                parameters["PeriodFrom"] = null;
                throw new PXReportRequiredException(parameters, "LM612010", "Report LM612010");
            }
            return adapter.Get();
        }
        #endregion

    }
}