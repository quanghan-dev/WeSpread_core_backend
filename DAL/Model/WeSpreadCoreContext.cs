using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace DAL.Model
{
    public partial class WeSpreadCoreContext : DbContext
    {
        public WeSpreadCoreContext()
        {
        }

        public WeSpreadCoreContext(DbContextOptions<WeSpreadCoreContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<DonateDetail> DonateDetails { get; set; }
        public virtual DbSet<DonationSession> DonationSessions { get; set; }
        public virtual DbSet<ItemCategory> ItemCategories { get; set; }
        public virtual DbSet<ItemLocation> ItemLocations { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<OrgPaymentAccount> OrgPaymentAccounts { get; set; }
        public virtual DbSet<OrgRepresentative> OrgRepresentatives { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<ParentChildComment> ParentChildComments { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<PersistentLogin> PersistentLogins { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<RecruitmentSession> RecruitmentSessions { get; set; }
        public virtual DbSet<RegistrationForm> RegistrationForms { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserAdmin> UserAdmins { get; set; }
        public virtual DbSet<UserOrganization> UserOrganizations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=171.244.143.202,1433;Database=WeSpreadCore;User ID=sa;Password=wespread");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUser");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Gender)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.NumberPhone)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AppUsers)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ AppUser_Role");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IconUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ParentId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Category_Category");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CommentatorId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Message).IsRequired();

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Commentator)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.CommentatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_ AppUser");

                entity.HasOne(d => d.CommentatorNavigation)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.CommentatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_Organization");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_Project");
            });

            modelBuilder.Entity<DonateDetail>(entity =>
            {
                entity.ToTable("DonateDetail");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DonateTime).HasColumnType("datetime");

                entity.Property(e => e.DonatorId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DonatorType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Message).HasMaxLength(150);

                entity.Property(e => e.PayType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResultMessage)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.SessionId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Donator)
                    .WithMany(p => p.DonateDetailDonators)
                    .HasForeignKey(d => d.DonatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DonateDetail_AppUser");

                entity.HasOne(d => d.PayTypeNavigation)
                    .WithMany(p => p.DonateDetails)
                    .HasForeignKey(d => d.PayType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DonateDetail_PaymentType");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.DonateDetails)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DonateDetail_DonationSession");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DonateDetailUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DonateDetail_AppUser1");
            });

            modelBuilder.Entity<DonationSession>(entity =>
            {
                entity.ToTable("DonationSession");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Checker)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Config).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreationMessage)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CheckerNavigation)
                    .WithMany(p => p.DonationSessionCheckerNavigations)
                    .HasForeignKey(d => d.Checker)
                    .HasConstraintName("FK_DonationSession_AppUser");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.DonationSessionCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DonationSession_AppUser1");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.DonationSessions)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DonationInfo_Project");
            });

            modelBuilder.Entity<ItemCategory>(entity =>
            {
                entity.ToTable("ItemCategory");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CategoryId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrgId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ItemCategory_Category");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("FK_ItemCategory_Organization");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK_ItemCategory_Project");
            });

            modelBuilder.Entity<ItemLocation>(entity =>
            {
                entity.ToTable("ItemLocation");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrgId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.ItemLocations)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ItemCity_City");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.ItemLocations)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("FK_ItemArea_Organization");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ItemLocations)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK_ItemArea_Project");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Location");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Latitude).HasColumnType("decimal(12, 9)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(12, 9)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<OrgPaymentAccount>(entity =>
            {
                entity.HasKey(e => e.OrgId);

                entity.ToTable("OrgPaymentAccount");

                entity.Property(e => e.OrgId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankBranch)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.BankCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CityCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Org)
                    .WithOne(p => p.OrgPaymentAccount)
                    .HasForeignKey<OrgPaymentAccount>(d => d.OrgId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrgPaymentAccount_Organization");
            });

            modelBuilder.Entity<OrgRepresentative>(entity =>
            {
                entity.HasKey(e => e.OrgId);

                entity.ToTable("OrgRepresentative");

                entity.Property(e => e.OrgId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.IdentityCard)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PermanentAddress).HasMaxLength(255);

                entity.Property(e => e.Position)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Org)
                    .WithOne(p => p.OrgRepresentative)
                    .HasForeignKey<OrgRepresentative>(d => d.OrgId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrgRepresentative_Organization");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Checker)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Config).IsUnicode(false);

                entity.Property(e => e.Cover).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreationMessage)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.EngName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FoundingDate).HasColumnType("datetime");

                entity.Property(e => e.Logo).IsUnicode(false);

                entity.Property(e => e.Mission)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.TaxCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Vision)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.HasOne(d => d.CheckerNavigation)
                    .WithMany(p => p.OrganizationCheckerNavigations)
                    .HasForeignKey(d => d.Checker)
                    .HasConstraintName("FK_Organization_AppUser");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.OrganizationCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Organization_AppUser1");
            });

            modelBuilder.Entity<ParentChildComment>(entity =>
            {
                entity.HasKey(e => new { e.ParentCommentId, e.ChildCommentId });

                entity.ToTable("ParentChildComment");

                entity.Property(e => e.ParentCommentId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChildCommentId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ChildComment)
                    .WithMany(p => p.ParentChildCommentChildComments)
                    .HasForeignKey(d => d.ChildCommentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ParentChildComment_Comment1");

                entity.HasOne(d => d.ParentComment)
                    .WithMany(p => p.ParentChildCommentParentComments)
                    .HasForeignKey(d => d.ParentCommentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ParentChildComment_Comment");
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("PaymentMethod");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<PersistentLogin>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_PERSISTENT_LOGIN");

                entity.ToTable("PersistentLogin");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExpirationDate).HasColumnType("datetime");

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.PersistentLogin)
                    .HasForeignKey<PersistentLogin>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PersistentLogin_ AppUser");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Project");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Checker)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Config).IsUnicode(false);

                entity.Property(e => e.Cover)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreationMessage)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EngName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Logo).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.OrgId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CheckerNavigation)
                    .WithMany(p => p.ProjectCheckerNavigations)
                    .HasForeignKey(d => d.Checker)
                    .HasConstraintName("FK_Project_AppUser");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ProjectCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_AppUser1");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(d => d.OrgId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_Organization");
            });

            modelBuilder.Entity<RecruitmentSession>(entity =>
            {
                entity.ToTable("RecruitmentSession");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Benefit).IsRequired();

                entity.Property(e => e.Checker)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Config).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreationMessage)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.JobDescription).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Requirement).IsRequired();

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CheckerNavigation)
                    .WithMany(p => p.RecruitmentSessionCheckerNavigations)
                    .HasForeignKey(d => d.Checker)
                    .HasConstraintName("FK_RecruitmentSession_AppUser");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.RecruitmentSessionCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RecruitmentSession_AppUser1");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.RecruitmentSessions)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistrationInfo_RegistrationInfo");
            });

            modelBuilder.Entity<RegistrationForm>(entity =>
            {
                entity.ToTable("RegistrationForm");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ApplyLocationId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CheckerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreationMessage)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.SessionId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ApplyLocation)
                    .WithMany(p => p.RegistrationForms)
                    .HasForeignKey(d => d.ApplyLocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistrationForm_Location");

                entity.HasOne(d => d.Checker)
                    .WithMany(p => p.RegistrationFormCheckers)
                    .HasForeignKey(d => d.CheckerId)
                    .HasConstraintName("FK__Registrat__Check__6754599E");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.RegistrationForms)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistrationForm_RecruitmentSession");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RegistrationFormUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistrationForm_AppUser");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<UserAdmin>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("UserAdmin");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserAdmin)
                    .HasForeignKey<UserAdmin>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserAdmin_AppUser");
            });

            modelBuilder.Entity<UserOrganization>(entity =>
            {
                entity.ToTable("UserOrganization");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrganizationId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrganization_Organization");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrganization_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrganization_ AppUser");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
