using System.ComponentModel.DataAnnotations;

namespace ClinicConnectApi.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PeselAttribute : ValidationAttribute
    {
        private static readonly int[] Weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };
        public PeselAttribute() : base("PESEL must be 11 digits with a valid checksum") { }
        public override bool IsValid(object? value)
        {
            if(value is null)
            {
                return true;
            }
            if(value is not string pesel || pesel.Length != 11 || !pesel.All(char.IsAsciiDigit))
            {
                return false;
            }
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += (pesel[i] - '0') * Weights[i];
            }

            int controlDigit = (10 - sum % 10) % 10;
            return controlDigit == pesel[10] - '0';
        }
    }
}