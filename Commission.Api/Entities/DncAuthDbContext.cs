using Commission.Api.Entities.QueryModels.DncPermission;
using Microsoft.EntityFrameworkCore;

namespace Commission.Api.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class DncZeusDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public DncZeusDbContext(DbContextOptions<DncZeusDbContext> options) : base(options)
        {

        }
        /// <summary>
        /// 用户
        /// </summary>
        public DbSet<DncUser> DncUser { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public DbSet<DncRole> DncRole { get; set; }
        /// <summary>
        /// 菜单
        /// </summary>
        public DbSet<DncMenu> DncMenu { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public DbSet<DncIcon> DncIcon { get; set; }

        /// <summary>
        /// 用户-角色多对多映射
        /// </summary>
        public DbSet<DncUserRoleMapping> DncUserRoleMapping { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        public DbSet<DncPermission> DncPermission { get; set; }
        /// <summary>
        /// 角色-权限多对多映射
        /// </summary>
        public DbSet<DncRolePermissionMapping> DncRolePermissionMapping { get; set; }

        #region DbQuery
        /// <summary>
        /// 
        /// </summary>
        public DbQuery<DncPermissionWithAssignProperty> DncPermissionWithAssignProperty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DbQuery<DncPermissionWithMenu> DncPermissionWithMenu { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<DncUser>()
            //    .Property(x => x.Status);
            //modelBuilder.Entity<DncUser>()
            //    .Property(x => x.IsDeleted);


            modelBuilder.Entity<DncRole>(entity =>
            {
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<DncMenu>(entity =>
            {
                //entity.haso
            });


            modelBuilder.Entity<DncUserRoleMapping>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.UserGuid,
                    x.RoleCode
                });

                entity.HasOne(x => x.DncUser)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserGuid)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.DncRole)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DncPermission>(entity =>
            {
                entity.HasIndex(x => x.Code)
                    .IsUnique();

                entity.HasOne(x => x.Menu)
                    .WithMany(x => x.Permissions)
                    .HasForeignKey(x => x.MenuGuid);
            });

            modelBuilder.Entity<DncRolePermissionMapping>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.RoleCode,
                    x.PermissionCode
                });

                entity.HasOne(x => x.DncRole)
                    .WithMany(x => x.Permissions)
                    .HasForeignKey(x => x.RoleCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.DncPermission)
                    .WithMany(x => x.Roles)
                    .HasForeignKey(x => x.PermissionCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //modelBuilder.Entity<UserSalesmanMapping>(entity =>
            //{
            //    entity.HasKey(x => new
            //    {
            //        x.UserId,
            //        x.SalesmanId
            //    });

            //    entity.HasOne(x => x.DncUser)
            //        .WithOne()
            //        .HasForeignKey<UserSalesmanMapping>(x => x.UserId)
            //        .OnDelete(DeleteBehavior.Restrict);

            //    entity.HasOne(x => x.BaseSalesman)
            //        .WithOne()
            //        .HasForeignKey<UserSalesmanMapping>(x => x.SalesmanId)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});

            base.OnModelCreating(modelBuilder);
        }


        public DbSet<BaseCustom> BaseCustom { get; set; }
        public DbSet<BaseSalesman> BaseSalesman { get; set; }
        public DbSet<BaseSoftware> BaseSoftware { get; set; }
        public DbSet<BaseCommisionWay> BaseCommisionWay { get; set; }
        public DbSet<BaseComparison> BaseComparison { get; set; }
        public DbSet<T_Bill> T_Bill { get; set; }
        public DbSet<T_BillEntry> T_BillEntry { get; set; }
        public DbSet<UserSalesmanMapping> UserSalesmanMapping { get; set; }
        public DbSet<vUserSalesmanMapping> vUserSalesmanMapping { get; set; }
        public DbSet<vBillRecord> vBillRecord { get; set; }
        public DbSet<vBillRecordList> vBillRecordList { get; set; }
        public DbSet<T_CalcBill> T_CalcBill { get; set; }
        public DbSet<T_CalcBillEntry> T_CalcBillEntry { get; set; }
        public DbSet<vCalcBillRecord> vCalcBillRecord { get; set; }
        public DbSet<vCustomPoint> vCustomPoint { get; set; }
        public DbSet<vEmployee> vEmployee { get; set; }
    }
}
