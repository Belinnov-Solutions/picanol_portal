using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Data;

namespace Picanol.Services
{
    public abstract class BaseService<C, T> : iBaseService<T>
        where C : DbContext
        where T : class
    {
        protected C Context { get; set; }
        protected iValidation ValidationDictionary { get; set; }
        protected DbSet<T> dbset
        {
            get
            {
                return Context.Set<T>();
            }
        }

        protected BaseService(C context, iValidation validationDictionary)
        {
            if (validationDictionary == null)
                throw new ArgumentNullException("validationDictionary");
            if (context == null)
                throw new ArgumentNullException("validationDictionary");
            this.Context = context;
            this.ValidationDictionary = validationDictionary;
        }

        #region IBaseService<T> Members

        public List<T> Get()
        {
            return dbset.ToList();
        }

        public List<T> Get(Expression<Func<T, bool>> expression)
        {
            return dbset.Where(expression).ToList();
        }

        public T GetSingle(Expression<Func<T, bool>> expression)
        {
            return dbset.Where(expression).FirstOrDefault();
        }

        public bool IsAttached(T entity)
        {
            throw new NotImplementedException();
        }
        public virtual void Add(T entity)
        {
            dbset.Add(entity);
            this.SaveChanges();
        }

        public void Delete(Expression<Func<T, bool>> expression)
        {
            var current = GetSingle(expression);
            dbset.Remove(current);
            this.SaveChanges();
        }
        public virtual void Delete(T entity)
        {
            dbset.Remove(entity);
        }
        public virtual void Delete(IEnumerable<T> entities)
        {
            if (entities != null)
            {
                throw new Exception("Null Value");
            }
            foreach (T entity in entities)
            {
                this.Delete(entity);
            }
            this.SaveChanges();
        }

        public virtual void Update(T entity)
        {
            var current = Context.Entry(entity);
            dbset.Attach(entity);
            current.State = EntityState.Modified;
            this.SaveChanges();
        }
        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        #endregion

        #region IDisposable Members
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}