using Hippo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Helpers
{
    public class CreateValidEntities
    {
        public static User User(int counter)
        {
            var rtValue = new User();
            rtValue.Id = counter;
            rtValue.FirstName = $"FirstName{counter}";
            rtValue.LastName = $"LastName{counter}";
            rtValue.Email = $"Email{counter}@fake.com";
            rtValue.Iam = $"Iam{counter}";
            rtValue.Kerberos = $"Kerberos{counter}";

            return rtValue;
        }
    }
}
