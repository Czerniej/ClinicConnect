using System.Linq.Expressions;

using ClinicConnectApi.Dtos;
using ClinicConnectApi.Models;

namespace ClinicConnectApi.Mappings
{
    public static class DoctorMappings
    {
        public static readonly Expression<Func<Doctor, DoctorResponseDto>> ToResponseDto =
            d => new DoctorResponseDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Specialization = d.Specialization,
                ConsultationFee = d.ConsultationFee
            };
    }
}