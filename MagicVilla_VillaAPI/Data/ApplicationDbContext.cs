using MagicVilla_VillaAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MagicVilla_VillaAPI.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Villa> Villas { get; set; }
}