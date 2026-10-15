using ClinicConnectApi.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicConnectApi.Data.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Specialization).IsRequired().HasMaxLength(100);
            builder.Property(d => d.ConsultationFee).IsRequired().HasPrecision(10, 2);
        }
    }
}