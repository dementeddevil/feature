using System.Collections.Generic;
using FeatureFlags.Library;
using NUnit.Framework;

namespace FeatureTesting
{
    [TestFixture]
    public class FeatureTests
    {
        public static IEnumerable<TestCaseData> ValidFlagRulesData
        {
            get
            {
                yield return new TestCaseData(new FeatureFlag { Enabled = true }, true)
                    .SetName("Enabled feature returns true");
                yield return new TestCaseData(new FeatureFlag { Enabled = false }, false)
                    .SetName("Disabled feature returns false");
                yield return new TestCaseData(new FeatureFlag { Admin = true }, true)
                    .SetName("Admin feature returns true");
                yield return new TestCaseData(new FeatureFlag { Internal = true }, true)
                    .SetName("Internal feature returns true");
                yield return new TestCaseData(new FeatureFlag { Users = new List<string> { "iserra", "MReynolds" } }, true)
                    .SetName("User feature including iserra returns true");
                yield return new TestCaseData(new FeatureFlag { Users = new List<string> { "sometestguy", "someothertestguy" } }, false)
                    .SetName("User feature including sometestguy returns false");
                yield return new TestCaseData(new FeatureFlag { Groups = new List<string> { "federation", "someothergroup" } }, false)
                    .SetName("Group feature including federation returns false");
                yield return new TestCaseData(new FeatureFlag { Groups = new List<string> { "travelswithjayne", "browncoats" } }, true)
                    .SetName("Group feature including browncoats returns true");
                yield return new TestCaseData(new FeatureFlag { PercentLoggedIn = 15 }, true) /* iserra is in a bucket that is included */
                    .SetName("Feature for 15 percent logged in users returns true");
                yield return new TestCaseData(new FeatureFlag { PercentLoggedIn = 5 }, false) /* iserra is not in a bucket that is included */
                    .SetName("Feature for 5 percent logged in users returns false");
                yield return new TestCaseData(new FeatureFlag { Users = new List<string> { "iserra", "MReynolds" }, Groups = new List<string> { "federation", "someothergroup" } }, true) /* iserra is still a user that is included */
                    .SetName("Feature for user including iserra and no matching group returns true");
                yield return new TestCaseData(new FeatureFlag { Enabled = true, Users = new List<string> { "sometestguy", "someothertestguy" }, Groups = new List<string> { "federation", "someothergroup" } }, true) /* if we enable it, it's enabled for everybody */
                    .SetName("Feature explicitly enabled but also including non-matching users and groups returns true");
                yield return new TestCaseData(new FeatureFlag { Enabled = false, Users = new List<string> { "iserra" }, Groups = new List<string> { "Browncoats" } }, false) /* if we disable it, it's disabled for everybody */
                    .SetName("Feature explicitly disabled but with matching user and group returns false");
                yield return new TestCaseData(new FeatureFlag { Url = "lassiter", Users = new List<string> { "someothertestguy" }, Groups = new List<string> { "Federation" } }, true) /* If we're at the magic url, it's enabled */
                    .SetName("Feature with matching url returns true");
                yield return new TestCaseData(new FeatureFlag { Url = "jaynestown", Users = new List<string> { "someothertestguy" }, Groups = new List<string> { "Federation" } }, false) /* If we're NOT at the magic url, it's not necessary enabled */
                    .SetName("Feature with non-matching url returns false");
            }
        }

        [Test(Description = "IsEnabled for")]
        [TestCaseSource(typeof(FeatureTests), nameof(ValidFlagRulesData))]
        public void IsEnabledFor_ValidFlagRulesAndInfo_Correct(FeatureFlag featureFlag, bool expected)
        {
            //  Arrange
            string testUser = "iserra"; /* Note that iserra will return a percentage of 13% (given 1000 buckets) */
            string testGroup = "Browncoats";
            string testUrl = "lassiter";
            bool testAdmin = true;
            bool testInternal = true;

            //  Act
            var retval = Feature.IsEnabled(featureFlag, testUser, testGroup, testUrl, testInternal, testAdmin);

            //  Assert
            Assert.AreEqual(expected, retval);
        }

        [Test]
        public void IsEnabled_TestUser_ReturnsAsExpected()
        {
            //  Arrange
            string testUser = "testuser";
            string testGroup = "testgroup";
            string testUrl = "";
            bool testAdmin = false;
            bool testInternal = false;

            Dictionary<FeatureFlag, bool> testRules = new Dictionary<FeatureFlag, bool>()
            {
                {new FeatureFlag{ Users = new List<string>{ "someotheruser", "anotheruser"}, Groups = new List<string>{ "someothergroup"}, Admin = true, Internal = true, Url = "" }, false},
            };

            //  For each item in the test table...
            foreach (var item in testRules)
            {
                //  Act
                var retval = Feature.IsEnabled(item.Key, testUser, testGroup, testUrl, testInternal, testAdmin);

                //  Assert
                Assert.AreEqual(item.Value, retval);
            }
        }

        [Test]
        [TestCase("mreynolds", 787)]
        [TestCase("zwashburne", 987)]
        [TestCase("wwashburne", 746)]
        [TestCase("iserra", 136)]
        [TestCase("jcobb", 33)]
        [TestCase("kfrye", 912)]
        [TestCase("stam", 146)]
        [TestCase("rtam", 147)]
        [TestCase("dbook", 466)]
        [TestCase("", 0)]
        public void GetBucket_ValidItem_ReturnsBucketNumber(string key, int expected)
        {
            //  Arrange

            //  For each item in the test table...
            //  Act
            var retval = Feature.GetBucket(key);

            //  Assert
            Assert.AreEqual(expected, retval);
        }

        [Test]
        public void GetBucket_NullItem_ReturnsBucketNumber()
        {
            //  Arrange
            string itemName = null;
            int expectedBucket = 0;

            //  Act
            var retval = Feature.GetBucket(itemName);

            //  Assert
            Assert.AreEqual(expectedBucket, retval);
        }

        [Test]
        public void GetVariantFor_ValidFlagRulesAndInfo_Correct()
        {
            //  Arrange
            FeatureFlag testFlag = new FeatureFlag()
            {
                Variants = new List<FlagVariant>{
                    new FlagVariant{ Name = "One", Percentage = 15 },
                    new FlagVariant{ Name = "Two", Percentage = 15 },
                    new FlagVariant{ Name = "Three", Percentage = 15 },
                }
            };

            //  Our test table of users and expected variants
            Dictionary<string, string> testUsers = new Dictionary<string, string>()
            {
                {"mreynolds", "None"},
                {"zwashburne", "None"},
                {"wwashburne", "None"},
                {"iserra", "control_2"},
                {"jcobb", "Two"},
                {"kfrye", "None"},
                {"stam", "control_2"},
                {"rtam", "One"},
                {"dbook", "control_2"},
                {"", "control_1"},
            };

            //  For each item in the test table...
            foreach (var item in testUsers)
            {
                //  Act
                var retval = Feature.GetVariantFor(testFlag, item.Key);

                //  Assert
                Assert.AreEqual(item.Value, retval);
            }
        }
    }
}
