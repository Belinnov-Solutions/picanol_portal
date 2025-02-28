using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Picanol.Services
{
    public interface iBaseService<T> : IDisposable where T : class
    {
        List<T> Get();
        List<T> Get(Expression<Func<T, bool>> expression);
        T GetSingle(Expression<Func<T, bool>> expression);
        bool IsAttached(T entity);
        void Add(T entity);
        void Delete(Expression<Func<T, bool>> expression);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);
        void Update(T entity);
        void SaveChanges();
    }
    public interface iValidation
    {
        /// <summary>
        /// Add error messages.
        /// </summary>
        /// <param name="key">Name of property.</param>
        /// <param name="errorMessage">Error message</param>
        void AddError(string key, string errorMessage);
        /// <summary>
        /// Return true if no error else false.
        /// </summary>
        bool isValid { get; }

    }
}