using ClinicConnectApi.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicConnectApi.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate }).IsUnique();
            builder.HasIndex(a => new { a.PatientId, a.AppointmentDate }).IsUnique();
            builder.Property(a => a.AppointmentDate)
                .HasConversion(
                    toDb => toDb,
                    fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));
        }
    }
}