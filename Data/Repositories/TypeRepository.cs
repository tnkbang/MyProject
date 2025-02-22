﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Data.IRepositories;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class TypeRepository<TModel> : ITypeRepository<TModel> where TModel : class
    {
        private readonly PasswordManagerContext _db;

        public TypeRepository(PasswordManagerContext db)
        {
            _db = db;
        }

        public async Task<List<TypePassword>> GetList()
        {
            try
            {
                return await _db.Set<TypePassword>().ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public TypePassword Details(string id)
        {
            try
            {
                var type = _db.TypePasswords.FirstOrDefault(x => x.TypeCode == id);
                return type != null ? type : new TypePassword();
            }
            catch
            {
                throw;
            }
        }

        public void Create(TypePassword type)
        {
            try
            {
                _db.Set<TypePassword>().Add(type);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void Update(TypePassword type)
        {
            try
            {
                _db.Set<TypePassword>().Update(type);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void Delete(TypePassword type)
        {
            try
            {
                _db.Set<TypePassword>().Remove(type);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
