﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusCatalog
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class MusCatEntities : DbContext
    {
        public MusCatEntities()
            : base("name=MusCatEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Albums> Albums { get; set; }
        public DbSet<Lineups> Lineups { get; set; }
        public DbSet<Musicians> Musicians { get; set; }
        public DbSet<Performers> Performers { get; set; }
        public DbSet<Songs> Songs { get; set; }
        public DbSet<AlbumsWithEmptySongsView> AlbumsWithEmptySongsView { get; set; }
        public DbSet<AlbumsWithoutTotalTimeView> AlbumsWithoutTotalTimeView { get; set; }
        public DbSet<EmptyAlbumsView> EmptyAlbumsView { get; set; }
    
        public virtual int DeleteByAID(Nullable<int> original_AID)
        {
            var original_AIDParameter = original_AID.HasValue ?
                new ObjectParameter("Original_AID", original_AID) :
                new ObjectParameter("Original_AID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("DeleteByAID", original_AIDParameter);
        }
    
        public virtual int DeleteByPID(Nullable<int> original_PID)
        {
            var original_PIDParameter = original_PID.HasValue ?
                new ObjectParameter("Original_PID", original_PID) :
                new ObjectParameter("Original_PID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("DeleteByPID", original_PIDParameter);
        }
    
        public virtual ObjectResult<SelectBetweenTimes_Result> SelectBetweenTimes(string lOW_TIME, string hIGH_TIME)
        {
            var lOW_TIMEParameter = lOW_TIME != null ?
                new ObjectParameter("LOW_TIME", lOW_TIME) :
                new ObjectParameter("LOW_TIME", typeof(string));
    
            var hIGH_TIMEParameter = hIGH_TIME != null ?
                new ObjectParameter("HIGH_TIME", hIGH_TIME) :
                new ObjectParameter("HIGH_TIME", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SelectBetweenTimes_Result>("SelectBetweenTimes", lOW_TIMEParameter, hIGH_TIMEParameter);
        }
    
        public virtual ObjectResult<SelectByLetter_Result> SelectByLetter(string lETTER)
        {
            var lETTERParameter = lETTER != null ?
                new ObjectParameter("LETTER", lETTER) :
                new ObjectParameter("LETTER", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SelectByLetter_Result>("SelectByLetter", lETTERParameter);
        }
    
        public virtual ObjectResult<SelectOthers_Result> SelectOthers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SelectOthers_Result>("SelectOthers");
        }
    
        public virtual int UpdateAlbumsWithoutTime()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdateAlbumsWithoutTime");
        }
    }
}
