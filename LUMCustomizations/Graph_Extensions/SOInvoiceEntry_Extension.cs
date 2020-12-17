﻿using PX.Data;
using PX.Objects.AR;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.SO
{
    public class SOInvoiceEntry_Extension : PXGraphExtension<SOInvoiceEntry>
    {
        public override void Initialize()
        {
            base.Initialize();
            Base.report.AddMenuAction(CommercialInvoiceReport);
            Base.report.AddMenuAction(CreditNoteReport);
        }

        #region Action
        public PXAction<ARInvoice> CommercialInvoiceReport;
        [PXButton]
        [PXUIField(DisplayName = "Print Commercial Invoice", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable commercialInvoiceReport(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM602000", "Report LM602000");
            }
            return adapter.Get();
        }
        #endregion

        #region Action
        public PXAction<ARInvoice> CreditNoteReport;
        [PXButton]
        [PXUIField(DisplayName = "Print Credit Note", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable creditNoteReport(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM602005", "Report LM602005");
            }
            return adapter.Get();
        }
        #endregion
    }
}