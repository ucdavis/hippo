using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.SlothModels
{
    public class TransactionViewModel
    {
        public TransactionViewModel()
        {
            TransactionDate = DateTime.UtcNow; 
            Transfers = new List<TransferViewModel>();
            AutoApprove = false; 
        }
        public string MerchantTrackingNumber { get; set; }

        public string MerchantTrackingUrl { get; set; }
        public bool AutoApprove { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Source { get; set; } = "Get From Database";
        public string SourceType { get; set; } = "Recharge";

        public string Description { get; set; } //If it isn't set, Sloth with use one of the transfer descriptions...

        public IList<MetadataEntry> Metadata { get; set; } = new List<MetadataEntry>();

        public void AddMetadata(string name, string value)
        {
            Metadata.Add(new MetadataEntry { Name = name, Value = value });
        }

        public class MetadataEntry
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public IList<TransferViewModel> Transfers { get; set; }
    }

    public class TransferViewModel
    {
        public Decimal Amount { get; set; }
        [StringLength(1)]
        public string Chart { get; set; }
        [StringLength(7)]
        public string Account { get; set; }
        [StringLength(5)]
        public string SubAccount { get; set; }
        [StringLength(4)]
        public string ObjectCode { get; set; }
        [StringLength(40)]
        public string Description { get; set; }
        public string FinancialSegmentString { get; set; }

        public string Direction { get; set; }// Debit or Credit Code associated with the transaction. = ['Credit', 'Debit'],

        public class Directions
        {
            public const string Debit = "Debit";
            public const string Credit = "Credit";
        }
    }
}
