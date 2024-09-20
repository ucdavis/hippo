using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models
{
    public static class AccessCodes
    {
        public const string SystemAccess = "SystemAccess";
        public const string ClusterAdminAccess  =  "ClusterAdminAccess";
        public const string GroupAdminAccess  =  "GroupAdminAccess";
        public const string FinancialAdminAccess = "FinancialAdminAccess";
        public const string ClusterAdminOrFinancialAdminAccess = "ClusterAdminOrFinancialAdminAccess";
    }
}
