using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rhino.Security.Services;
using Xunit;
using Xunit.Abstractions;

namespace Rhino.Security.Tests
{
    public class SecurityKeyExtractorTests : DatabaseFixture
    {
        public SecurityKeyExtractorTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {}

        [Fact]
        public void ExtractLinq()
        {
            var extractor = new AccountInformationExtractor(session);

            var foundAccount = session.Query<Account>()
                        .Where(ExpressionHelper.GetSecurityKeyExpression<Account>(account.SecurityKey))
                        .First();
            
            Assert.Equal(account.SecurityKey, foundAccount.SecurityKey);
        }

        [Fact]
        public void ExpressionJuggling()
        {
            var securityKeyToSearchFor = account.SecurityKey;

            var objectType = typeof(Account);

            var item = Expression.Parameter(typeof(object), "item");

            // Where(item => item)
            var property = Expression
                .Property(
                    Expression.Convert(item, objectType), 
                    Security.GetSecurityKeyProperty(objectType)
                );
            
            // Where(item => item.SomeProperty.Equals)
            var containsMethod = typeof(Guid).GetMethod("Equals", new[] {typeof(Guid)});

            // What we're searching for (e.g. SomeProperty.Contains("foo"))
            var searchExpression = Expression.Constant(securityKeyToSearchFor, typeof(Guid));
            
            // Call the "Contains" method for the "SomeProperty" with 
            // searchExpression as the constant to compare with
            var methodExpression = Expression.Call(property, containsMethod, searchExpression);
            
            // Create a lambda to use inside the where call
            var lambda = Expression.Lambda<Func<object, bool>>(methodExpression, item);
            
            // Do the query with the expression
            var objects = session
                .Query<object>(objectType.FullName).Where(lambda);

            Assert.NotEmpty(objects);
        }
    }

    
}