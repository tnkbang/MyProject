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
    public class UserRepository<TModel> : IUserRepository<TModel> where TModel : class
    {
        private readonly PasswordManagerContext _db;

        public UserRepository(PasswordManagerContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetList()
        {
            try
            {
                return await _db.Set<User>().ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public User Details(string id)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(x => x.Uid == id);
                return user != null ? user : new User();
            }
            catch
            {
                throw;
            }
        }

        public User Details(string uname, string pass)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(x => x.Username == uname && x.Password == pass);
                return user != null ? user : new User();
            }
            catch
            {
                throw;
            }
        }

        public User GetWithUsername(string uname)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(x => x.Username == uname);
                return user != null ? user : new User();
            }
            catch
            {
                throw;
            }
        }

        public void Create(User user)
        {
            try
            {
                _db.Set<User>().Add(user);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void Update(User user)
        {
            try
            {
                _db.Set<User>().Update(user);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void Delete(User user)
        {
            try
            {
                _db.Set<User>().Remove(user);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
