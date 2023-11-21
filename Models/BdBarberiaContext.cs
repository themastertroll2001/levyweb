using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Models;

public partial class BdBarberiaContext : DbContext
{
    public BdBarberiaContext()
    {
    }

    public BdBarberiaContext(DbContextOptions<BdBarberiaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Corte> Cortes { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<RegistrosCorte> RegistrosCortes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TcEstatusRole> TcEstatusRoles { get; set; }

    public virtual DbSet<TdRolesUsuario> TdRolesUsuarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1E534E0EC5C");

            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Nombre).HasMaxLength(255);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Categoria)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Categoria__Usuar__7C4F7684");
        });

        modelBuilder.Entity<Corte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cortes__3214EC078BB601EA");

            entity.Property(e => e.NombreCorte).HasMaxLength(255);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__Producto__A430AEA3E2BBA14E");

            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Imagen).HasMaxLength(1000);
            entity.Property(e => e.Nombre).HasMaxLength(255);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK__Productos__Categ__7F2BE32F");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Productos)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Productos__Usuar__00200768");
        });

        modelBuilder.Entity<RegistrosCorte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registro__3214EC07BB2E1604");

            entity.HasIndex(e => e.CorteId, "idx_CorteId");

            entity.HasIndex(e => e.UsuarioId, "idx_UsuarioId");

            entity.Property(e => e.Estado).HasDefaultValueSql("((1))");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.NombreCliente).HasMaxLength(255);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Corte).WithMany(p => p.RegistrosCortes)
                .HasForeignKey(d => d.CorteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registros__Corte__5EBF139D");

            entity.HasOne(d => d.Usuario).WithMany(p => p.RegistrosCortes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registros__Usuar__5FB337D6");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC079FAA9C9E");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TcEstatusRole>(entity =>
        {
            entity.HasKey(e => e.IdTcEstatusRoles).HasName("PK__TC_Estat__135B99E320F15B93");

            entity.ToTable("TC_Estatus_Roles");

            entity.Property(e => e.IdTcEstatusRoles).HasColumnName("id_tc_estatusRoles");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<TdRolesUsuario>(entity =>
        {
            entity.HasKey(e => e.IdRolesTd).HasName("PK__TD_Roles__849739D75D4A0F63");

            entity.ToTable("TD_RolesUsuarios");

            entity.Property(e => e.IdRolesTd).HasColumnName("id_roles_td");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdEstatus).HasColumnName("id_estatus");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Usuario)
                .HasMaxLength(100)
                .HasColumnName("usuario");

            entity.HasOne(d => d.IdEstatusNavigation).WithMany(p => p.TdRolesUsuarios)
                .HasForeignKey(d => d.IdEstatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TD_RolesU__id_es__52593CB8");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.TdRolesUsuarios)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TD_RolesU__id_ro__5165187F");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TdRolesUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TD_RolesU__usuar__5070F446");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC076A9C5B6C");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19AE559F02").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE09A5CD2C1").IsUnique();

            entity.Property(e => e.Contrasena).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.CuentaVerificada).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaTokenRecuperacionContrasena).HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).HasMaxLength(100);
            entity.Property(e => e.NombreUsuario).HasMaxLength(50);
            entity.Property(e => e.RolId).HasDefaultValueSql("((4))");
            entity.Property(e => e.TokenRecuperacionContrasena).HasMaxLength(255);
            entity.Property(e => e.TokenVerificacionCorreo).HasMaxLength(255);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK__Usuarios__RolId__3C69FB99");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas__3214EC073DF4FD79");

            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.NombreCliente).HasMaxLength(255);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Ventas__IdUsuari__04E4BC85");

            entity.HasOne(d => d.Producto).WithMany(p => p.Venta)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK__Ventas__Producto__05D8E0BE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
