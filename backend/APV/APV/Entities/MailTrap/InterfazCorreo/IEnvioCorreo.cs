namespace APV.Entities.MailTrap.InterfazCorreo
{
    public interface IEnvioCorreo
    {
        Task EmailRegistroAsync(string to, string name, string codigoA);
        Task EmailOlvidePasswordAsync(string to, string name, string codigoA);
    }
}
