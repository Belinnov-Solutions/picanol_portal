using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Picanol.Helpers
{
    public class ConstantsHelper
    {

        public enum SelectPaymentMethod
        {
            Cash,
            Cheque,
            NEFT,
            RTGS,
            Others
        }

        public enum SelectMonths
        {
            January,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        public enum SelectAction
        {
            //Edit,
            RecordPayment,
            PrintInvoice,
            PaymentDetails,
            EmailInvoice,
            CreateCreditNote,
            CreateDebitNote,
            CancelCreditNote

        }

        public enum InvoiceStatus
        {
            Sent,
            Paid,
            Overdue
        }
        public enum PackingTypes
        {
            Small,
            Big
        }

        public enum OrderStatus
        {
            Open,
            Completed,
            InProgress,
            OnHold,
            Dispatched
        }

        public enum OrderProgress
        {
            Open,
            Completed,
            InProgress,
            OnHold

        }
        public enum RepairType
        {
            Chargeable,
            FOC,
            RepairWarranty,
            NoRepairWarranty,
            UnRepairedBoards,
            Loan,
            ReturnLoan
        }

        public enum InvoiceType
        {
            RP,
            SC,
            FOC,
            RW,
            LN,
            PI,
            URD,
            CN,
            DN,
            AC1,
            AC1FOC,
            AC
        }

        public enum UserRoles
        {
            Service_Engineer = 1,
            Field_Engineer = 2
        }
        public enum Actions
        { 
            Edit,
            EditProformaInvoice,
            /*EditInvoice,*/
            //Email,
            //ViewInvoice,
            PreViewInvoice,
            PerformaInvoice,
            //PrintPerformaInvoice,
            PrintOriginalInvoice,
            PrintDuplicateInvoice,
            PrintTriplicateInvoice,
            PrintJobSheet,
            PrintAll,
            ReturnLoan,
            CancelInvoice,
            DispatchDetails
        }
        public enum PartType
        {
            Board,
            Component
        }

        public enum WorkOrderType
        {
            InstallationAndCommissioning,
            ServiceWithinWarranty,
            ServiceOutsidWarranty,
            AllFOC,
        }

        public enum ExpenseType
        {
            ConveyanceAndIncidental,
            Fare
        }
        public enum Invoice
        {
            Original,
            Duplicate,
            Triplicate,
            All
        }

        public enum ActionInitoator
        {
            InvoiceGenerated
            
        }

    }
}