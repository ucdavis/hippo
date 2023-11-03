using System.Linq.Expressions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Core.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<RequestModel> SelectRequestModel(this IQueryable<Request> requests, AppDbContext dbContext)
        {
            return requests.LeftJoin(dbContext.Groups, r => r.Group, g => g.Name, r => new RequestModel
            {
                Id = r.Left.Id,
                Action = r.Left.Action,
                RequesterEmail = r.Left.Requester.Email,
                RequesterName = $"{r.Left.Requester.LastName}, {r.Left.Requester.FirstName}",
                Status = r.Left.Status,
                Cluster = r.Left.Cluster.Name,
                GroupModel = r.Right == null ? null : new GroupModel
                {
                    Id = r.Right.Id,
                    DisplayName = r.Right.DisplayName,
                    Name = r.Right.Name,
                    Admins = r.Right.AdminAccounts
                        .Select(a => new GroupAccountModel
                        {
                            Kerberos = a.Kerberos,
                            Name = a.Name,
                            Email = a.Email
                        }).ToList(),
                }
            });

        }

        /// <summary>
        /// Helper to make left joins a bit more readable
        /// </summary>
        public static IQueryable<TResult> LeftJoin<TLeft, TRight, TKey, TResult>(
            this IQueryable<TLeft> left,
            IQueryable<TRight> right,
            Expression<Func<TLeft, TKey>> outerKeySelector,
            Expression<Func<TRight, TKey>> innerKeySelector,
            Expression<Func<JoinResult<TLeft, TRight>, TResult>> resultSelector)
        {
            var result = left
                .GroupJoin(right, outerKeySelector, innerKeySelector, (left, right) => new { left, right })
                .SelectMany(row => row.right.DefaultIfEmpty(), (row, right) => new JoinResult<TLeft, TRight> { Left = row.left, Right = right })
                .Select(resultSelector);

            return result;
        }

        /// <summary>
        /// Helper to make left joins a bit more readable
        /// </summary>
        public static IQueryable<JoinResult<TLeft, TRight>> LeftJoin<TLeft, TRight, TKey>(
            this IQueryable<TLeft> left,
            IQueryable<TRight> right,
            Expression<Func<TLeft, TKey>> outerKeySelector,
            Expression<Func<TRight, TKey>> innerKeySelector)
        {
            return left.LeftJoin(right, outerKeySelector, innerKeySelector, row => row);
        }

        public class JoinResult<TLeft, TRight>
        {
            public TLeft Left { get; set; }
            public TRight Right { get; set; }
        }
    }
}