﻿using FeatureFlags.Library;
using NFluent;
using System.Collections.Generic;
using NUnit.Framework;

namespace FeatureExtensionTesting
{
    [TestFixture]
    public class FeatureExtensionsTests
    {
        [Test]
        public void IsFeatureEnablingString_ValidTestString_ReturnsCorrectly()
        {
            //  Arrange
            Dictionary<string, bool> testEnableStrings = new Dictionary<string, bool>()
            {
                {"", false},
                {"enabled", true},
                {"enable", true},
                {"on", true},

                /* JSON objects */
                {"{true}", true },
                {"{enabled}", true },
                {"{\"true\"}", true },

                /* Everything else*/
                {"off", false},
                {"disabled", false},
                {"pretty much anything else", false},
            };

            //  For each item in the test table ... 
            foreach (var item in testEnableStrings)
            {
                //  Act
                var retval = item.Key.IsFeatureCompletelyEnabled();

                //  Assert
                Assert.AreEqual(item.Value, retval);
            }            
        }

        [Test]
        public void ToFeatureFlag_ValidRuleString_ParsesCorrectly()
        {
            //  Arrange
            Dictionary<string, FeatureFlag> testJSONRules = new Dictionary<string, FeatureFlag>()
            {
                {"{true}", new FeatureFlag{ Enabled = true } },
                {"{\"enabled\": true}", new FeatureFlag{ Enabled = true } },
                {"{\"enabled\": \"true\"}", new FeatureFlag{ Enabled = true } },
                {"{\"enabled\": false}", new FeatureFlag{ Enabled = false} },
                {"{\"enabled\": \"false\"}", new FeatureFlag{ Enabled = false} },
                {"{\"percent_loggedin\": 5, \"variant_name\": \"testing\"}", new FeatureFlag{ PercentLoggedIn = 5, VariantName = "testing", Enabled = null } },
            };

            //  For each item in the test table...
            foreach (var item in testJSONRules)
            {
                //  Act
                var retval = item.Key.ToFeatureFlag();

                //  Assert
                Check.That(retval).HasFieldsWithSameValues(item.Value);
            }
        }

        [Test]
        public void ParseFeatureFlag_NullRuleString_ParsesCorrectly()
        {
            //  Arrange
            string jsonString = null;
            FeatureFlag expectedRule = new FeatureFlag();

            //  Act
            var retval = jsonString.ToFeatureFlag();

            //  Assert
            Check.That(retval).HasFieldsWithSameValues(expectedRule);
        }

        [Test]
        public void ToJSON_ValidFeatureFlag_SerializesCorrectly()
        {
            //  Arrange
            Dictionary<string, FeatureFlag> testJSONRules = new Dictionary<string, FeatureFlag>()
            {   
                /* Note the sorting and spacing in the JSON strings... */
                {"{\"enabled\":true}", new FeatureFlag{ Enabled = true } },
                {"{\"admin\":true,\"internal\":true}", new FeatureFlag{ Internal = true, Admin = true } },
                {"{\"users\":[\"testuser\"]}", new FeatureFlag{ Users = new List<string>{ "testuser"} } },
            };

            //  For each item in the test table...
            foreach (var item in testJSONRules)
            {
                //  Act
                var retval = item.Value.ToJSON();

                //  Assert
                Assert.AreEqual(item.Key, retval);
            }
        }
    }
}
