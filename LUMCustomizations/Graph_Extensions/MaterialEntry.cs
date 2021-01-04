using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;

namespace JAMS.AM
{
    public class MaterialEntry_Extension : PXGraphExtension<MaterialEntry>
    {
        public override void Initialize()
        {
            ReportAction.AddMenuAction(MaterialIssuesAction);
            ReportAction.MenuAutoOpen = true;
        }

        #region Override Attribute
        [PXDBQuantity(0,typeof(AMMTran.uOM), typeof(AMMTran.baseQty), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0")]
        [PXUIField(DisplayName = "Quantity")]
        [PXFormula(null, typeof(SumCalc<AMBatch.totalQty>))]
        public virtual void _(Events.CacheAttached<AMMTran.qty> e) { }

        #endregion

        #region  Actions

        #region Report Action
        public PXAction<AMBatch> ReportAction;
        [PXButton]
        [PXUIField(DisplayName = "Report")]
        protected void reportAction() { }
        #endregion

        #region Material Issues Action
        public PXAction<AMBatch> MaterialIssuesAction;
        [PXButton]
        [PXUIField(DisplayName = "Material Issues", MapEnableRights = PXCacheRights.Select)]
        protected void materialIssuesAction()
        {
            var curAMBatchCache = (AMBatch)Base.batch.Cache.Current;
            // create the parameter for report
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["BatNbr"] = curAMBatchCache.BatNbr;
            parameters["AttributeID"] = "PRODLINE";

            // using Report Required Exception to call the report
            throw new PXReportRequiredException(parameters, "LM206100", "LM206100");
        }
        #endregion

        #endregion

        #region Event Handlers

        #endregion
    }
}