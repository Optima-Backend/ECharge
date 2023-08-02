using System;
using ECharge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Infrastructure.Services.DatabaseContext;

	public class Context: DbContext
	{
		public Context(DbContextOptions<Context> dbContextOptions): base(dbContextOptions)
		{
			
		}
		
		public DbSet<Transaction> Transactions { get; set; }
	}

