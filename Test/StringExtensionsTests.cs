using System;
using Shouldly;
using Xunit;

namespace Test
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("ssh-rsa AAAAB3NzaC1yc2EAAAABIwAAAQEAkllMqVSsbxNrRFi9wrf+M7Q== some@text.here", true)]
        [InlineData("ssh-ed25519 AAAAB3NzaC1yc2EAAAABIwAAAQEAkllMqVSsbxNrRFi9wrf+M7Q== some@text.here", false)]
        public void GivenAString_WhenValidatingAsSshKey_TheResultShouldBe(string value, bool expected)
        {
            var isValid = value.IsValidSshKey();
            isValid.ShouldBe(expected);
        }
    }
}