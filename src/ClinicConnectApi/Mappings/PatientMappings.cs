using System.Linq.Expressions;

using ClinicConnectApi.Dtos;
using ClinicConnectApi.Models;

namespace ClinicConnectApi.Mappings
{
    public static class PatientMappings
    {
        public static readonly Expression<Func<Patient, PatientResponseDto>> ToResponseDto =
            p => new PatientResponseDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Pesel = p.Pesel
            };
    }
}